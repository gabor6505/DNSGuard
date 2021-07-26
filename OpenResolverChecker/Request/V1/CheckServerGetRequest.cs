using System.ComponentModel.DataAnnotations;
using DnsClient;

namespace OpenResolverChecker.Request.V1
{
    public class CheckServerGetRequest
    {
        [Required]
        public string NameServerAddress { get; init; }

        public ushort NameServerPort { get; init; } = 53;

        public string QueryAddress { get; init; } = null;
        
        // TODO allow comma separated list
        public QueryType[] QueryTypes { get; init; } = null;
    }
}