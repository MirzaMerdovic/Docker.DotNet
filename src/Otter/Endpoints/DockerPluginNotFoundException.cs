using System.Net;

namespace Otter
{
    public class DockerPluginNotFoundException : DockerApiException
    {
        public DockerPluginNotFoundException(HttpStatusCode statusCode, string responseBody) 
            : base(statusCode, responseBody)
        {
        }
    }
}