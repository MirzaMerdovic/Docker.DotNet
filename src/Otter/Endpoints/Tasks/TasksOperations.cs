using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Otter.Converters.QueryString;
using Otter.Models;

namespace Otter
{
    internal class TasksOperations : ITasksOperations
    {
        private readonly DockerClient _client;

        internal TasksOperations(DockerClient client)
        {
            _client = client;
        }

        Task<IList<TaskResponse>> ITasksOperations.ListAsync(CancellationToken cancellationToken)
        {
            return ((ITasksOperations)this).ListAsync(null, cancellationToken);
        }

        async Task<IList<TaskResponse>> ITasksOperations.ListAsync(TasksListParameters parameters, CancellationToken cancellationToken)
        {
            IQueryString query = null;

            if (parameters != null)
                query = new QueryString<TasksListParameters>(parameters);

            var response = 
                await 
                    _client.MakeRequestAsync(
                        _client.NoErrorHandlers, 
                        HttpMethod.Get, 
                        "tasks", 
                        query,
                        null,
                        null,
                        _client.DefaultTimeout,
                        cancellationToken)
                    .ConfigureAwait(false)
                    ;
            return JsonSerializer.DeserializeObject<IList<TaskResponse>>(response.Body);
        }

        async Task<TaskResponse> ITasksOperations.InspectAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var response =
                await 
                    _client.MakeRequestAsync(
                        _client.NoErrorHandlers, 
                        HttpMethod.Get, 
                        $"tasks/{id}",
                        null,
                        null,
                        null,
                        _client.DefaultTimeout,
                        cancellationToken)
                    .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<TaskResponse>(response.Body);
        }
    }
}