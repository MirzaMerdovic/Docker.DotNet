using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class SwarmCreateConfigParameters // (main.SwarmCreateConfigParameters)
    {
        [DataMember(Name = "Config", EmitDefaultValue = false)]
        public SwarmConfigSpec Config { get; set; }
    }
}
