using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Otter.Models;

namespace Otter
{
    internal class VolumeOperations : IVolumeOperations
    {
        private readonly DockerClient _client;

        internal VolumeOperations(DockerClient client)
        {
            _client = client;
        }

        async Task<VolumesListResponse> IVolumeOperations.ListAsync(CancellationToken cancellationToken)
        {
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Get, 
                            "volumes", 
                            cancellationToken)
                        .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<VolumesListResponse>(response.Body);
        }

        async Task<VolumeResponse> IVolumeOperations.CreateAsync(VolumesCreateParameters parameters, CancellationToken cancellationToken)
        {
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));

            var data = new JsonRequestContent<VolumesCreateParameters>(parameters);

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Post, 
                            "volumes/create",
                            null, 
                            data, 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<VolumeResponse>(response.Body);
        }

        async Task<VolumeResponse> IVolumeOperations.InspectAsync(string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Get, 
                            $"volumes/{name}", 
                            cancellationToken)
                        .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<VolumeResponse>(response.Body);
        }

        Task IVolumeOperations.RemoveAsync(string name, bool? force, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            
            return _client.MakeRequestAsync(_client.NoErrorHandlers, HttpMethod.Delete, $"volumes/{name}", cancellationToken);
        }

        async Task<VolumesPruneResponse> IVolumeOperations.PruneAsync(VolumesPruneParameters parameters, CancellationToken cancellationToken)
        {
            var queryParameters = parameters == null ? null : new QueryString<VolumesPruneParameters>(parameters);
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Post, 
                            $"volumes/prune", 
                            queryParameters, 
                            cancellationToken)
                        .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<VolumesPruneResponse>(response.Body);
        }
    }
}