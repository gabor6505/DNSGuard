using DnsClient;

namespace OpenResolverChecker.Response.V1
{
    public class DnsQueryResponse
    {
        public DnsHeaderResponseCode ResponseCode { get; init; }
        public string ResponseFlags { get; init; }
        public bool RecursionAvailableFlag { get; init; }
        public ushort AnswerRecordCount { get; init; }
    }
}