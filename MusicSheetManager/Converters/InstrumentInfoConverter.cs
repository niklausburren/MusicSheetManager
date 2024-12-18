using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MusicSheetManager.Models;

namespace MusicSheetManager.Converters
{
    public class InstrumentInfoConverter : JsonConverter<InstrumentInfo>
    {
        #region Public Methods

        public override InstrumentInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var key = reader.GetString();
            return InstrumentInfo.GetByKey(key);
        }

        public override void Write(Utf8JsonWriter writer, InstrumentInfo value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Key);
        }

        #endregion
    }
}
