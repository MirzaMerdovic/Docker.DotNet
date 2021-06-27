using Otter.Converters.QueryString;
using Otter.Http.Streaming;
using Otter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Otter.DockerClient;

namespace Otter
{
    internal class ContainerOperations : IContainerOperations
    {
        internal static readonly ApiResponseErrorHandlingDelegate NoSuchContainerHandler = (statusCode, responseBody) =>
        {
            if (statusCode == HttpStatusCode.NotFound)
                throw new DockerContainerNotFoundException(statusCode, responseBody);
        };

        private readonly DockerClient _client;

        internal ContainerOperations(DockerClient client)
        {
            _client = client;
        }

        public async Task<IList<ContainerListResponse>> ListContainersAsync(
            ContainersListParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            IQueryString queryParameters = new QueryString<ContainersListParameters>(parameters);
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Get, 
                            "containers/json", 
                            queryParameters,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ContainerListResponse[]>(response.Body);
        }

        public async Task<CreateContainerResponse> CreateContainerAsync(
            CreateContainerParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            IQueryString qs = null;

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (!string.IsNullOrEmpty(parameters.Name))
                qs = new QueryString<CreateContainerParameters>(parameters);

            var data = new JsonRequestContent<CreateContainerParameters>(parameters);
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchContainerHandler }, 
                            HttpMethod.Post, 
                            "containers/create", 
                            qs, 
                            data,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<CreateContainerResponse>(response.Body);
        }

