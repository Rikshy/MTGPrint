using System;

using Newtonsoft.Json;

using MTGPrint.Models;

namespace MTGPrint.Helper
{
    public sealed class CardLayoutTypeConverter : JsonConverter
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
                case "normal":
                    return CardLayout.Normal;
                case "split":
                    return CardLayout.Split;
                case "flip":
                    return CardLayout.Flip;
                case "transform":
                    return CardLayout.Transform;
                case "meld":
                    return CardLayout.Meld;
                case "leveler":
                    return CardLayout.Leveler;
                case "saga":
                    return CardLayout.Saga;
                case "adventure":
                    return CardLayout.Adventure;
                case "planar":
                    return CardLayout.Planar;
                case "scheme":
                    return CardLayout.Scheme;
                case "vanguard":
                    return CardLayout.Vanguard;
                case "token":
                    return CardLayout.Token;
                case "double_faced_token":
                    return CardLayout.DoubleFacedToken;
                case "emblem":
                    return CardLayout.Emblem;
                case "augment":
                    return CardLayout.Augment;
                case "host":
                    return CardLayout.Host;
                case "art_series":
                    return CardLayout.ArtSeries;
                case "double_sided":
                    return CardLayout.DoubleSided;
                default:
                    throw new JsonSerializationException( $"{value} not found in enum {nameof( CardLayout )}" );
            }
        }

        public override void WriteJson(JsonWriter writer, object value,
                    JsonSerializer serializer)
        {
            var val = (CardLayout)value;

            switch ( val )
            {
                case CardLayout.Normal:
                    writer.WriteValue( "normal" );
                    break;
                case CardLayout.Split:
                    writer.WriteValue( "split" );
                    break;
                case CardLayout.Flip:
                    writer.WriteValue( "flip" );
                    break;
                case CardLayout.Transform:
                    writer.WriteValue( "transform" );
                    break;
                case CardLayout.Meld:
                    writer.WriteValue( "meld" );
                    break;
                case CardLayout.Leveler:
                    writer.WriteValue( "leveler" );
                    break;
                case CardLayout.Saga:
                    writer.WriteValue( "saga" );
                    break;
                case CardLayout.Adventure:
                    writer.WriteValue( "adventure" );
                    break;
                case CardLayout.Planar:
                    writer.WriteValue( "planar" );
                    break;
                case CardLayout.Scheme:
                    writer.WriteValue( "scheme" );
                    break;
                case CardLayout.Vanguard:
                    writer.WriteValue( "vanguard" );
                    break;
                case CardLayout.Token:
                    writer.WriteValue( "token" );
                    break;
                case CardLayout.DoubleFacedToken:
                    writer.WriteValue( "double_faced_token" );
                    break;
                case CardLayout.Emblem:
                    writer.WriteValue( "emblem" );
                    break;
                case CardLayout.Augment:
                    writer.WriteValue( "augment" );
                    break;
                case CardLayout.Host:
                    writer.WriteValue( "host" );
                    break;
                case CardLayout.ArtSeries:
                    writer.WriteValue( "art_series" );
                    break;
                case CardLayout.DoubleSided:
                    writer.WriteValue( "double_sided" );
                    break;
                default:
                    throw new JsonSerializationException( $"{val} not found in enum {nameof( CardLayout )}" );
            }
        }
    }
}
