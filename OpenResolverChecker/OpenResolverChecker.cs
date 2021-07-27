using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using OpenResolverChecker.Response.V1;

namespace OpenResolverChecker
{
    public class OpenResolverChecker
    {
        private static readonly DnsQueryAndServerOptions DnsQueryOptions = new()
        {
            UseCache = false,
            Recursion = true,
            Retries = 10,
            ThrowDnsErrors = false,
            UseRandomNameServer = false,
            ContinueOnDnsError = false,
            ContinueOnEmptyResponse = false
        };
        
        private readonly IEnumerable<IPEndPoint> _nameServers;
        private readonly string _queryAddress;
        private readonly IEnumerable<QueryType> _queryTypes;

        public OpenResolverChecker(IEnumerable<IPEndPoint> nameServers, string queryAddress, IEnumerable<QueryType> queryTypes)
        {
            _nameServers = nameServers;
            _queryAddress = queryAddress;
            _queryTypes = queryTypes;
        }

        public async Task<CheckResponse> CheckServer()
        {
            var possibleRecursion = false;

            foreach (var nameServer in _nameServers)
            {
                var lookup = new LookupClient(nameServer);
                foreach (var queryType in _queryTypes)
                {
                    var response = await lookup.QueryAsync(new DnsQuestion(_queryAddress, queryType), DnsQueryOptions);
                    if (!possibleRecursion) possibleRecursion = response.Header.RecursionAvailable;
                }
            }

            return new CheckResponse
            {
                TimestampUtc = DateTime.UtcNow,
                NameServerAddresses = _nameServers.Select(endPoint => endPoint.ToString()),
                QueryAddress = _queryAddress,
                QueryTypes = _queryTypes,
                PossibleRecursion = possibleRecursion,
                ConnectionErrors = false
            };
        }
    }
}