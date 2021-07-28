using System;

namespace OpenResolverChecker.AddressParsing
{
    public class NonExistentDomainException : Exception
    {
        public NonExistentDomainException(string address)
            : base($"Failed to resolve hostname '{address}'! The domain does not exist.")
        {
        }
    }
}