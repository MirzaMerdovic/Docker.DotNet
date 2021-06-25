using System.Net.Http;

namespace Otter
{
    internal interface IRequestContent
    {
        HttpContent GetContent();
    }
}