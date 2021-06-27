using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class ContainerKillParameters // (main.ContainerKillParameters)
    {
        [QueryStringParameter("signal", false)]
        public string Signal { get; set; }
    }
}
