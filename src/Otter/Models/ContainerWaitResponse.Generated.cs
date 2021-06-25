using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainerWaitResponse // (main.ContainerWaitResponse)
    {
        [DataMember(Name = "StatusCode", EmitDefaultValue = false)]
        public long StatusCode { get; set; }
    }
}
