using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainerListProcessesParameters // (main.ContainerListProcessesParameters)
    {
        [QueryStringParameter("ps_args", false)]
        public string PsArgs { get; set; }
    }
}
