using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class PortStatus // (swarm.PortStatus)
    {
        [DataMember(Name = "Ports", EmitDefaultValue = false)]
        public IList<PortConfig> Ports { get; set; }
    }
}
