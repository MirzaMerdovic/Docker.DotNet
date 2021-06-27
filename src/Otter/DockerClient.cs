using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Otter.Http.Streaming;
using Otter.Http;
using Otter.Converters.QueryString;

#if (NETSTANDARD1_6 || NETSTANDARD2_0)
using System.Net.Sockets;
#endif

namespace Otter
{
    public sealed class DockerClient : IDockerClient
    {
        private const string USER_AGENT = "Otter";
        private static readonly TimeSpan INFINITE_TIMEOUT = TimeSpan.FromMilliseconds(Timeout.Infinite);

        internal delegate void ApiResponseErrorHandlingDelegate(HttpStatusCode statusCode, string responseBody);

        private readonly HttpClient _client;
        private readonly Uri _endpointBaseUri;
        private readonly Version _requestedApiVersion;

        internal readonly IEnumerable<ApiResponseErrorHandlingDelegate> NoErrorHandlers = Enumerable.Empty<ApiResponseErrorHandlingDelegate>();

        public DockerClientConfiguration Configuration { get; }
        public IContainerOperations Containers => new ContainerOperations(this);
        public IImageOperations Images => new ImageOperations(this);
        public INetworkOperations Networks => new NetworkOperations(this);
        public IVolumeOperations Volumes => new VolumeOperations(this);
        public ISecretsOperations Secrets => new SecretsOperations(this);
        public ISwarmOperations Swarm => new SwarmOperations(this);
        public ITasksOperations Tasks => new TasksOperations(this);
        public ISystemOperations System => new SystemOperations(this);
        public IPluginOperations Plugin => new PluginOperations(this);
        public IExecOperations Exec => new ExecOperations(this);

        public TimeSpan DefaultTimeout { get; set; }

