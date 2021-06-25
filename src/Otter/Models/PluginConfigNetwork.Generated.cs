using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class PluginConfigNetwork // (types.PluginConfigNetwork)
    {
        [DataMember(Name = "Type", EmitDefaultValue = false)]
        public string Type { get; set; }
    }
}
