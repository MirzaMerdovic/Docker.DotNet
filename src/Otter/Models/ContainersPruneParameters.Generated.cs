using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainersPruneParameters // (main.ContainersPruneParameters)
    {
        [QueryStringParameter("filters", false, typeof(MapQueryStringConverter))]
        public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }
    }
}
