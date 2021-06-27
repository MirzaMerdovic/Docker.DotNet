using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class NodeRemoveParameters
    {
        [QueryStringParameter("force", false)]
        public bool Force { get; set; }
    }
}