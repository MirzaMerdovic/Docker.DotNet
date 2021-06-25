using System.IO;

namespace Otter.Models
{
    public class GetArchiveFromContainerResponse
    {
        public ContainerPathStatResponse Stat { get; set; }

        public Stream Stream { get; set; }
    }
}
