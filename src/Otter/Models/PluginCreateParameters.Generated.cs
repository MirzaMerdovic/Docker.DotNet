using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class PluginCreateParameters // (main.PluginCreateParameters)
    {
        [QueryStringParameter("name", true)]
        public string Name { get; set; }
    }
}
