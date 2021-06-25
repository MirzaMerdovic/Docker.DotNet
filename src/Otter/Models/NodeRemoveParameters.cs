using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class NodeRemoveParameters
    {
        [QueryStringParameter("force", false)]
        public bool Force { get; set; }
    }
}