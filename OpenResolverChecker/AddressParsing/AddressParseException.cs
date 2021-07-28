using System;

namespace OpenResolverChecker.AddressParsing
{
    
    public class AddressParseException : Exception
    {
        public AddressParseException(string address, Exception innerException = null)
            : base($"Failed to parse address '{address}'! Format should be <hostname|ipv4_addr|ipv6_addr>[:<port>].", innerException)
        {
        }
    }
}