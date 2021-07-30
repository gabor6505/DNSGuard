using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenResolverChecker.AddressParsing;
using OpenResolverChecker.Options;
using OpenResolverChecker.Request.V1;
using OpenResolverChecker.Response.V1;

namespace OpenResolverChecker.Controller.V1
{
    [ApiController]
    [Route("OpenResolverChecker/v1")]
    public class CheckController : ControllerBase
    {
        private const ushort DefaultNameServerPort = 53;
        
        private readonly CheckerOptions _options;

        private readonly AddressParser _addressParser = new(DefaultNameServerPort);

        public CheckController(IOptions<CheckerOptions> options)
        {
            _options = options.Value;
        }

        [HttpGet("CheckServer")]
        public async Task<CheckResponse> CheckServer([FromQuery] CheckServerGetRequest request)
        {
            var queryAddress = request.QueryAddress ?? _options.DefaultDnsQueryAddress;
            
            // TODO filter enum values - custom model binder or enum
            var queryTypes = request.QueryTypes ?? ParseQueryTypes(_options.DefaultDnsQueryTypes);

            var nameServers = _addressParser.Parse(request.NameServerAddress);
            if (!(_options.EnableIPv4 && _options.EnableIPv6))
                nameServers = nameServers.Where(FilterEndPoint);

            var checker = new OpenResolverChecker(nameServers, queryAddress, queryTypes, false);
            return await checker.CheckServersAsync();
        }

        /**
         * Checks if this Checker instance can handle the specified IPAddress
         * (depending on EnableIPv4 and EnableIPv6 options)
         */
        private bool FilterEndPoint(EndPoint endPoint)
        {
            if (_options.EnableIPv4 && endPoint.AddressFamily == AddressFamily.InterNetwork) return true;
            if (_options.EnableIPv6 && endPoint.AddressFamily == AddressFamily.InterNetworkV6) return true;
            return false;
        }

        private static IEnumerable<QueryType> ParseQueryTypes(string queryTypesString)
        {
            return queryTypesString.Split(",")
                .Select(s => Enum.Parse<QueryType>(s, true));
        }
    }
}