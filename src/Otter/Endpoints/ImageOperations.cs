using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Otter.Models;
using static Otter.DockerClient;

namespace Otter
{
    internal class ImageOperations : IImageOperations
    {
        private const string RegistryAuthHeaderKey = "X-Registry-Auth";
        private const string TarContentType = "application/x-tar";
        private const string ImportFromBodySource = "-";

        private static readonly Func<string> SerializedEmptyAuthConfig = delegate()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.SerializeObject(new AuthConfig())));
        };

        internal static readonly ApiResponseErrorHandlingDelegate NoSuchImageHandler = (statusCode, responseBody) =>
        {
            if (statusCode == HttpStatusCode.NotFound)
                throw new DockerImageNotFoundException(statusCode, responseBody);
        };

        private readonly DockerClient _client;

        internal ImageOperations(DockerClient client)
        {
            _client = client;
        }

        public async Task<IList<ImagesListResponse>> ListImagesAsync(ImagesListParameters parameters, CancellationToken cancellationToken = default)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Get, 
                            "images/json",
                            new QueryString<ImagesListParameters>(parameters), 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ImagesListResponse[]>(response.Body);
        }

        public Task<Stream> BuildImageFromDockerfileAsync(Stream contents, ImageBuildParameters parameters, CancellationToken cancellationToken = default)
        {
            if (contents == null)
                throw new ArgumentNullException(nameof(contents));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            
            return 
                _client.MakeRequestForStreamAsync(
                    _client.NoErrorHandlers, 
                    HttpMethod.Post, 
                    "build",
                    new QueryString<ImageBuildParameters>(parameters),
                    new BinaryRequestContent(contents, TarContentType), 
                    cancellationToken);
        }

        public Task CreateImageAsync(ImagesCreateParameters parameters, AuthConfig authConfig, Func<JSONMessage, Task> progress, CancellationToken cancellationToken = default)
        {
            return CreateImageAsync(parameters, null, authConfig, progress, cancellationToken);
        }

        public Task CreateImageAsync(ImagesCreateParameters parameters, AuthConfig authConfig, IDictionary<string, string> headers, Func<JSONMessage, Task> progress, CancellationToken cancellationToken = default)
        {
            return CreateImageAsync(parameters, null, authConfig, headers, progress, cancellationToken);
        }

        public Task CreateImageAsync(ImagesCreateParameters parameters, Stream imageStream, AuthConfig authConfig, Func<JSONMessage, Task> progress, CancellationToken cancellationToken = default)
        {
            return CreateImageAsync(parameters, imageStream, authConfig, null, progress, cancellationToken);
        }
        
        public Task CreateImageAsync(ImagesCreateParameters parameters, Stream imageStream, AuthConfig authConfig, IDictionary<string, string> headers, Func<JSONMessage, Task> progress, CancellationToken cancellationToken = default)
        {
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));

            HttpMethod httpMethod = HttpMethod.Post;
            BinaryRequestContent content = null;

            if (imageStream != null)
            {
                content = new BinaryRequestContent(imageStream, TarContentType);
                parameters.FromSrc = ImportFromBodySource;
            }

            IQueryString queryParameters = new QueryString<ImagesCreateParameters>(parameters);
            
            Dictionary<string, string> customHeaders = RegistryAuthHeaders(authConfig);

            if(headers != null)
            {
                foreach(string key in headers.Keys)
                {
                    customHeaders[key] = headers[key];
                }
            }

            return StreamUtil.MonitorResponseForMessagesAsync(
                _client.MakeRequestForRawResponseAsync(httpMethod,
                "images/create", queryParameters, content, customHeaders, cancellationToken),
                cancellationToken,
                progress);
        }

        public async Task<ImageInspectResponse> InspectImageAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchImageHandler }, 
                            HttpMethod.Get, 
                            $"images/{name}/json", 
                            cancellationToken)
                        .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<ImageInspectResponse>(response.Body);
        }

        public async Task<IList<ImageHistoryResponse>> GetImageHistoryAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchImageHandler },
                            HttpMethod.Get, 
                            $"images/{name}/history", 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ImageHistoryResponse[]>(response.Body);
        }

        public Task PushImageAsync(string name, ImagePushParameters parameters, AuthConfig authConfig, Func<JSONMessage, Task> progress, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var stream =
                _client.MakeRequestForStreamAsync(
                    _client.NoErrorHandlers,
                    HttpMethod.Post,
                    $"images/{name}/push",
                    new QueryString<ImagePushParameters>(parameters),
                    null,
                    RegistryAuthHeaders(authConfig), CancellationToken.None);

            return StreamUtil.MonitorStreamForMessagesAsync(stream, cancellationToken, progress);
        }

        public Task TagImageAsync(string name, ImageTagParameters parameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchImageHandler }, 
                    HttpMethod.Post, 
                    $"images/{name}/tag",
                    new QueryString<ImageTagParameters>(parameters), 
                    cancellationToken);
        }

        public async Task<IList<IDictionary<string, string>>> DeleteImageAsync(string name, ImageDeleteParameters parameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var response = 
                await
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchImageHandler }, 
                            HttpMethod.Delete, 
                            $"images/{name}",
                            new QueryString<ImageDeleteParameters>(parameters), 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<Dictionary<string, string>[]>(response.Body);
        }

        public async Task<IList<ImageSearchResponse>> SearchImagesAsync(ImagesSearchParameters parameters, CancellationToken cancellationToken = default)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Get, 
                            "images/search",
                            new QueryString<ImagesSearchParameters>(parameters), 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ImageSearchResponse[]>(response.Body);
        }

        public async Task<ImagesPruneResponse> PruneImagesAsync(
            ImagesPruneParameters parameters, 
            CancellationToken cancellationToken)
        {
            var queryParameters = parameters == null ? null : new QueryString<ImagesPruneParameters>(parameters);
            
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Post, 
                            "images/prune", 
                            queryParameters, 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ImagesPruneResponse>(response.Body);
        }

        public async Task<CommitContainerChangesResponse> CommitContainerChangesAsync(CommitContainerChangesParameters parameters, CancellationToken cancellationToken = default)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var data = parameters.Config == null
                ? null
                : new JsonRequestContent<Config>(parameters.Config);

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Post, 
                            "commit", 
                            new QueryString<CommitContainerChangesParameters>(parameters), 
                            data, 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<CommitContainerChangesResponse>(response.Body);
        }

        public Task<Stream> SaveImageAsync(string name, CancellationToken cancellationToken = default)
        {
            return SaveImagesAsync(new[] { name }, cancellationToken);
        }

        public Task<Stream> SaveImagesAsync(string[] names, CancellationToken cancellationToken = default)
        {
            EnumerableQueryString queryString = null;

            if (names?.Length > 0)
                queryString = new EnumerableQueryString("names", names);

            return 
                _client.MakeRequestForStreamAsync(
                    new[] { NoSuchImageHandler },
                    HttpMethod.Get, 
                    "images/get", 
                    queryString, 
                    cancellationToken);
        }

        public Task LoadImageAsync(ImageLoadParameters parameters, Stream imageStream, Func<JSONMessage, Task> progress, CancellationToken cancellationToken = default)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            var stream = 
                _client.MakeRequestForStreamAsync(
                    _client.NoErrorHandlers, 
                    HttpMethod.Post, 
                    "images/load",
                    new QueryString<ImageLoadParameters>(parameters),
                    new BinaryRequestContent(imageStream, TarContentType), 
                    cancellationToken);

            return StreamUtil.MonitorStreamForMessagesAsync(stream, cancellationToken, progress);
        }

        private Dictionary<string, string> RegistryAuthHeaders(AuthConfig authConfig)
        {
            var serializedAuthConfig =
                authConfig == null
                    ? SerializedEmptyAuthConfig()
                    : Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.SerializeObject(authConfig)));

            return new Dictionary<string, string>
            {
                [RegistryAuthHeaderKey] = serializedAuthConfig        
            };
        }
    }
}