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

        /**
         * Parses the address, giving back one IPEndPoint if the address is a valid
         * IPv4/v6 address, or one or more IPEndPoints if it is a hostname and could be resolved.
         * If the address doesn't contain a port number, the Parser's default port will be used.
         * 
         * <exception cref="HostnameResolveException">
         * The address couldn't be resolved using the system's internal resolver</exception>
         * <exception cref="HostnameTooLongException">
         * The hostname (without the port) exceeds 254 characters</exception>
         * <exception cref="AddressParseException">
         * The address contains more than one colon</exception>
         * <exception cref="AddressParseException">
         * The string after the colon couldn't be parsed to a ushort port number</exception>
         */
        public IEnumerable<IPEndPoint> Parse(string address)
        {
            var ipEndPoint = ParseIpAddress(address);
            return ipEndPoint != null ? new[] {ipEndPoint} : ResolveHostname(address);
        }

        /**
         * Parses the address to an IPEndPoint, returning null if the address couldn't be parsed.
         * If the address doesn't contain a port number, the Parser's default port will be used.
         */
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

        /**
         * Resolve the specified address to one or more IPEndPoints, treating address as a hostname.
         * If the address doesn't contain a port number, the Parser's default port will be used.
         *
         * <exception cref="HostnameResolveException">
         * The address couldn't be resolved using the system's internal resolver</exception>
         * <exception cref="HostnameTooLongException">
         * The hostname (without the port) exceeds 254 characters</exception>
         * <exception cref="AddressParseException">
         * The address contains more than one colon</exception>
         * <exception cref="AddressParseException">
         * The string after the colon couldn't be parsed to a ushort port number</exception>
         */
        private IEnumerable<IPEndPoint> ResolveHostname(string address)
        {
            var (hostname, port) = ParseHostname(address);

            try
            {
                return Dns.GetHostAddresses(hostname).Select(a => new IPEndPoint(a, port));
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new HostnameTooLongException(hostname);
            }
            catch (Exception e)
            {
                throw new HostnameResolveException(address, e);
            }
        }

        /**
         * Parses the address into a hostname and a port.
         * If the address doesn't contain a port number, the Parser's default port will be used.
         * 
         * <exception cref="AddressParseException">
         * The address contains more than one colon</exception>
         * <exception cref="AddressParseException">
         * The string after the colon couldn't be parsed to a ushort port number</exception>
         */
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