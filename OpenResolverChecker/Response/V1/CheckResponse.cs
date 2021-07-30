using System;
using System.Collections.Generic;
using DnsClient;

namespace OpenResolverChecker.Response.V1
{
    public class CheckResponse
    {
        public DateTime TimestampUtc { get; init; }
        public string QueryAddress { get; init; }

        public IEnumerable<CheckResult> CheckResults { get; init; }
    }
}