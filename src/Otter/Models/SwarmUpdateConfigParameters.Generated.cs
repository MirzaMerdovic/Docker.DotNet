using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class SwarmUpdateConfigParameters // (main.SwarmUpdateConfigParameters)
    {
        [DataMember(Name = "Config", EmitDefaultValue = false)]
        public SwarmConfigSpec Config { get; set; }

        [QueryStringParameter("version", true)]
        public long Version { get; set; }
    }
}
