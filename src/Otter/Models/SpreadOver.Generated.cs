using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class SpreadOver // (swarm.SpreadOver)
    {
        [DataMember(Name = "SpreadDescriptor", EmitDefaultValue = false)]
        public string SpreadDescriptor { get; set; }
    }
}
