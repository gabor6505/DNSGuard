using System;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace OpenResolverChecker
{
    [ApiController]
    [Route("OpenResolverCheck")]
    public class OpenResolverCheckController : ControllerBase
    {
        private static readonly DnsQuestion dnsQuery = new("a.root-servers.net", QueryType.ANY);

        private static readonly DnsQueryAndServerOptions dnsQueryOptions = new()
        {
            UseCache = false,
            Recursion = true,
            Retries = 10,
            ThrowDnsErrors = false,
            UseRandomNameServer = true,
            ContinueOnDnsError = false,
            ContinueOnEmptyResponse = false
        };

        [HttpGet("CheckServer")]
        public async Task<OpenResolverCheckResponse> CheckServer(string addressOfNameServer)
        {
            var lookup = new LookupClient(ResolveAddressToIpAddresses(addressOfNameServer));
            var result = await lookup.QueryAsync(dnsQuery, dnsQueryOptions);

            return new OpenResolverCheckResponse
            {
                TimeOfAnswerUtc = DateTime.UtcNow,
                NameServerAddress = addressOfNameServer,
                DnsResponseCode = result.Header.ResponseCode.ToString(),
                DnsResponseRaFlag = result.Header.RecursionAvailable,
                RecursionAvailable = result.Header.RecursionAvailable && result.Header.ResponseCode == DnsHeaderResponseCode.NoError
                // TODO figure out what response codes can be accepted (everything except Refused OR only NoError)
            };
        }
        
        /**
         * For the input address (an IPv4/IPv6 address or a host name),
         * return a list of found IPv4 and IPv6 addresses.
         */
        public static IPAddress[] ResolveAddressToIpAddresses(string address)
        {
            // todo maybe use DnsClient instead of built-in GetHostAddresses
            // first check if address can be parsed to IPAddress,
            // if not then do a dns query and if it doesnt return any IP address then throw an error
            
            // addresses reversed for testing purposes
            return Dns.GetHostAddresses(address.Trim()).Reverse().ToArray();
        }
    }
}