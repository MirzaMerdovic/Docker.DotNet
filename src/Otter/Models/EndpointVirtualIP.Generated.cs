using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class EndpointVirtualIP // (swarm.EndpointVirtualIP)
    {
        [DataMember(Name = "NetworkID", EmitDefaultValue = false)]
        public string NetworkID { get; set; }

        [DataMember(Name = "Addr", EmitDefaultValue = false)]
        public string Addr { get; set; }
    }
}
