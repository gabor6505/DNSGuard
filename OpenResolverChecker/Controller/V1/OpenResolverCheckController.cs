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
        private readonly OpenResolverCheckerOptions _options;

        public OpenResolverCheckController(IOptions<OpenResolverCheckerOptions> options)
        {
            _options = options.Value;
        }

        [HttpGet("CheckServer")]
        public async Task<OpenResolverCheckResponse> CheckServer([FromQuery] CheckServerGetRequest request)
        {
            var queryAddress = request.QueryAddress ?? _options.DefaultDnsQueryAddress;
            
            // TODO filter enum values - custom model binder or enum
            var queryTypes = request.QueryTypes ?? ParseQueryTypes(_options.DefaultDnsQueryTypes);

            var ipAddresses = ResolveAddressToIpAddresses(request.NameServerAddress);
            if (!(_options.EnableIPv4 && _options.EnableIPv6))
                ipAddresses = ipAddresses.Where(FilterIpAddress);

            var checker = new OpenResolverChecker(ipAddresses, request.NameServerPort, queryAddress, queryTypes);
            return await checker.CheckServer();
        }

        /**
         * For the input address (an IPv4/IPv6 address or a host name),
         * return a list of found IPv4 and IPv6 addresses.
         */
        private static IEnumerable<IPAddress> ResolveAddressToIpAddresses(string address)
        {
            // TODO first check if address can be parsed to IPEndPoint,
            // if not then do a dns query and if it doesnt return any IP address then throw an error

            return Dns.GetHostAddresses(address.Trim());
        }

        /**
         * Checks if this Checker instance can handle the specified IPAddress
         * (depending on EnableIPv4 and EnableIPv6 options)
         */
        private bool FilterIpAddress(IPAddress address)
        {
            if (_options.EnableIPv4 && address.AddressFamily == AddressFamily.InterNetwork) return true;
            if (_options.EnableIPv6 && address.AddressFamily == AddressFamily.InterNetworkV6) return true;
            return false;
        }

        private static IEnumerable<QueryType> ParseQueryTypes(string queryTypesString)
        {
            return queryTypesString.Split(",")
                .Select(s => Enum.Parse<QueryType>(s, true));
        }
    }
}