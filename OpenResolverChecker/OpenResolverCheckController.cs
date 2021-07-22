using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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

        private readonly bool _enableIPv4;
        private readonly bool _enableIPv6;
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

            _enableIPv4 = checkerOptions.EnableIPv4;
            _enableIPv6 = checkerOptions.EnableIPv6;
        }

        [HttpGet("CheckServer")]
        public async Task<OpenResolverCheckResponse> CheckServer(string nameServerAddress, ushort nameServerPort = DefaultNameServerPort,
            string queryAddress = null, string queryTypes = null)
        {
            queryAddress ??= _defaultQueryAddress;
            var parsedQueryTypes = queryTypes != null ? ParseQueryTypes(queryTypes) : _defaultQueryTypes;

            var ipAddresses = ResolveAddressToIpAddresses(nameServerAddress);
            ipAddresses = FilterIpAddresses(ipAddresses);

            var checker = new OpenResolverChecker(ipAddresses, nameServerPort, queryAddress, parsedQueryTypes);
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

        /**
         * Filters the IP addresses according to the capabilities of this Checker instance
         * (depending on EnableIPv4 and EnableIPv6 options)
         */
        private IPAddress[] FilterIpAddresses(IPAddress[] addresses)
        {
            if (_enableIPv4 && _enableIPv6) return addresses;
            
            var filteredAddresses = new List<IPAddress>();

            foreach (var address in addresses)
            {
                if (!_enableIPv4 && address.AddressFamily == AddressFamily.InterNetwork) continue;
                if (!_enableIPv6 && address.AddressFamily == AddressFamily.InterNetworkV6) continue;
                filteredAddresses.Add(address);
            }

            return filteredAddresses.ToArray();
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