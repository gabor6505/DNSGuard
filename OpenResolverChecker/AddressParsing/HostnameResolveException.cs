using System;

namespace OpenResolverChecker.AddressParsing
{
    public class HostnameResolveException : Exception
    {
        public HostnameResolveException(string address, Exception innerException = null)
            : base($"Failed to resolve hostname '{address}'! The domain might not exist.", innerException)
        {
        }
    }
}