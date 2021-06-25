using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class PlacementPreference // (swarm.PlacementPreference)
    {
        [DataMember(Name = "Spread", EmitDefaultValue = false)]
        public SpreadOver Spread { get; set; }
    }
}
