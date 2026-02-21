using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;

namespace UI.Models;

public sealed class MarkupStringJsonConverter : JsonConverter<MarkupString>
{
    public override MarkupString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new MarkupString(reader.GetString() ?? string.Empty);
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            if (document.RootElement.TryGetProperty("Value", out JsonElement valueElement))
            {
                return new MarkupString(valueElement.GetString() ?? string.Empty);
            }
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return new MarkupString(string.Empty);
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing MarkupString.");
    }

    public override void Write(Utf8JsonWriter writer, MarkupString value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
