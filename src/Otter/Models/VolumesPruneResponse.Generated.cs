using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class VolumesPruneResponse // (types.VolumesPruneReport)
    {
        [DataMember(Name = "VolumesDeleted", EmitDefaultValue = false)]
        public IList<string> VolumesDeleted { get; set; }

        [DataMember(Name = "SpaceReclaimed", EmitDefaultValue = false)]
        public ulong SpaceReclaimed { get; set; }
    }
}
