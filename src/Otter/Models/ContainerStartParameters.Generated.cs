using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class ContainerStartParameters // (main.ContainerStartParameters)
    {
        [QueryStringParameter("detachKeys", false)]
        public string DetachKeys { get; set; }
    }
}
