using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class VolumesPruneParameters // (main.VolumesPruneParameters)
    {
        [QueryStringParameter("filters", false, typeof(MapQueryStringConverter))]
        public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }
    }
}
