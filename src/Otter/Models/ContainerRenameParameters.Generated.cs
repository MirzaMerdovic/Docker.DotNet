using System.Runtime.Serialization;

namespace Otter.Models
{
    [DataContract]
    public class ContainerRenameParameters // (main.ContainerRenameParameters)
    {
        [QueryStringParameter("name", false)]
        public string NewName { get; set; }
    }
}
