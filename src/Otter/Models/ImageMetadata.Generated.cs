using System;
using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ImageMetadata // (types.ImageMetadata)
    {
        [DataMember(Name = "LastTagTime", EmitDefaultValue = false)]
        public DateTime LastTagTime { get; set; }
    }
}
