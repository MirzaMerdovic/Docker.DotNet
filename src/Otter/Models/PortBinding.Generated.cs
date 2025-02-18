using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class PortBinding // (nat.PortBinding)
    {
        [DataMember(Name = "HostIp", EmitDefaultValue = false)]
        public string HostIP { get; set; }

        [DataMember(Name = "HostPort", EmitDefaultValue = false)]
        public string HostPort { get; set; }
    }
}
