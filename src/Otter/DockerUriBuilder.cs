using System;
using Otter.Converters.QueryString;

namespace Otter
{
    internal static class DockerUriBuilder
    {
        public static Uri BuildUri(Uri baseUri, Version requestedApiVersion, string path, IQueryString queryString)
        {
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));

            var builder = new UriBuilder(baseUri);

            if (requestedApiVersion != null)
                builder.Path += $"v{requestedApiVersion}/";

            if (!string.IsNullOrWhiteSpace(path))
                builder.Path += path;

            if (queryString != null)
                builder.Query = queryString.GetQueryString();

            return builder.Uri;
        }
    }
}