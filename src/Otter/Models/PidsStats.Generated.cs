using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class PidsStats // (types.PidsStats)
    {
        [DataMember(Name = "current", EmitDefaultValue = false)]
        public ulong Current { get; set; }

        [DataMember(Name = "limit", EmitDefaultValue = false)]
        public ulong Limit { get; set; }
    }
}
