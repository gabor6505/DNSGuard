using System;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenResolverChecker.Response;

namespace OpenResolverChecker
{
    [ApiController]
    [Route("OpenResolverCheck")]
    public class OpenResolverCheckController : ControllerBase
    {
        private const ushort DefaultNameServerPort = 53;
        
        private readonly string _defaultQueryAddress = "google.com";
        private readonly QueryType[] _defaultQueryTypes = {QueryType.A, QueryType.AAAA, QueryType.NS, QueryType.SOA};

        public OpenResolverCheckController(IOptions<OpenResolverCheckerOptions> options)
        {
            var checkerOptions = options.Value;
            
            // Set default parameters from options, only if they are present
            if (checkerOptions.DefaultDnsQueryAddress != null)
                _defaultQueryAddress = checkerOptions.DefaultDnsQueryAddress;
            
            if (checkerOptions.DefaultDnsQueryTypes != null)
                _defaultQueryTypes = ParseQueryTypes(checkerOptions.DefaultDnsQueryTypes);
        }

        [HttpGet("CheckServer")]
        public async Task<OpenResolverCheckResponse> CheckServer(string nameServerAddress, ushort nameServerPort = DefaultNameServerPort,
            string queryAddress = null, string queryTypes = null)
        {
            queryAddress ??= _defaultQueryAddress;
            var parsedQueryTypes = queryTypes != null ? ParseQueryTypes(queryTypes) : _defaultQueryTypes;

            var checker = new OpenResolverChecker(ResolveAddressToIpAddresses(nameServerAddress), nameServerPort, queryAddress, parsedQueryTypes);
            return await checker.CheckServer();
        }

        /**
         * For the input address (an IPv4/IPv6 address or a host name),
         * return a list of found IPv4 and IPv6 addresses.
         */
        private static IPAddress[] ResolveAddressToIpAddresses(string address)
        {
            // TODO maybe use DnsClient instead of built-in GetHostAddresses
            // first check if address can be parsed to IPAddress,
            // if not then do a dns query and if it doesnt return any IP address then throw an error

            return Dns.GetHostAddresses(address.Trim());
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