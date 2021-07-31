using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.AspNetCore.Http;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckServers([FromQuery] CheckServersGetRequest request)
        {
            return await TryCheckServers(request, false);
        }

        [HttpGet("CheckServersDetailed")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckServersDetailed([FromQuery] CheckServersGetRequest request)
        {
            return await TryCheckServers(request, true);
        }
        
        private async Task<IActionResult> TryCheckServers(CheckServersGetRequest request, bool detailed)
        {
            try
            {
                return Ok(await CheckServers(request, detailed));
            }
            catch (Exception e) when(e is AddressParseException or HostnameResolveException or HostnameTooLongException)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
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