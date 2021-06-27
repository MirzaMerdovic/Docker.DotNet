using System.Runtime.Serialization;
using Otter.Converters.QueryString;

namespace Otter.Models
{
    [DataContract]
    public class ContainerRenameParameters // (main.ContainerRenameParameters)
    {
        [QueryStringParameter("name", false)]
        public string NewName { get; set; }
    }
}
