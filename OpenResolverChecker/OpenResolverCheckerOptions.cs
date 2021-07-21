namespace OpenResolverChecker
{
    public class OpenResolverCheckerOptions
    {
        public const string Key = "OpenResolverChecker";

        public bool EnableIPv4 { get; set; }
        public bool EnableIPv6 { get; set; }
        
        public string DefaultDnsQueryAddress { get; set; }
    }
}