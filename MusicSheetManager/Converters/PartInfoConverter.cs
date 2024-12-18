using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MusicSheetManager.Models;

namespace MusicSheetManager.Converters
{
    public class PartInfoConverter : JsonConverter<PartInfo>
    {
        #region Public Methods

        public override PartInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var key = reader.GetString();
            return PartInfo.GetByKey(key);
        }

        public override void Write(Utf8JsonWriter writer, PartInfo value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Key);
        }

        #endregion
    }
}
