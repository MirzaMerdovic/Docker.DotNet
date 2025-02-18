using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ReplicatedJob // (swarm.ReplicatedJob)
    {
        [DataMember(Name = "MaxConcurrent", EmitDefaultValue = false)]
        public ulong? MaxConcurrent { get; set; }

        [DataMember(Name = "TotalCompletions", EmitDefaultValue = false)]
        public ulong? TotalCompletions { get; set; }
    }
}
