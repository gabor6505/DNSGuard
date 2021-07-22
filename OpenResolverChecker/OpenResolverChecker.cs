using System;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using OpenResolverChecker.Response;

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
        
        private readonly IPAddress[] _nameServerIpAddresses;
        private readonly ushort _nameServerPort;
        private readonly string _queryAddress;
        private readonly QueryType[] _queryTypes;

        public OpenResolverChecker(IPAddress[] nameServerIpAddresses, ushort nameServerPort, string queryAddress, QueryType[] queryTypes)
        {
            _nameServerIpAddresses = nameServerIpAddresses;
            _nameServerPort = nameServerPort;
            _queryAddress = queryAddress;
            _queryTypes = queryTypes;
        }

        public async Task<OpenResolverCheckResponse> CheckServer()
        {
            var possibleRecursion = false;

            foreach (var ipAddress in _nameServerIpAddresses)
            {
                var lookup = new LookupClient(ipAddress, _nameServerPort);
                foreach (var queryType in _queryTypes)
                {
                    var response = await lookup.QueryAsync(new DnsQuestion(_queryAddress, queryType), DnsQueryOptions);
                    if (!possibleRecursion) possibleRecursion = response.Header.RecursionAvailable;
                }
            }

            return new OpenResolverCheckResponse
            {
                TimestampUtc = DateTime.UtcNow,
                NameServerIpAddresses = Array.ConvertAll(_nameServerIpAddresses, input => input.ToString()),
                NameServerPort = _nameServerPort,
                QueryAddress = _queryAddress,
                QueryTypes = _queryTypes,
                PossibleRecursion = possibleRecursion,
                ConnectionErrors = false
            };
        }
    }
}