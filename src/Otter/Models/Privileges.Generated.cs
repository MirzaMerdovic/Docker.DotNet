using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class Privileges // (swarm.Privileges)
    {
        [DataMember(Name = "CredentialSpec", EmitDefaultValue = false)]
        public CredentialSpec CredentialSpec { get; set; }

        [DataMember(Name = "SELinuxContext", EmitDefaultValue = false)]
        public SELinuxContext SELinuxContext { get; set; }
    }
}
