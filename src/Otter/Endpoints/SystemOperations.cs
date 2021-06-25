using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Otter.Models;

namespace Otter
{
    internal class SystemOperations : ISystemOperations
    {
        private readonly DockerClient _client;

        internal SystemOperations(DockerClient client)
        {
            _client = client;
        }

        public Task AuthenticateAsync(AuthConfig authConfig, CancellationToken cancellationToken = default)
        {
            _ = authConfig ?? throw new ArgumentNullException(nameof(authConfig));

            var data = new JsonRequestContent<AuthConfig>(authConfig);

            return _client.MakeRequestAsync(_client.NoErrorHandlers, HttpMethod.Post, "auth", null, data, cancellationToken);
        }

        public async Task<VersionResponse> GetVersionAsync(CancellationToken cancellationToken = default)
        {
            var response = 
                await 
                    _client
                        .MakeRequestAsync(_client.NoErrorHandlers, HttpMethod.Get, "version", cancellationToken)
                        .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<VersionResponse>(response.Body);
        }

        public Task PingAsync(CancellationToken cancellationToken = default)
        {
            return _client.MakeRequestAsync(_client.NoErrorHandlers, HttpMethod.Get, "_ping", cancellationToken);
        }

        public async Task<SystemInfoResponse> GetSystemInfoAsync(CancellationToken cancellationToken = default)
        {
            var response = 
                await 
                    _client
                        .MakeRequestAsync(_client.NoErrorHandlers, HttpMethod.Get, "info", cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<SystemInfoResponse>(response.Body);
        }

        public Task MonitorEventsAsync(
            ContainerEventsParameters parameters,
            Func<Message, Task> progress,
            CancellationToken cancellationToken = default)
        {
            _ = progress ?? throw new ArgumentNullException(nameof(progress));

            var stream = 
                _client.MakeRequestForStreamAsync(
                    _client.NoErrorHandlers, 
                    HttpMethod.Get, 
                    "events",
                    new QueryString<ContainerEventsParameters>(parameters), 
                    cancellationToken);

            return StreamUtil.MonitorStreamForMessagesAsync(stream, cancellationToken, progress);
        }
    }
}