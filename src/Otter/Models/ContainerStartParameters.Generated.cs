using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainerStartParameters // (main.ContainerStartParameters)
    {
        [QueryStringParameter("detachKeys", false)]
        public string DetachKeys { get; set; }
    }
}
