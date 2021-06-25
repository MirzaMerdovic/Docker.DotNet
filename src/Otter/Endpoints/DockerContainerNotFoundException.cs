using System.Net;

namespace Otter
{
    public class DockerContainerNotFoundException : DockerApiException
    {
        public DockerContainerNotFoundException(HttpStatusCode statusCode, string responseBody) 
            : base(statusCode, responseBody)
        {
        }
    }
}