using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ServiceCreateResponse // (types.ServiceCreateResponse)
    {
        [DataMember(Name = "ID", EmitDefaultValue = false)]
        public string ID { get; set; }

        [DataMember(Name = "Warnings", EmitDefaultValue = false)]
        public IList<string> Warnings { get; set; }
    }
}
