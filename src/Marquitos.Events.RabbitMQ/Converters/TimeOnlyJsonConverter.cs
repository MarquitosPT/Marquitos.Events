using System;
using System.Globalization;
#if NET6_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;
#endif
namespace Marquitos.Events.RabbitMQ.Converters
{
#if NET6_0_OR_GREATER
    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private const string Format = "HH:mm:ss.fff";

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeOnly.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
#endif
}
