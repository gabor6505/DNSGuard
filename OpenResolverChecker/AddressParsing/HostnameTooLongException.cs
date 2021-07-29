using System;

namespace OpenResolverChecker.AddressParsing
{
    public class HostnameTooLongException : Exception
    {
        public HostnameTooLongException(string hostname) : base($"'{hostname}' is too long, it must be no longer than 254 characters.")
        {
        }
    }
}