using System;
using System.Collections.Generic;
using DnsClient;

namespace OpenResolverChecker.Response.V1
{
    public class OpenResolverCheckResponse
    {
        public DateTime TimestampUtc { get; init; }
        
        public IEnumerable<string> NameServerIpAddresses { get; init; }
        public ushort NameServerPort { get; init; }
        public string QueryAddress { get; init; }
        public IEnumerable<QueryType> QueryTypes { get; init; }

        public bool PossibleRecursion { get; init; }
        public bool ConnectionErrors { get; init; }
    }
}