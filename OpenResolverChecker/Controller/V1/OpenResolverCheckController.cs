using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenResolverChecker.Options;
using OpenResolverChecker.Request.V1;
using OpenResolverChecker.Response.V1;

namespace OpenResolverChecker.Controller.V1
{
    [ApiController]
    [Route("OpenResolverChecker/v1")]
    public class OpenResolverCheckController : ControllerBase
    {
        private const ushort DefaultNameServerPort = 53;

        private readonly bool _enableIPv4;
        private readonly bool _enableIPv6;
        private readonly string _defaultQueryAddress;
        private readonly IEnumerable<QueryType> _defaultQueryTypes;

        public OpenResolverCheckController(IOptions<OpenResolverCheckerOptions> options)
        {
            var checkerOptions = options.Value;
            
            // Set default parameters from options
            _enableIPv4 = checkerOptions.EnableIPv4;
            _enableIPv6 = checkerOptions.EnableIPv6;
            _defaultQueryAddress = checkerOptions.DefaultDnsQueryAddress;
            _defaultQueryTypes = checkerOptions.DefaultDnsQueryTypes;
            Console.WriteLine(_defaultQueryAddress);
        }

        [HttpGet("CheckServer")]
        public async Task<OpenResolverCheckResponse> CheckServer([FromQuery] GetCheckServer request)
        {
            var queryAddress = request.QueryAddress ?? _defaultQueryAddress;
            var queryTypes = request.QueryTypes == null ? _defaultQueryTypes : ParseQueryTypes(request.QueryTypes);

            var ipAddresses = ResolveAddressToIpAddresses(request.NameServerAddress);
            if (!(_enableIPv4 && _enableIPv6)) ipAddresses = ipAddresses.Where(FilterIpAddress);

            var checker = new OpenResolverChecker(ipAddresses, request.NameServerPort, queryAddress, queryTypes);
            return await checker.CheckServer();
        }

        /**
         * For the input address (an IPv4/IPv6 address or a host name),
         * return a list of found IPv4 and IPv6 addresses.
         */
        private static IEnumerable<IPAddress> ResolveAddressToIpAddresses(string address)
        {
            // TODO maybe use DnsClient instead of built-in GetHostAddresses
            // first check if address can be parsed to IPAddress,
            // if not then do a dns query and if it doesnt return any IP address then throw an error

            return Dns.GetHostAddresses(address.Trim());
        }

        /**
         * Checks if this Checker instance can handle the specified IPAddress
         * (depending on EnableIPv4 and EnableIPv6 options)
         */
        private bool FilterIpAddress(IPAddress address)
        {
            if (_enableIPv4 && address.AddressFamily == AddressFamily.InterNetwork) return true;
            if (_enableIPv6 && address.AddressFamily == AddressFamily.InterNetworkV6) return true;
            return false;
        }

        private static IEnumerable<QueryType> ParseQueryTypes(string queryTypesString)
        {
            // TODO only allow certain query types - throw an error or just skip requests for illegal types
            return queryTypesString.Split(",")
                .Select(s => Enum.Parse<QueryType>(s, true));
        }
    }
}