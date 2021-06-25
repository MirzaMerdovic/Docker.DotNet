using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainerExecCreateResponse // (main.ContainerExecCreateResponse)
    {
        [DataMember(Name = "Id", EmitDefaultValue = false)]
        public string ID { get; set; }
    }
}
