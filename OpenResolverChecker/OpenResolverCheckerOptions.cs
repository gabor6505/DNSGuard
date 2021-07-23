using System.Collections.Generic;
using System.Text.Json.Serialization;
using DnsClient;

namespace OpenResolverChecker
{
    public class OpenResolverCheckerOptions
    {
        public const string Key = "OpenResolverChecker";

        public bool EnableIPv4 { get; init; } = true;
        public bool EnableIPv6 { get; init; } = false;

        public string DefaultDnsQueryAddress { get; init; } = "google.com";
        
        [JsonConverter(typeof(QueryTypeEnumerableJsonConverter))]
        public IEnumerable<QueryType> DefaultDnsQueryTypes { get; init; } = new [] {QueryType.A, QueryType.AAAA, QueryType.NS, QueryType.SOA};
    }
}