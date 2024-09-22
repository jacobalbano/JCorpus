using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JCorpus.Web.Converters;

internal class InstantToJsDate : JsonConverter<Instant>
{
    public override Instant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Instant.FromUnixTimeMilliseconds(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, Instant value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("epochMs", value.ToUnixTimeMilliseconds());
        writer.WriteEndObject();
    }
}
