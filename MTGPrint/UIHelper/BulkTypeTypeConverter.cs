using System;

using Newtonsoft.Json;

using MTGPrint.Models;

namespace MTGPrint.Helper
{
    public sealed class BulkTypeTypeConverter : JsonConverter
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
                case "oracle_cards":
                    return BulkType.OracleCards;
                case "rulings":
                    return BulkType.Rulings;
                case "all_cards":
                    return BulkType.AllCards;
                case "artworks":
                    return BulkType.ArtWorks;
                case "default_cards":
                    return BulkType.DefaultCards;
                default:
                    throw new JsonSerializationException( $"{value} not found in enum {nameof( BulkType )}" );
            }
        }

        public override void WriteJson(JsonWriter writer, object value,
                    JsonSerializer serializer)
        {
            var val = (BulkType)value;

            switch ( val )
            {
                case BulkType.OracleCards:
                    writer.WriteValue( "oracle_cards" );
                    break;
                case BulkType.Rulings:
                    writer.WriteValue( "rulings" );
                    break;
                case BulkType.AllCards:
                    writer.WriteValue( "all_cards" );
                    break;
                case BulkType.ArtWorks:
                    writer.WriteValue( "artworks" );
                    break;
                case BulkType.DefaultCards:
                    writer.WriteValue( "default_cards" );
                    break;
                default:
                    throw new JsonSerializationException( $"{val} not found in enum {nameof( BulkType )}" );
            }
        }
    }
}
