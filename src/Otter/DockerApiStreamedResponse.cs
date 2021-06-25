using System.IO;
using System.Net;
using System.Net.Http.Headers;

namespace Otter
{
    internal class DockerApiStreamedResponse
    {
        public HttpStatusCode StatusCode { get; private set; }

        public Stream Body { get; private set; }

        public HttpResponseHeaders Headers { get; private set; }

        public DockerApiStreamedResponse(HttpStatusCode statusCode, Stream body, HttpResponseHeaders headers)
        {
            StatusCode = statusCode;
            Body = body;
            Headers = headers;
        }
    }
}
