using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure
{
    /// <summary>
    /// Provides <see cref="JsonConverter"/> to convert <see cref="TimeSpan"/>.
    /// </summary>
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        /// <inheritdoc/>
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            TimeSpan.Parse(reader.GetString());

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
    }
}
