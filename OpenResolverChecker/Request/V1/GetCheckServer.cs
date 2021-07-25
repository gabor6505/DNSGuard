using System.ComponentModel.DataAnnotations;

namespace OpenResolverChecker.Request.V1
{
    public class GetCheckServer
    {
        [Required]
        public string NameServerAddress { get; init; }

        public ushort NameServerPort { get; init; } = 53;

        public string QueryAddress { get; init; } = null;
        
        // TODO create model binder for IEnumerable<QueryType>
        public string QueryTypes { get; init; } = null;
    }
}