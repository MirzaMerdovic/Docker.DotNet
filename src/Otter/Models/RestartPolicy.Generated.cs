using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class RestartPolicy // (container.RestartPolicy)
    {
        [DataMember(Name = "Name", EmitDefaultValue = false)]
        public RestartPolicyKind Name { get; set; }

        [DataMember(Name = "MaximumRetryCount", EmitDefaultValue = false)]
        public long MaximumRetryCount { get; set; }
    }
}
