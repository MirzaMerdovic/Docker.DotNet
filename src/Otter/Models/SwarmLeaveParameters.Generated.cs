using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class SwarmLeaveParameters // (main.SwarmLeaveParameters)
    {
        [QueryStringParameter("force", false, typeof(BoolQueryStringConverter))]
        public bool? Force { get; set; }
    }
}
