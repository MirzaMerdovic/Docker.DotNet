using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class PluginEnableParameters // (main.PluginEnableParameters)
    {
        [QueryStringParameter("timeout", false)]
        public long? Timeout { get; set; }
    }
}
