using System.Text.Json.Serialization;
using System.Text.Json;

namespace AspNetCore.Live.Api.HealthChecks.Client
{
    public class SystemTextJsonExceptionConverter : JsonConverter<Exception>
    {
        public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(Exception.Message), value.Message);

            if (value.InnerException is not null)
            {
                writer.WriteStartObject(nameof(Exception.InnerException));
                Write(writer, value.InnerException, options);
                writer.WriteEndObject();
            }

            if (value.TargetSite is not null)
            {
                writer.WriteStartObject(nameof(Exception.TargetSite));
                writer.WriteString(nameof(Exception.TargetSite.Name), value.TargetSite?.Name);
                writer.WriteString(nameof(Exception.TargetSite.DeclaringType), value.TargetSite?.DeclaringType?.FullName);
                writer.WriteEndObject();
            }

            if (value.StackTrace is not null)
            {
                writer.WriteString(nameof(Exception.StackTrace), value.StackTrace);
            }

            writer.WriteString(nameof(Type), value.GetType().ToString());
            writer.WriteEndObject();
        }
    }
}
