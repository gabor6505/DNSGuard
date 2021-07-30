using DnsClient;

namespace OpenResolverChecker.Response.V1
{
    public class CheckResult
    {
        public string NameServerIp { get; init; }
        public QueryType QueryType { get; init; }
        
        public ConnectionError ConnectionError { get; init; }
        public bool PossibleRecursion { get; init; }
        
        // TODO exclude from response when null
        public DnsQueryResponse DnsQueryResponse { get; init; }
    }
}