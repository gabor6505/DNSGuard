using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet("CheckServers")]
        public async Task<CheckResponse> CheckServers([FromQuery] CheckServersGetRequest request)
        {
            return await CheckServers(request, false);
        }

        [HttpGet("CheckServersDetailed")]
        public async Task<CheckResponse> CheckServersDetailed([FromQuery] CheckServersGetRequest request)
        {
            return await CheckServers(request, true);
        }

        private async Task<CheckResponse> CheckServers(CheckServersGetRequest request, bool detailed)
        {
            var queryAddress = request.QueryAddress ?? _options.DefaultDnsQueryAddress;
            
            // TODO filter enum values - custom model binder or enum
            var queryTypes = request.QueryTypes ?? ParseQueryTypes(_options.DefaultDnsQueryTypes);

            var nameServers = _addressParser.Parse(request.NameServerAddresses);

            var checker = new OpenResolverChecker(nameServers, queryAddress, queryTypes, detailed);
            return await checker.CheckServersAsync();
        }

        private static IEnumerable<QueryType> ParseQueryTypes(string queryTypesString)
        {
            return queryTypesString.Split(",")
                .Select(s => Enum.Parse<QueryType>(s, true));
        }
    }
}