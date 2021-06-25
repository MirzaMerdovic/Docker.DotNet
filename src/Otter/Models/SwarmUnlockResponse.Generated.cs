using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class SwarmUnlockResponse // (main.SwarmUnlockResponse)
    {
        [DataMember(Name = "UnlockKey", EmitDefaultValue = false)]
        public string UnlockKey { get; set; }
    }
}