        internal DockerClient(DockerClientConfiguration configuration, Version requestedApiVersion)
        {
            Configuration = configuration;
            _requestedApiVersion = requestedApiVersion;

            ManagedHandler handler;
            var uri = Configuration.EndpointBaseUri;
            switch (uri.Scheme.ToLowerInvariant())
            {
                case "npipe":
                    if (Configuration.Credentials.IsTlsCredentials())
                    {
                        throw new NotSupportedException("TLS not supported over npipe");
                    }

                    var segments = uri.Segments;
                    if (segments.Length != 3 || !segments[1].Equals("pipe/", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException($"{Configuration.EndpointBaseUri} is not a valid npipe URI");
                    }

                    var serverName = uri.Host;
                    if (string.Equals(serverName, "localhost", StringComparison.OrdinalIgnoreCase))
                    {
                        // npipe schemes dont work with npipe://localhost/... and need npipe://./... so fix that for a client here.
                        serverName = ".";
                    }

                    var pipeName = uri.Segments[2];

                    uri = new UriBuilder("http", pipeName).Uri;
                    handler = new ManagedHandler(async (host, port, cancellationToken) =>
                    {
                        int timeout = (int)Configuration.NamedPipeConnectTimeout.TotalMilliseconds;
                        var stream = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                        var dockerStream = new DockerPipeStream(stream);

#if NET45
                        await Task.Run(() => stream.Connect(timeout), cancellationToken);
#else
                        await stream.ConnectAsync(timeout, cancellationToken);
#endif
                        return dockerStream;
                    });

                    break;

                case "tcp":
                case "http":
                    var builder = new UriBuilder(uri)
                    {
                        Scheme = configuration.Credentials.IsTlsCredentials() ? "https" : "http"
                    };
                    uri = builder.Uri;
                    handler = new ManagedHandler();
                    break;

                case "https":
                    handler = new ManagedHandler();
                    break;

#if (NETSTANDARD1_6 || NETSTANDARD2_0)
                case "unix":
                    var pipeString = uri.LocalPath;
                    handler = new ManagedHandler(async (host, port, cancellationToken) =>
                    {
                        var sock = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                        await sock.ConnectAsync(new UnixDomainSocketEndPoint(pipeString));
                        return sock;
                    });
                    uri = new UriBuilder("http", uri.Segments.Last()).Uri;
                    break;
#endif

                default:
                    throw new NotSupportedException($"Unknown URL scheme {configuration.EndpointBaseUri.Scheme}");
            }

            _endpointBaseUri = uri;

            _client = new HttpClient(Configuration.Credentials.GetHandler(handler), true);
            DefaultTimeout = Configuration.DefaultTimeout;
            _client.Timeout = INFINITE_TIMEOUT;
        }

        public void Dispose()
        {
            Configuration.Dispose();
        }

        internal async Task<DockerApiResponse> MakeRequestAsync(
            IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers,
            HttpMethod method,
            string path,
            IQueryString queryString,
            IRequestContent body,
            IDictionary<string, string> headers,
            TimeSpan timeout,
            CancellationToken token)
        {
            var response =
                await
                    PrivateMakeRequestAsync(
                        timeout,
                        HttpCompletionOption.ResponseContentRead,
                        method,
                        path,
                        queryString,
                        headers,
                        body,
                        token)
                    .ConfigureAwait(false);

            using (response)
            {
                await HandleIfErrorResponseAsync(response.StatusCode, response, errorHandlers);

                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return new DockerApiResponse(response.StatusCode, responseBody);
            }
        }

        internal async Task<Stream> MakeRequestForStreamAsync(
            IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers,
            HttpMethod method,
            string path,
            IQueryString queryString,
            IRequestContent body,
            IDictionary<string, string> headers,
            TimeSpan timeout,
            CancellationToken token)
        {
            var response =
                await
                    PrivateMakeRequestAsync(
                        timeout,
                        HttpCompletionOption.ResponseHeadersRead,
                        method,
                        path,
                        queryString,
                        headers,
                        body,
                        token)
                    .ConfigureAwait(false);

            await HandleIfErrorResponseAsync(response.StatusCode, response, errorHandlers);

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        internal async Task<HttpResponseMessage> MakeRequestForRawResponseAsync(
            HttpMethod method,
            string path,
            IQueryString queryString,
            IRequestContent body,
            IDictionary<string, string> headers,
            CancellationToken token)
        {
            var response =
                await
                    PrivateMakeRequestAsync(
                        INFINITE_TIMEOUT,
                        HttpCompletionOption.ResponseHeadersRead,
                        method,
                        path,
                        queryString,
                        headers,
                        body,
                        token)
                    .ConfigureAwait(false);

            return response;
        }

        internal async Task<DockerApiStreamedResponse> MakeRequestForStreamedResponseAsync(
            IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers,
            HttpMethod method,
            string path,
            IQueryString queryString,
            CancellationToken cancellationToken)
        {
            var response =
                await
                    PrivateMakeRequestAsync(
                        INFINITE_TIMEOUT,
                        HttpCompletionOption.ResponseHeadersRead,
                        method,
                        path,
                        queryString,
                        null,
                        null,
                        cancellationToken);

            await HandleIfErrorResponseAsync(response.StatusCode, response, errorHandlers);

            var body = await response.Content.ReadAsStreamAsync();

            return new DockerApiStreamedResponse(response.StatusCode, body, response.Headers);
        }

        internal async Task<WriteClosableStream> MakeRequestForHijackedStreamAsync(
            IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers,
            HttpMethod method,
            string path,
            IQueryString queryString,
            IRequestContent body,
            IDictionary<string, string> headers,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var response =
                await
                    PrivateMakeRequestAsync(
                        timeout,
                        HttpCompletionOption.ResponseHeadersRead,
                        method,
                        path,
                        queryString,
                        headers,
                        body,
                        cancellationToken)
                    .ConfigureAwait(false);

            await HandleIfErrorResponseAsync(response.StatusCode, response, errorHandlers);

            var content = response.Content as HttpConnectionResponseContent;

            _ = content ?? throw new NotSupportedException("message handler does not support hijacked streams");

            return content.HijackStream();
        }

        private async Task<HttpResponseMessage> PrivateMakeRequestAsync(
            TimeSpan timeout,
            HttpCompletionOption completionOption,
            HttpMethod method,
            string path,
            IQueryString queryString,
            IDictionary<string, string> headers,
            IRequestContent data,
            CancellationToken cancellationToken)
        {
            var request = PrepareRequest(method, path, queryString, headers, data);

            if (timeout != INFINITE_TIMEOUT)
            {
                using (var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeoutTokenSource.CancelAfter(timeout);

                    return await _client.SendAsync(request, completionOption, timeoutTokenSource.Token).ConfigureAwait(false);
                }
            }

            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            using (cancellationToken.Register(() => tcs.SetCanceled()))
            {
                var sendTask = _client.SendAsync(request, completionOption, cancellationToken);

                return await await Task.WhenAny(tcs.Task, sendTask).ConfigureAwait(false);
            }
        }

        private async Task HandleIfErrorResponseAsync(
            HttpStatusCode statusCode,
            HttpResponseMessage response,
            IEnumerable<ApiResponseErrorHandlingDelegate> handlers)
        {
            bool isErrorResponse = statusCode < HttpStatusCode.OK || statusCode >= HttpStatusCode.BadRequest;

            string responseBody = null;

            if (isErrorResponse)
            {
                // If it is not an error response, we do not read the response body because the caller may wish to consume it.
                // If it is an error response, we do because there is nothing else going to be done with it anyway and
                // we want to report the response body in the error message as it contains potentially useful info.
                responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            // If no customer handlers just default the response.
            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    handler(statusCode, responseBody);
                }
            }

            // No custom handler was fired. Default the response for generic success/failures.
            if (isErrorResponse)
                throw new DockerApiException(statusCode, responseBody);
        }

        private HttpRequestMessage PrepareRequest(HttpMethod method, string path, IQueryString queryString, IDictionary<string, string> headers, IRequestContent data)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var request = new HttpRequestMessage(method, BuildUri(_endpointBaseUri, _requestedApiVersion, path, queryString))
            {
                Version = new Version(1, 1)
            };

            request.Headers.Add("User-Agent", USER_AGENT);


            foreach (var header in headers ?? Enumerable.Empty<KeyValuePair<string, string>>())
            {
                request.Headers.Add(header.Key, header.Value);
            }

            if (data != null)
            {
                var requestContent = data.GetContent(); // make the call only once.
                request.Content = requestContent;
            }

            return request;
        }

        private static Uri BuildUri(Uri baseUri, Version requestedApiVersion, string path, IQueryString queryString)
        {
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));

            var builder = new UriBuilder(baseUri);

            builder.Path = requestedApiVersion == null ? builder.Path : builder.Path + $"v{requestedApiVersion}/";
            builder.Path = string.IsNullOrWhiteSpace(path) ? builder.Path : builder.Path + path;
            builder.Query = queryString == null ? builder.Query : queryString.GetQueryString();

            return builder.Uri;
        }
    }
}
