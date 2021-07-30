using System.ComponentModel.DataAnnotations;
using DnsClient;

namespace OpenResolverChecker.Request.V1
{
    public class CheckServersGetRequest
    {
        [Required]
        public string[] NameServerAddresses { get; init; }

        public string QueryAddress { get; init; } = null;
        
        // TODO allow comma separated list
        public QueryType[] QueryTypes { get; init; } = null;
    }
}