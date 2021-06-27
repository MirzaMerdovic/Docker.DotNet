using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Otter.Converters.QueryString;
using Otter.Http.Streaming;
using Otter.Models;
using static Otter.DockerClient;

namespace Otter
{
    internal class ExecOperations : IExecOperations
    {
        internal static readonly ApiResponseErrorHandlingDelegate NoSuchContainerHandler = (statusCode, responseBody) =>
        {
            if (statusCode == HttpStatusCode.NotFound)
                throw new DockerContainerNotFoundException(statusCode, responseBody);
        };

        private readonly DockerClient _client;

        internal ExecOperations(DockerClient client)
        {
            _client = client;
        }

        public async Task<ContainerExecCreateResponse> ExecCreateContainerAsync(string id, ContainerExecCreateParameters parameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var data = new JsonRequestContent<ContainerExecCreateParameters>(parameters);
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchContainerHandler }, 
                            HttpMethod.Post, 
                            $"containers/{id}/exec", 
                            null, 
                            data,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ContainerExecCreateResponse>(response.Body);
        }

        public async Task<ContainerExecInspectResponse> InspectContainerExecAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var response =
                await
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchContainerHandler },
                            HttpMethod.Get,
                            $"exec/{id}/json",
                            null,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<ContainerExecInspectResponse>(response.Body);
        }

        public Task ResizeContainerExecTtyAsync(string id, ContainerResizeParameters parameters, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Post, 
                    $"exec/{id}/resize",
                    new QueryString<ContainerResizeParameters>(parameters),
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        // StartContainerExecAsync will start the process specified by id in detach mode with no connected
        // stdin, stdout, or stderr pipes.
        public Task StartContainerExecAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var parameters = new ContainerExecStartParameters
            {
                Detach = true,
            };

            var data = new JsonRequestContent<ContainerExecStartParameters>(parameters);
            
            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Post, 
                    $"exec/{id}/start", 
                    null, 
                    data,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        // StartAndAttachContainerExecAsync will start the process specified by id with stdin, stdout, stderr
        // connected, and optionally using terminal emulation if tty is true.
        public Task<MultiplexedStream> StartAndAttachContainerExecAsync(string id, bool tty, CancellationToken cancellationToken)
        {
            return 
                StartWithConfigContainerExecAsync(
                    id, 
                    new ContainerExecStartParameters 
                    { 
                        AttachStdin = true, 
                        AttachStderr = true, 
                        AttachStdout = true, 
                        Tty = tty 
                    }, 
                    cancellationToken);
        }

        public async Task<MultiplexedStream> StartWithConfigContainerExecAsync(string id, ContainerExecStartParameters eConfig, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var data = new JsonRequestContent<ContainerExecStartParameters>(eConfig);
            var stream = 
                await 
                    _client
                        .MakeRequestForHijackedStreamAsync(
                            new[] { NoSuchContainerHandler },
                            HttpMethod.Post, 
                            $"exec/{id}/start", 
                            null, 
                            data, 
                            null,
                            TimeSpan.FromMilliseconds(Timeout.Infinite),
                            cancellationToken)
                        .ConfigureAwait(false);
            
            if (!stream.CanCloseWrite)
            {
                stream.Dispose();
                throw new NotSupportedException("Cannot shutdown write on this transport");
            }

            return new MultiplexedStream(stream, !eConfig.Tty);
        }
    }
}
