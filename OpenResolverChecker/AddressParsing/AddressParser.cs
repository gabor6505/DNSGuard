using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace OpenResolverChecker.AddressParsing
{
    public class AddressParser
    {
        private readonly ushort _defaultPort;

        public AddressParser(ushort defaultPort)
        {
            _defaultPort = defaultPort;
        }

        public IEnumerable<IPEndPoint> Parse(IEnumerable<string> addresses)
        {
            return addresses.SelectMany(Parse);
        }

        public IEnumerable<IPEndPoint> Parse(string address)
        {
            var ipEndPoint = ParseIpAddress(address);
            return ipEndPoint != null ? new[] {ipEndPoint} : ResolveHostname(address);
        }

        private IPEndPoint ParseIpAddress(string address)
        {
            // TODO properly validate IPv4/v6 addresses
            try
            {
                var ipEndPoint = IPEndPoint.Parse(address);
                if (ipEndPoint.Port == 0) ipEndPoint.Port = _defaultPort;
                return ipEndPoint;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private IEnumerable<IPEndPoint> ResolveHostname(string address)
        {
            var (hostname, port) = ParseHostname(address);
            // TODO handle GetHostAddresses exceptions and NXDOMAIN
            var ipEndPoints = Dns.GetHostAddresses(hostname).Select(a => new IPEndPoint(a, port));;

            //if (!ipEndPoints.Any())
            //    throw new NonExistentDomainException(address);

            return ipEndPoints;
        }

        private Tuple<string, ushort> ParseHostname(string address)
        {
            var colonCount = address.Count(c => c == ':');

            switch (colonCount)
            {
                case 0:
                    return new Tuple<string, ushort>(address, _defaultPort);
                case > 1:
                    throw new AddressParseException(address);
                default: // colonCount == 1
                {
                    var splitAddress = address.Split(":");
            
                    try
                    {
                        return new Tuple<string, ushort>(splitAddress[0], ushort.Parse(splitAddress[1]));
                    }
                    catch (Exception e)
                    {
                        throw new AddressParseException(address, e);
                    }
                }
            }
        }

        // TODO range parser
    }
}