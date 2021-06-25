using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class PluginDisableParameters // (main.PluginDisableParameters)
    {
        [QueryStringParameter("force", false, typeof(BoolQueryStringConverter))]
        public bool? Force { get; set; }
    }
}
