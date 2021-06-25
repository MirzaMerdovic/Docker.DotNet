using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class NetworkAttachmentSpec // (swarm.NetworkAttachmentSpec)
    {
        [DataMember(Name = "ContainerID", EmitDefaultValue = false)]
        public string ContainerID { get; set; }
    }
}
