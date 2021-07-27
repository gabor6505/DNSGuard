namespace OpenResolverChecker.Options
{
    public class CheckerOptions
    {
        public const string Key = "OpenResolverChecker";

        public bool EnableIPv4 { get; init; } = true;
        public bool EnableIPv6 { get; init; } = false;

        public string DefaultDnsQueryAddress { get; init; } = "google.com";
        
        // TODO proper binding of string to QueryType[]
        public string DefaultDnsQueryTypes { get; init; } = "A,AAAA,NS,SOA";
    }
}