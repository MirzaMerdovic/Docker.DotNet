using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ServiceUpdateResponse // (types.ServiceUpdateResponse)
    {
        [DataMember(Name = "Warnings", EmitDefaultValue = false)]
        public IList<string> Warnings { get; set; }
    }
}
