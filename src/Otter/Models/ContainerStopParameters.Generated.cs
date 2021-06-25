using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainerStopParameters // (main.ContainerStopParameters)
    {
        [QueryStringParameter("t", false)]
        public uint? WaitBeforeKillSeconds { get; set; }
    }
}
