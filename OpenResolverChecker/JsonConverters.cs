using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DnsClient;

namespace OpenResolverChecker
{
    public class QueryTypeEnumerableJsonConverter : JsonConverter<IEnumerable<QueryType>>
    {
        public override IEnumerable<QueryType> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var queryTypeStrings = reader.GetString()?.Split(",") ?? throw new Exception();
            return queryTypeStrings.Select(s => Enum.Parse<QueryType>(s, true));
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<QueryType> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(string.Join(",", value));
        }
    }
}