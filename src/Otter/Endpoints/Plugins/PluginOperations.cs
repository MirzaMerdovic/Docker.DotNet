using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Otter.Converters.QueryString;
using Otter.Models;
using static Otter.DockerClient;

namespace Otter
{
    internal class PluginOperations : IPluginOperations
    {
        internal static readonly ApiResponseErrorHandlingDelegate NoSuchPluginHandler = (statusCode, responseBody) =>
        {
            if (statusCode == HttpStatusCode.NotFound)
            {
                throw new DockerPluginNotFoundException(statusCode, responseBody);
            }
        };

        private readonly DockerClient _client;
        private const string TarContentType = "application/x-tar";

        internal PluginOperations(DockerClient client)
        {
            _client = client;
        }

        public async Task<IList<Plugin>> ListPluginsAsync(PluginListParameters parameters, CancellationToken cancellationToken = default)
        {
            IQueryString queryParameters = parameters == null ? null : new QueryString<PluginListParameters>(parameters);
            
            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Get, 
                            "plugins", 
                            queryParameters, 
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<Plugin[]>(response.Body);
        }

        public async Task<IList<PluginPrivilege>> GetPluginPrivilegesAsync(PluginGetPrivilegeParameters parameters, CancellationToken cancellationToken = default)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            _client.NoErrorHandlers, 
                            HttpMethod.Get, 
                            "plugins/privileges",
                            new QueryString<PluginGetPrivilegeParameters>(parameters), 
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);
            
            return JsonSerializer.DeserializeObject<PluginPrivilege[]>(response.Body);
        }

        public Task InstallPluginAsync(PluginInstallParameters parameters, Func<JSONMessage, Task> progress, CancellationToken cancellationToken = default)
        {
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Privileges == null)
                throw new ArgumentNullException(nameof(parameters.Privileges));

            var data = new JsonRequestContent<IList<PluginPrivilege>>(parameters.Privileges);

            var stream = 
                _client.MakeRequestForStreamAsync(
                    _client.NoErrorHandlers, 
                    HttpMethod.Post, 
                    $"plugins/pull",
                    new QueryString<PluginInstallParameters>(parameters), 
                    data, 
                    null,
                    TimeSpan.FromMilliseconds(Timeout.Infinite),
                    CancellationToken.None);

            return StreamUtil.MonitorStreamForMessagesAsync(stream, cancellationToken, progress);
        }

        public async Task<Plugin> InspectPluginAsync(string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var response = 
                await 
                    _client
                        .MakeRequestAsync(
                            new[] { NoSuchPluginHandler },
                            HttpMethod.Get, 
                            $"plugins/{name}/json",
                            null,
                            null,
                            null,
                            _client.DefaultTimeout,
                            cancellationToken)
                        .ConfigureAwait(false);

            return JsonSerializer.DeserializeObject<Plugin>(response.Body);
        }

        public Task RemovePluginAsync(string name, PluginRemoveParameters parameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            IQueryString queryParameters = parameters == null ? null : new QueryString<PluginRemoveParameters>(parameters);

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchPluginHandler }, 
                    HttpMethod.Delete, 
                    $"plugins/{name}", 
                    queryParameters,
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task EnablePluginAsync(string name, PluginEnableParameters parameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            IQueryString queryParameters = parameters == null ? null : new QueryString<PluginEnableParameters>(parameters);
            
            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchPluginHandler }, 
                    HttpMethod.Post, 
                    $"plugins/{name}/enable", 
                    queryParameters, 
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task DisablePluginAsync(string name, PluginDisableParameters parameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            IQueryString queryParameters = parameters == null ? null : new QueryString<PluginDisableParameters>(parameters);
            
            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchPluginHandler }, 
                    HttpMethod.Post, 
                    $"plugins/{name}/disable", 
                    queryParameters, 
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task UpgradePluginAsync(string name, PluginUpgradeParameters parameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Privileges == null)
                throw new ArgumentNullException(nameof(parameters.Privileges));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchPluginHandler }, 
                    HttpMethod.Post, 
                    $"plugins/{name}/upgrade",
                    new QueryString<PluginUpgradeParameters>(parameters),
                    new JsonRequestContent<IList<PluginPrivilege>>(parameters.Privileges), 
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task CreatePluginAsync(PluginCreateParameters parameters, Stream plugin, CancellationToken cancellationToken = default)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));

            return 
                _client.MakeRequestAsync(
                    _client.NoErrorHandlers, 
                    HttpMethod.Post, 
                    $"plugins/create",
                    new QueryString<PluginCreateParameters>(parameters),
                    new BinaryRequestContent(plugin, TarContentType), 
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task PushPluginAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchPluginHandler }, 
                    HttpMethod.Post, 
                    $"plugins/{name}/push", 
                    null,
                    null,
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }

        public Task ConfigurePluginAsync(string name, PluginConfigureParameters parameters, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _ = parameters.Args ?? throw new ArgumentNullException(nameof(parameters.Args));

            return 
                _client.MakeRequestAsync(
                    new[] { NoSuchPluginHandler }, 
                    HttpMethod.Post, 
                    $"plugins/{name}/set", 
                    null,
                    new JsonRequestContent<IList<string>>(parameters.Args), 
                    null,
                    _client.DefaultTimeout,
                    cancellationToken);
        }
    }
}
