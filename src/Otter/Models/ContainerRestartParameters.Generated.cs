using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainerRestartParameters // (main.ContainerRestartParameters)
    {
        [QueryStringParameter("t", false)]
        public uint? WaitBeforeKillSeconds { get; set; }
    }
}
