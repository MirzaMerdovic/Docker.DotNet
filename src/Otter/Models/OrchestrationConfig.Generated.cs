using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class OrchestrationConfig // (swarm.OrchestrationConfig)
    {
        [DataMember(Name = "TaskHistoryRetentionLimit", EmitDefaultValue = false)]
        public long? TaskHistoryRetentionLimit { get; set; }
    }
}
