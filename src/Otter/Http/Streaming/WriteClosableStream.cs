using System.IO;

namespace Otter.Http.Streaming
{
    public abstract class WriteClosableStream : Stream
    {
        public abstract bool CanCloseWrite { get; }

        public abstract void CloseWrite();
    }
}