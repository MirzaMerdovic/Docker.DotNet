using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ShimConfig // (types.ShimConfig)
    {
        [DataMember(Name = "Binary", EmitDefaultValue = false)]
        public string Binary { get; set; }

        [DataMember(Name = "Opts", EmitDefaultValue = false)]
        public object Opts { get; set; }
    }
}
