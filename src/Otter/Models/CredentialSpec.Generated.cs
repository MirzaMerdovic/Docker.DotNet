using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class CredentialSpec // (swarm.CredentialSpec)
    {
        [DataMember(Name = "Config", EmitDefaultValue = false)]
        public string Config { get; set; }

        [DataMember(Name = "File", EmitDefaultValue = false)]
        public string File { get; set; }

        [DataMember(Name = "Registry", EmitDefaultValue = false)]
        public string Registry { get; set; }
    }
}
