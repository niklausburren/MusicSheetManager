using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MusicSheetManager.Models;

namespace MusicSheetManager.Converters;

public class ClefInfoConverter : JsonConverter<ClefInfo>
{
    #region Public Methods

    public override ClefInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var key = reader.GetString();
        return ClefInfo.GetByKey(key);
    }

    public override void Write(Utf8JsonWriter writer, ClefInfo value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Key);
    }

    #endregion
}