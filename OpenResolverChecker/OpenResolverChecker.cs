using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using OpenResolverChecker.Response.V1;
using DnsQueryResponse = OpenResolverChecker.Response.V1.DnsQueryResponse;

namespace OpenResolverChecker
{
    public class OpenResolverChecker
    {
        private static readonly DnsQueryAndServerOptions DnsQueryOptions = new()
        {
            UseCache = false,
            Recursion = true,
            Retries = 1,
            ThrowDnsErrors = false,
            UseRandomNameServer = false,
            ContinueOnDnsError = false,
            ContinueOnEmptyResponse = false
        };
        
        private readonly IEnumerable<IPEndPoint> _nameServers;
        private readonly string _queryAddress;
        private readonly IEnumerable<QueryType> _queryTypes;
        private readonly bool _detailed;

        public OpenResolverChecker(IEnumerable<IPEndPoint> nameServers, string queryAddress, IEnumerable<QueryType> queryTypes, bool detailed)
        {
            _nameServers = nameServers;
            _queryAddress = queryAddress;
            _queryTypes = queryTypes;
            _detailed = detailed;
        }

        public async Task<CheckResponse> CheckServersAsync()
        {
            // TODO maybe run queries in parallel - compare performance
            var checkTasks = _nameServers.SelectMany(nameServer =>
            {
                var lookupClient = new LookupClient(nameServer);
                return _queryTypes.Select(queryType => CheckServerAsync(lookupClient, queryType));
            });

            var checkResults = await Task.WhenAll(checkTasks);

            return new CheckResponse
            {
                TimestampUtc = DateTime.UtcNow,
                QueryAddress = _queryAddress,
                CheckResults = checkResults
            };
        }

        private async Task<CheckResult> CheckServerAsync(ILookupClient lookupClient, QueryType queryType)
        {
            var lookupResponse = await lookupClient.QueryAsync(new DnsQuestion(_queryAddress, queryType), DnsQueryOptions);

            DnsQueryResponse dnsQueryResponse = null;
            if (_detailed)
            {
                dnsQueryResponse = new DnsQueryResponse
                {
                    ResponseCode = lookupResponse.Header.ResponseCode,
                    ResponseFlags = GetHeaderFlags(lookupResponse.Header),
                    AnswerRecordCount = (ushort) lookupResponse.Header.AnswerCount
                };
            }

            return new CheckResult
            {
                NameServerIp = lookupClient.NameServers.First().ToString(),
                QueryType = queryType,
                ConnectionError = ConnectionError.None,
                PossibleRecursion = lookupResponse.Header.RecursionAvailable,
                DnsQueryResponse = dnsQueryResponse
            };
        }

        private static string GetHeaderFlags(DnsResponseHeader header)
        {
            var flags = new List<string>();
            if (header.HasAuthorityAnswer) flags.Add("aa");
            if (header.ResultTruncated) flags.Add("tc");
            if (header.RecursionDesired) flags.Add("rd");
            if (header.RecursionAvailable) flags.Add("ra");
            if (header.IsAuthenticData) flags.Add("ad");
            if (header.IsCheckingDisabled) flags.Add("cd");
            return string.Join(" ", flags);
        }
    }
}