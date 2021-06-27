using System.Collections.Generic;
using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class ServiceListParameters // (main.ServiceListParameters)
    {
        [QueryStringParameter("filters", false, typeof(MapQueryStringConverter))]
        public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }

        [QueryStringParameter("status", false, typeof(BoolQueryStringConverter))]
        public bool? Status { get; set; }
    }
}
