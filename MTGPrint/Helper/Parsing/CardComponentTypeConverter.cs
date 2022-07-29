using System;

using Newtonsoft.Json;

using MTGPrint.Models;

namespace MTGPrint.Helper.Parsing
{
    public sealed class CardComponentTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType,
                    object existingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;

            return value switch
            {
                "token" => CardComponent.Token,
                "meld_part" => CardComponent.MeldPart,
                "meld_result" => CardComponent.MeldResult,
                "combo_piece" => (object)CardComponent.ComboPiece,
                _ => throw new JsonSerializationException($"{value} not found in enum {nameof(CardComponent)}"),
            };
        }

        public override void WriteJson(JsonWriter writer, object value,
                    JsonSerializer serializer)
        {
            var val = (CardComponent)value;

            switch (val)
            {
                case CardComponent.Token:
                    writer.WriteValue("token");
                    break;
                case CardComponent.MeldPart:
                    writer.WriteValue("meld_part");
                    break;
                case CardComponent.MeldResult:
                    writer.WriteValue("meld_result");
                    break;
                case CardComponent.ComboPiece:
                    writer.WriteValue("combo_piece");
                    break;
                default:
                    throw new JsonSerializationException($"{val} not found in enum {nameof(CardComponent)}");
            }
        }
    }
}
