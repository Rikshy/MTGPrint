using MTGPrint.Models;
using Newtonsoft.Json;
using System;

namespace MTGPrint.Helper
{
    public sealed class CardComponentTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof( string );
        }

        public override object ReadJson(JsonReader reader, Type objectType,
                    object existingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;

            switch ( value )
            {
                case "token":
                    return CardComponent.Token;
                case "meld_part":
                    return CardComponent.MeldPart;
                case "meld_result":
                    return CardComponent.MeldResult;
                case "combo_piece":
                    return CardComponent.ComboPiece;
                default:
                    throw new JsonSerializationException( $"{value} not found in enum {nameof( CardComponent )}" );
            }
        }

        public override void WriteJson(JsonWriter writer, object value,
                    JsonSerializer serializer)
        {
            var val = (CardComponent)value;

            switch ( val )
            {
                case CardComponent.Token:
                    writer.WriteValue( "token" );
                    break;
                case CardComponent.MeldPart:
                    writer.WriteValue( "meld_part" );
                    break;
                case CardComponent.MeldResult:
                    writer.WriteValue( "meld_result" );
                    break;
                case CardComponent.ComboPiece:
                    writer.WriteValue( "combo_piece" );
                    break;
                default:
                    throw new JsonSerializationException( $"{val} not found in enum {nameof( CardComponent )}" );
            }
        }
    }
}
