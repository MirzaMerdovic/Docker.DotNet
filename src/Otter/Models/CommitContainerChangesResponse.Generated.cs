using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class CommitContainerChangesResponse // (main.CommitContainerChangesResponse)
    {
        [DataMember(Name = "Id", EmitDefaultValue = false)]
        public string ID { get; set; }
    }
}
