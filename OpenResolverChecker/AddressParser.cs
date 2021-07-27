using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace OpenResolverChecker
{
    public static class AddressParser
    {
        public static IEnumerable<IPEndPoint> ParseAddresses(IEnumerable<string> addresses, ushort defaultPort)
        {
            return addresses.SelectMany(a => ParseAddress(a, defaultPort));
        }
        
        public static IEnumerable<IPEndPoint> ParseAddress(string address, ushort defaultPort)
        {
            var parseSuccess = IPEndPoint.TryParse(address, out var ipEndPoint);
            if (parseSuccess)
                return new []{ipEndPoint};

            var colonCount = address.Count(c => c == ':');
            if (colonCount > 1)
                throw new Exception("Invalid address");

            var splitAddress = address.Split(":");

            if (colonCount == 0)
                return Dns.GetHostAddresses(address).Select(a => new IPEndPoint(a, defaultPort));
            
            parseSuccess = ushort.TryParse(splitAddress[1], out var port);
            if (parseSuccess)
                return Dns.GetHostAddresses(splitAddress[0]).Select(a => new IPEndPoint(a, port));
            
            throw new Exception("Invalid address");
        }
        
        // TODO range parser
    }
}