        public async Task<ContainerInspectResponse> InspectContainerAsync(
            string id, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchContainerHandler }, 
                            HttpMethod.Get, 
                            $"containers/{id}/json",
                            null,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ContainerInspectResponse>(response.Body);
        }

        public async Task<ContainerProcessesResponse> ListProcessesAsync(
            string id, 
            ContainerListProcessesParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            IQueryString queryParameters = new QueryString<ContainerListProcessesParameters>(parameters);
            var response =
                await
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchContainerHandler },
                            HttpMethod.Get,
                            $"containers/{id}/top",
                            queryParameters,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ContainerProcessesResponse>(response.Body);
        }

        public Task GetContainerLogsAsync(
            string id, 
            ContainerLogsParameters parameters, 
            CancellationToken cancellationToken, 
            Func<string, Task> progress)
        {
            var stream = 
                _client.MakeRequestForStreamAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Get, 
                    $"containers/{id}/logs",
                    new QueryString<ContainerLogsParameters>(parameters),
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);

            return StreamUtil.MonitorStreamAsync(stream, cancellationToken, progress);
        }

        public async Task<MultiplexedStream> GetContainerLogsAsync(
            string id, 
            bool tty, 
            ContainerLogsParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            IQueryString queryParameters = new QueryString<ContainerLogsParameters>(parameters);

            Stream result = 
                await 
                    _client
                        .MakeRequestForStreamAsync(
                            new[] { NoSuchContainerHandler }, 
                            HttpMethod.Get, 
                            $"containers/{id}/logs", 
                            queryParameters,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);

            return new MultiplexedStream(result, !tty);
        }

        public async Task<IList<ContainerFileSystemChangeResponse>> InspectChangesAsync(
            string id, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchContainerHandler }, 
                            HttpMethod.Get, 
                            $"containers/{id}/changes",
                            null,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ContainerFileSystemChangeResponse[]>(response.Body);
        }

        public Task<Stream> ExportContainerAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            return 
                _client.MakeRequestForStreamAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Get, 
                    $"containers/{id}/export",
                    null,
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task<Stream> GetContainerStatsAsync(
            string id, 
            ContainerStatsParameters parameters, 
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            IQueryString queryParameters = new QueryString<ContainerStatsParameters>(parameters);
            
            return 
                _client.MakeRequestForStreamAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Get, 
                    $"containers/{id}/stats", 
                    queryParameters, 
                    null, 
                    null, 
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task GetContainerStatsAsync(
            string id, 
            ContainerStatsParameters parameters, 
            Func<ContainerStatsResponse, Task> progress, 
            CancellationToken cancellationToken = default)
        {
            var stream = 
                _client.MakeRequestForStreamAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Get, 
                    $"containers/{id}/stats",
                    new QueryString<ContainerStatsParameters>(parameters), 
                    null, 
                    null, 
                    _client.DefaultTimeout,
                    cancellationToken);

            return StreamUtil.MonitorStreamForMessagesAsync(stream, cancellationToken, progress);
        }

        public Task ResizeContainerTtyAsync(
            string id, 
            ContainerResizeParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Post, 
                    $"containers/{id}/resize",
                    new QueryString<ContainerResizeParameters>(parameters),
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public async Task<bool> StartContainerAsync(
            string id, 
            ContainerStartParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var queryParams = parameters == null ? null : new QueryString<ContainerStartParameters>(parameters);
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchContainerHandler }, 
                            HttpMethod.Post, 
                            $"containers/{id}/start", 
                            queryParams,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return response.StatusCode != HttpStatusCode.NotModified;
        }

        public async Task<bool> StopContainerAsync(
            string id, 
            ContainerStopParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            // since specified wait timespan can be greater than HttpClient's default, we set the
            // client timeout to infinite and provide a cancellation token.
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchContainerHandler }, 
                            HttpMethod.Post, 
                            $"containers/{id}/stop",
                            new QueryString<ContainerStopParameters>(parameters), 
                            null, 
                            null, 
                            TimeSpan.FromMilliseconds(Timeout.Infinite), 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return response.StatusCode != HttpStatusCode.NotModified;
        }

        public Task RestartContainerAsync(
            string id, 
            ContainerRestartParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            // since specified wait timespan can be greater than HttpClient's default, we set the
            // client timeout to infinite and provide a cancellation token.
            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Post, 
                    $"containers/{id}/restart",
                    new QueryString<ContainerRestartParameters>(parameters), 
                    null, 
                    null, 
                    TimeSpan.FromMilliseconds(Timeout.Infinite), 
                    cancellationToken);
        }

        public Task KillContainerAsync(
            string id, 
            ContainerKillParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Post, 
                    $"containers/{id}/kill", 
                    new QueryString<ContainerKillParameters>(parameters),
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task RenameContainerAsync(string id, ContainerRenameParameters parameters, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Post, 
                    $"containers/{id}/rename",
                    new QueryString<ContainerRenameParameters>(parameters),
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task PauseContainerAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Post, 
                    $"containers/{id}/pause",
                    null,
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task UnpauseContainerAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Post, 
                    $"containers/{id}/unpause",
                    null,
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public async Task<MultiplexedStream> AttachContainerAsync(
            string id, 
            bool tty, 
            ContainerAttachParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var stream = 
                await 
                    _client
                        .MakeRequestForHijackedStreamAsync(
                            new[] { NoSuchContainerHandler }, 
                            HttpMethod.Post, 
                            $"containers/{id}/attach",
                            new QueryString<ContainerAttachParameters>(parameters), 
                            null, 
                            null, 
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            if (!stream.CanCloseWrite)
            {
                stream.Dispose();
                throw new NotSupportedException("Cannot shutdown write on this transport");
            }

            return new MultiplexedStream(stream, !tty);
        }

        public async Task<ContainerWaitResponse> WaitContainerAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchContainerHandler }, 
                            HttpMethod.Post, 
                            $"containers/{id}/wait", 
                            null, 
                            null, 
                            null, 
                            TimeSpan.FromMilliseconds(Timeout.Infinite),
                            cancellationToken)
                        .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<ContainerWaitResponse>(response.Body);
        }

        public Task RemoveContainerAsync(
            string id, 
            ContainerRemoveParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Delete, 
                    $"containers/{id}",
                    new QueryString<ContainerRemoveParameters>(parameters),
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public async Task<GetArchiveFromContainerResponse> GetArchiveFromContainerAsync(
            string id, 
            GetArchiveFromContainerParameters parameters, 
            bool statOnly, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            IQueryString queryParameters = new QueryString<GetArchiveFromContainerParameters>(parameters);

            var response = 
                await 
                    _client
                        .MakeRequestForStreamedResponseAsync(
                            new[] { NoSuchContainerHandler }, 
                            statOnly ? HttpMethod.Head : HttpMethod.Get, 
                            $"containers/{id}/archive", 
                            queryParameters,
                            cancellationToken)
                        .ConfigureAwait(false);

            var statHeader = response.Headers.GetValues("X-Docker-Container-Path-Stat").First();

            var bytes = Convert.FromBase64String(statHeader);
            var stat = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var pathStat = JsonSerializer.DeserializeObject<ContainerPathStatResponse>(stat);

            return new GetArchiveFromContainerResponse
            {
                Stat = pathStat,
                Stream = statOnly ? null : response.Body
            };
        }

        public Task ExtractArchiveToContainerAsync(
            string id, 
            ContainerPathStatParameters parameters, 
            Stream stream, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var data = new BinaryRequestContent(stream, "application/x-tar");
            
            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchContainerHandler }, 
                    HttpMethod.Put, 
                    $"containers/{id}/archive",
                    new QueryString<ContainerPathStatParameters>(parameters), 
                    data,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public async Task<ContainersPruneResponse> PruneContainersAsync(
            ContainersPruneParameters parameters = null, 
            CancellationToken cancellationToken = default)
        {
            var queryParameters = parameters == null ? null : new QueryString<ContainersPruneParameters>(parameters);
            
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Post, 
                            "containers/prune",
                            queryParameters,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ContainersPruneResponse>(response.Body);
        }

        public async Task<ContainerUpdateResponse> UpdateContainerAsync(
            string id, 
            ContainerUpdateParameters parameters, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var data = new JsonRequestContent<ContainerUpdateParameters>(parameters);
            
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchContainerHandler }, 
                            HttpMethod.Post, 
                            $"containers/{id}/update", 
                            null, 
                            data,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ContainerUpdateResponse>(response.Body);
        }
    }
}