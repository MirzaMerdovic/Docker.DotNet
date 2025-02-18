using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class Commit // (types.Commit)
    {
        [DataMember(Name = "ID", EmitDefaultValue = false)]
        public string ID { get; set; }

        [DataMember(Name = "Expected", EmitDefaultValue = false)]
        public string Expected { get; set; }
    }
}
