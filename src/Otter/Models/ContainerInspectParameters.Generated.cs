using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainerInspectParameters // (main.ContainerInspectParameters)
    {
        [QueryStringParameter("size", false, typeof(BoolQueryStringConverter))]
        public bool? IncludeSize { get; set; }
    }
}
