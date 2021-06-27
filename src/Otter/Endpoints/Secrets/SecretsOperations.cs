using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Otter.Models;

namespace Otter
{
    internal class SecretsOperations : ISecretsOperations
    {
        private readonly DockerClient _client;

        internal SecretsOperations(DockerClient client)
        {
            _client = client;
        }

        async Task<IList<Secret>> ISecretsOperations.ListAsync(CancellationToken cancellationToken)
        {
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Get, 
                            "secrets",
                            null,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<IList<Secret>>(response.Body);
        }

        async Task<SecretCreateResponse> ISecretsOperations.CreateAsync(SecretSpec body, CancellationToken cancellationToken)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var data = new JsonRequestContent<SecretSpec>(body);

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Post, 
                            "secrets/create", 
                            null, 
                            data, 
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<SecretCreateResponse>(response.Body);
        }

        async Task<Secret> ISecretsOperations.InspectAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Get, 
                            $"secrets/{id}",
                            null,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<Secret>(response.Body);
        }

        Task ISecretsOperations.DeleteAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            return 
                _client.MakeRequestAsync(
                    _client.NoErrorHandlers, 
                    HttpMethod.Delete, 
                    $"secrets/{id}",
                    null,
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }
    }
}