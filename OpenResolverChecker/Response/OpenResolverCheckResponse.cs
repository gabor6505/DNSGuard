using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DnsClient;

namespace OpenResolverChecker.Response
{
    public class OpenResolverCheckResponse
    {
        public DateTime TimestampUtc { get; init; }
        
        public string[] NameServerIpAddresses { get; init; }
        public ushort NameServerPort { get; init; }
        public string QueryAddress { get; init; }
        
        [JsonConverter(typeof(QueryTypeEnumerableJsonConverter))]
        public IEnumerable<QueryType> QueryTypes { get; init; }

        public bool PossibleRecursion { get; init; }
        public bool ConnectionErrors { get; init; }
    }
}