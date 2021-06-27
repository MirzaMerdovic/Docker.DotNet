using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class ContainerResizeParameters // (main.ContainerResizeParameters)
    {
        [QueryStringParameter("h", false)]
        public long? Height { get; set; }

        [QueryStringParameter("w", false)]
        public long? Width { get; set; }
    }
}
