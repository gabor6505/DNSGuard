using System;

namespace OpenResolverChecker.AddressParsing
{
    public class HostnameResolveException : Exception
    {
        public HostnameResolveException(string hostname, Exception innerException = null)
            : base($"Failed to resolve hostname '{hostname}'! The domain might not exist.", innerException)
        {
        }
    }
}