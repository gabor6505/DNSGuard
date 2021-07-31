namespace OpenResolverChecker.Response.V1
{
    public enum ConnectionError : byte
    {
        None,
        HostUnreachable,
        Timeout,
        Unknown
    }
}