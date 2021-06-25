using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainerKillParameters // (main.ContainerKillParameters)
    {
        [QueryStringParameter("signal", false)]
        public string Signal { get; set; }
    }
}
