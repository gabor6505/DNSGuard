using System;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.Extensions.Options;
using OpenResolverChecker.Response;

namespace OpenResolverChecker
{
    [ApiController]
    [Route("OpenResolverCheck")]
    public class OpenResolverCheckController : ControllerBase
    {
        private const ushort DefaultNameServerPort = 53;
        
        private readonly string DefaultQueryAddress = "google.com";
        private readonly QueryType[] DefaultQueryTypes = {QueryType.A, QueryType.AAAA, QueryType.NS, QueryType.SOA};
        
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

        private readonly OpenResolverCheckerOptions _options;

        public OpenResolverCheckController(IOptions<OpenResolverCheckerOptions> options)
        {
            _options = options.Value;
            
            // Set default parameters from options, only if they are present
            if (_options.DefaultDnsQueryAddress != null)
                DefaultQueryAddress = _options.DefaultDnsQueryAddress;
            
            if (_options.DefaultDnsQueryTypes != null)
                DefaultQueryTypes = ParseQueryTypes(_options.DefaultDnsQueryTypes);
        }

        [HttpGet("CheckServer")]
        public async Task<OpenResolverCheckResponse> CheckServer(string nameServerAddress, ushort nameServerPort = DefaultNameServerPort,
            string queryAddress = null, string queryTypes = null)
        {
            queryAddress ??= DefaultQueryAddress;
            var parsedQueryTypes = queryTypes != null ? ParseQueryTypes(queryTypes) : DefaultQueryTypes;

            var possibleRecursion = false;

            foreach (var ipAddress in ResolveAddressToIpAddresses(nameServerAddress))
            {
                var lookup = new LookupClient(ipAddress, nameServerPort);
                foreach (var queryType in parsedQueryTypes)
                {
                    var result = await lookup.QueryAsync(new DnsQuestion(queryAddress, queryType), DnsQueryOptions);
                    if (!possibleRecursion) possibleRecursion = result.Header.RecursionAvailable;
                }
            }

            return new OpenResolverCheckResponse
            {
                TimestampUtc = DateTime.UtcNow,
                NameServerAddress = nameServerAddress,
                NameServerPort = nameServerPort,
                QueryAddress = queryAddress,
                QueryTypes = parsedQueryTypes,
                PossibleRecursion = possibleRecursion,
                ConnectionErrors = false
            };
        }

        /**
         * For the input address (an IPv4/IPv6 address or a host name),
         * return a list of found IPv4 and IPv6 addresses.
         */
        public static IPAddress[] ResolveAddressToIpAddresses(string address)
        {
            // TODO maybe use DnsClient instead of built-in GetHostAddresses
            // first check if address can be parsed to IPAddress,
            // if not then do a dns query and if it doesnt return any IP address then throw an error

            // addresses reversed for testing purposes
            return Dns.GetHostAddresses(address.Trim()).Reverse().ToArray();
        }

        private static QueryType[] ParseQueryTypes(string queryTypesString)
        {
            // TODO only allow certain query types - throw an error or just skip requests for illegal types
            var queryTypesSplit = queryTypesString.Split(",");
            
            var queryTypes = new QueryType[queryTypesSplit.Length];

            for (var i = 0; i < queryTypesSplit.Length; i++)
            {
                var parseSuccessful = Enum.TryParse(queryTypesSplit[i], true, out queryTypes[i]);
                if (!parseSuccessful)
                    throw new Exception();
            }

            return queryTypes;
        }
    }
}