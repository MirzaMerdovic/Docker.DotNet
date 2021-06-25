using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using static Docker.DotNet.DockerClient;

namespace Docker.DotNet
{
    internal class SwarmOperations : ISwarmOperations
    {
        internal static readonly ApiResponseErrorHandlingDelegate SwarmResponseHandler = (statusCode, responseBody) =>
        {
            if (statusCode == HttpStatusCode.ServiceUnavailable)
                throw new NotSupportedException("Node is not part of a swarm.");
        };

        private readonly DockerClient _client;

        internal SwarmOperations(DockerClient client)
        {
            _client = client;
        }

        async Task<ServiceCreateResponse> ISwarmOperations.CreateServiceAsync(ServiceCreateParameters parameters, CancellationToken cancellationToken)
        {
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _ = parameters.Service ?? throw new ArgumentNullException(nameof(parameters.Service));

            var data = new JsonRequestContent<ServiceSpec>(parameters.Service);

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { SwarmResponseHandler }, 
                            HttpMethod.Post, 
                            "services/create", 
                            null, 
                            data, 
                            RegistryAuthHeaders(parameters.RegistryAuth), 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ServiceCreateResponse>(response.Body);
        }

        async Task<SwarmUnlockResponse> ISwarmOperations.GetSwarmUnlockKeyAsync(CancellationToken cancellationToken)
        {
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { SwarmResponseHandler }, 
                            HttpMethod.Get, 
                            "swarm/unlockkey", 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<SwarmUnlockResponse>(response.Body);
        }

        async Task<string> ISwarmOperations.InitSwarmAsync(SwarmInitParameters parameters, CancellationToken cancellationToken)
        {
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));

            var data = new JsonRequestContent<SwarmInitParameters>(parameters);

            var response = 
                await 
                    _client.MakeRequestAsync(
                        new ApiResponseErrorHandlingDelegate[]
                            {
                                (statusCode, responseBody) =>
                                {
                                    if (statusCode == HttpStatusCode.NotAcceptable)
                                        throw new NotSupportedException("Node is already part of a swarm.");
                                }
                            },
                        HttpMethod.Post,
                        "swarm/init",
                        null,
                        data,
                        cancellationToken)
                    .ConfigureAwait(false);

