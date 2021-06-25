using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class PluginRemoveParameters // (main.PluginRemoveParameters)
    {
        [QueryStringParameter("force", false, typeof(BoolQueryStringConverter))]
        public bool? Force { get; set; }
    }
}
