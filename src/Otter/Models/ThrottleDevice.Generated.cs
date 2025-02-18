using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ThrottleDevice // (blkiodev.ThrottleDevice)
    {
        [DataMember(Name = "Path", EmitDefaultValue = false)]
        public string Path { get; set; }

        [DataMember(Name = "Rate", EmitDefaultValue = false)]
        public ulong Rate { get; set; }
    }
}