            return response.Body;
        }

        async Task<SwarmService> ISwarmOperations.InspectServiceAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { SwarmResponseHandler }, 
                            HttpMethod.Get, 
                            $"services/{id}", 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<SwarmService>(response.Body);
        }

        async Task<SwarmInspectResponse> ISwarmOperations.InspectSwarmAsync(CancellationToken cancellationToken)
        {
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { SwarmResponseHandler }, 
                            HttpMethod.Get, 
                            "swarm", 
                            cancellationToken)
                        .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<SwarmInspectResponse>(response.Body);
        }

        async Task ISwarmOperations.JoinSwarmAsync(SwarmJoinParameters parameters, CancellationToken cancellationToken)
        {
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));

            var data = new JsonRequestContent<SwarmJoinParameters>(parameters);
            
            await 
                _client.MakeRequestAsync(
                    new ApiResponseErrorHandlingDelegate[]
                    {
                        (statusCode, responseBody) =>
                        {
                            if (statusCode == HttpStatusCode.ServiceUnavailable)
                                throw new NotSupportedException("Node is already part of a swarm.");
                        }
                    },
                    HttpMethod.Post,
                    "swarm/join",
                    null,
                    data,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        async Task ISwarmOperations.LeaveSwarmAsync(SwarmLeaveParameters parameters, CancellationToken cancellationToken)
        {
            var query = parameters == null ? null : new QueryString<SwarmLeaveParameters>(parameters);
            
            await 
                _client.MakeRequestAsync(
                    new ApiResponseErrorHandlingDelegate[]
                        {
                            (statusCode, responseBody) =>
                            {
                                if (statusCode == HttpStatusCode.ServiceUnavailable)
                                    throw new NotSupportedException("Node is not part of a swarm.");
                            }
                        },
                    HttpMethod.Post,
                    "swarm/leave",
                    query,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        async Task<IEnumerable<SwarmService>> ISwarmOperations.ListServicesAsync(ServicesListParameters parameters, CancellationToken cancellationToken)
        {
            var queryParameters = parameters != null ? new QueryString<ServicesListParameters>(parameters) : null;
            var response = 
                await _client
                    .MakeRequestAsync(
                        new[] { SwarmResponseHandler }, 
                        HttpMethod.Get, 
                        $"services", 
                        queryParameters, 
                        cancellationToken)
                .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<SwarmService[]>(response.Body);
        }

        async Task ISwarmOperations.RemoveServiceAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id)) 
                throw new ArgumentNullException(nameof(id));

            await 
                _client
                    .MakeRequestAsync(
                        new[] { SwarmResponseHandler }, 
                        HttpMethod.Delete, 
                        $"services/{id}", 
                        cancellationToken)
                    .ConfigureAwait(false);
        }

        async Task ISwarmOperations.UnlockSwarmAsync(SwarmUnlockParameters parameters, CancellationToken cancellationToken)
        {
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));

            var body = new JsonRequestContent<SwarmUnlockParameters>(parameters);
            
            await 
                _client
                    .MakeRequestAsync(
                        new[] { SwarmResponseHandler }, 
                        HttpMethod.Post, 
                        "swarm/unlock", 
                        null, 
                        body, 
                        cancellationToken)
                    .ConfigureAwait(false);
        }

        async Task<ServiceUpdateResponse> ISwarmOperations.UpdateServiceAsync(string id, ServiceUpdateParameters parameters, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id)) 
                throw new ArgumentNullException(nameof(id));
            
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _ = parameters.Service ?? throw new ArgumentNullException(nameof(parameters.Service));

            var query = new QueryString<ServiceUpdateParameters>(parameters);
            var body = new JsonRequestContent<ServiceSpec>(parameters.Service);

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { SwarmResponseHandler }, 
                            HttpMethod.Post, 
                            $"services/{id}/update", 
                            query, 
                            body, 
                            RegistryAuthHeaders(parameters.RegistryAuth), 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<ServiceUpdateResponse>(response.Body);
        }

       async Task ISwarmOperations.UpdateSwarmAsync(SwarmUpdateParameters parameters, CancellationToken cancellationToken)
        {
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _ = parameters.Spec ?? throw new ArgumentNullException(nameof(parameters.Spec));

            var query = new QueryString<SwarmUpdateParameters>(parameters);
            var body = new JsonRequestContent<Spec>(parameters.Spec);
            
            await 
                _client.MakeRequestAsync(
                    new ApiResponseErrorHandlingDelegate[]
                        {
                            (statusCode, responseBody) =>
                            {
                                if (statusCode == HttpStatusCode.ServiceUnavailable)
                                    throw new NotSupportedException("Node is not part of a swarm.");
                            }
                        },
                    HttpMethod.Post,
                    "swarm/update",
                    query,
                    body,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        private IDictionary<string, string> RegistryAuthHeaders(AuthConfig authConfig)
        {
            if (authConfig == null)
                return new Dictionary<string, string>();

            return new Dictionary<string, string>
            {
                {
                    "X-Registry-Auth",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.SerializeObject(authConfig)))
                }
            };
        }

        async Task<IEnumerable<NodeListResponse>> ISwarmOperations.ListNodesAsync(CancellationToken cancellationToken)
        {
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { SwarmResponseHandler }, 
                            HttpMethod.Get, 
                            $"nodes", 
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<NodeListResponse[]>(response.Body);
        }

        async Task<NodeListResponse> ISwarmOperations.InspectNodeAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id)) 
                throw new ArgumentNullException(nameof(id));

            var response = 
                await 
                    _client.MakeRequestAsync(
                        new[] { SwarmResponseHandler }, 
                        HttpMethod.Get, 
                        $"nodes/{id}", 
                        cancellationToken)
                    .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<NodeListResponse>(response.Body);
        }

        async Task ISwarmOperations.RemoveNodeAsync(string id, bool force, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id)) 
                throw new ArgumentNullException(nameof(id));

            await 
                _client
                    .MakeRequestAsync(
                        new[] { SwarmResponseHandler }, 
                        HttpMethod.Delete, 
                        $"nodes/{id}",
                        new QueryString<NodeRemoveParameters>(new NodeRemoveParameters { Force = force }), 
                        cancellationToken)
                    .ConfigureAwait(false);
        }

        async Task ISwarmOperations.UpdateNodeAsync(string id, ulong version, NodeUpdateParameters parameters, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));

            var query = new EnumerableQueryString("version", new[] { version.ToString() });
            var body = new JsonRequestContent<NodeUpdateParameters>(parameters);
            
            await 
                _client
                    .MakeRequestAsync(
                        new[] { SwarmResponseHandler }, 
                        HttpMethod.Post, 
                        $"nodes/{id}/update", 
                        query, 
                        body, 
                        cancellationToken)
                    .ConfigureAwait(false);
        }
    }
}