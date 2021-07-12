using System;

namespace OpenResolverChecker
{
    public record OpenResolverCheckResponse
    {
        public DateTime TimeOfAnswerUtc { get; init; }

        // public string OwnIP { get; init; }
        
        public string NameServerAddress { get; init; }
        
        public bool RecursionAvailable { get; init; }
    }
}