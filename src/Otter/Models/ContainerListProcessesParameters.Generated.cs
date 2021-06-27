using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class ContainerListProcessesParameters // (main.ContainerListProcessesParameters)
    {
        [QueryStringParameter("ps_args", false)]
        public string PsArgs { get; set; }
    }
}
