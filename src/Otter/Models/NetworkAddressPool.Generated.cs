using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class NetworkAddressPool // (types.NetworkAddressPool)
    {
        [DataMember(Name = "Base", EmitDefaultValue = false)]
        public string Base { get; set; }

        [DataMember(Name = "Size", EmitDefaultValue = false)]
        public long Size { get; set; }
    }
}
