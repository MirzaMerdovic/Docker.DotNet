using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class PeerInfo // (network.PeerInfo)
    {
        [DataMember(Name = "Name", EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "IP", EmitDefaultValue = false)]
        public string IP { get; set; }
    }
}
