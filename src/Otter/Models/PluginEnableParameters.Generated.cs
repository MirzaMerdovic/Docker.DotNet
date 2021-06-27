using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class PluginEnableParameters // (main.PluginEnableParameters)
    {
        [QueryStringParameter("timeout", false)]
        public long? Timeout { get; set; }
    }
}
