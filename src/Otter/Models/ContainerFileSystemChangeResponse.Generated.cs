using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainerFileSystemChangeResponse // (container.ContainerChangeResponseItem)
    {
        [DataMember(Name = "Kind", EmitDefaultValue = false)]
        public FileSystemChangeKind Kind { get; set; }

        [DataMember(Name = "Path", EmitDefaultValue = false)]
        public string Path { get; set; }
    }
}
