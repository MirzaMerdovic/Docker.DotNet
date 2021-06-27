using System;
using System.Net.Http;

namespace Otter
{
    public abstract class Credentials : IDisposable
    {
        public abstract bool IsTlsCredentials();

        public abstract HttpMessageHandler GetHandler(HttpMessageHandler innerHandler);

        public virtual void Dispose()
        {
        }
    }
}