using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class SwarmCreateConfigResponse // (main.SwarmCreateConfigResponse)
    {
        [DataMember(Name = "ID", EmitDefaultValue = false)]
        public string ID { get; set; }
    }
}
