using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class SwarmUnlockParameters // (main.SwarmUnlockParameters)
    {
        [DataMember(Name = "UnlockKey", EmitDefaultValue = false)]
        public string UnlockKey { get; set; }
    }
}
