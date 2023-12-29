using Csb.BigMom.Infrastructure.Entities;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Job.Integration
{
    public class CommerceIntegrationMccJsonConverter : JsonConverter<Mcc>
    {
        public override Mcc Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string code = null;
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                reader.Read();

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (string.Equals(reader.GetString(), nameof(Mcc.Code), StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Read();
                        code = reader.GetString();
                    }
                }
            }
            return new()
            {
                Code = code
            };
        }

        public override void Write(Utf8JsonWriter writer, Mcc value, JsonSerializerOptions options) => throw new NotImplementedException();
    }
}
