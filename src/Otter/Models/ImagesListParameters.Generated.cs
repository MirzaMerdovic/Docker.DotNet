using System.Collections.Generic;
using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class ImagesListParameters // (main.ImagesListParameters)
    {
        [QueryStringParameter("all", false, typeof(BoolQueryStringConverter))]
        public bool? All { get; set; }

        [QueryStringParameter("filters", false, typeof(MapQueryStringConverter))]
        public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }
    }
}
