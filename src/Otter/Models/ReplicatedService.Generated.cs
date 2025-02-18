using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ReplicatedService // (swarm.ReplicatedService)
    {
        [DataMember(Name = "Replicas", EmitDefaultValue = false)]
        public ulong? Replicas { get; set; }
    }
}
