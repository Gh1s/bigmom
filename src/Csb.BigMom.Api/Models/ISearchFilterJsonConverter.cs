using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Csb.BigMom.Api.Models
{
    public class ISearchFilterJsonConverter : JsonConverter<ISearchFilter>
    {
        private readonly Regex _dateRegex = new Regex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2})?");

        public override ISearchFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var values = JsonSerializer.Deserialize<IEnumerable<JsonElement>>(ref reader, options);
                if (values.Any(v => v.ValueKind == JsonValueKind.String))
                {
                    return new ArraySearchFilter<string>
                    {
                        Values = values.Select(v => v.GetString()).ToArray()
                    };
                }
                else if (values.All(v => v.ValueKind == JsonValueKind.Number))
                {
                    return new ArraySearchFilter<double>
                    {
                        Values = values.Select(v => v.GetDouble()).ToArray()
                    };
                }
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                var range = JsonSerializer.Deserialize<RangeSearchFilter<JsonElement>>(ref reader, options);
                if (range.Start.ValueKind == JsonValueKind.String &&
                    _dateRegex.IsMatch(range.Start.GetString()) &&
                    range.End.ValueKind == JsonValueKind.String &&
                    _dateRegex.IsMatch(range.End.GetString()))
                {
                    return new RangeSearchFilter<DateTimeOffset>
                    {
                        Start = DateTimeOffset.Parse(range.Start.GetString()),
                        End = DateTimeOffset.Parse(range.End.GetString())
                    };
                }
                else if (
                    range.Start.ValueKind == JsonValueKind.Number &&
                    range.End.ValueKind == JsonValueKind.Number)
                {
                    return new RangeSearchFilter<double>
                    {
                        Start = range.Start.GetDouble(),
                        End = range.End.GetDouble()
                    };
                }
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, ISearchFilter value, JsonSerializerOptions options) => throw new NotImplementedException();
    }
}
