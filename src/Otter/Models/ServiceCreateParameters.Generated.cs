using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ServiceCreateParameters // (main.ServiceCreateParameters)
    {
        [DataMember(Name = "Service", EmitDefaultValue = false)]
        public ServiceSpec Service { get; set; }

        [DataMember(Name = "RegistryAuth", EmitDefaultValue = false)]
        public AuthConfig RegistryAuth { get; set; }
    }
}
