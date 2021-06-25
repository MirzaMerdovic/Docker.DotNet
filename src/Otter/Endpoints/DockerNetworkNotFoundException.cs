using System.Net;

namespace Otter
{
    public class DockerNetworkNotFoundException : DockerApiException
    {
        public DockerNetworkNotFoundException(HttpStatusCode statusCode, string responseBody) 
            : base(statusCode, responseBody)
        {
        }
    }
}