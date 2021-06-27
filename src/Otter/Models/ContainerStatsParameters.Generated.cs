using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class ContainerStatsParameters // (main.ContainerStatsParameters)
    {
        [QueryStringParameter("stream", true, typeof(BoolQueryStringConverter))]
        public bool Stream { get; set; } = true;
    }
}
