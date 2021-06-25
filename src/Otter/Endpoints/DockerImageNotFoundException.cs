using System.Net;

namespace Otter
{
    public class DockerImageNotFoundException : DockerApiException
    {
        public DockerImageNotFoundException(HttpStatusCode statusCode, string body) 
            : base(statusCode, body)
        {
        }
    }
}