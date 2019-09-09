using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MTGPrint.Models
{
    public class ScryCard
    {
        [JsonProperty( "id" )]
        public Guid Id { get; set; }

        [JsonProperty( "oracle_id" )]
        public Guid OracleId { get; set; }

        [JsonProperty( "name" )]
        public string Name { get; set; }

        [JsonProperty( "released_at" )]
        public DateTimeOffset ReleasedAt { get; set; }

        [JsonProperty( "set" )]
        public string Set { get; set; }

        [JsonProperty( "set_name" )]
        public string SetName { get; set; }

        [JsonProperty( "layout" )]
        [JsonConverter( typeof( CardLayoutTypeConverter ) )]
        public CardLayout Layout { get; set; }


        [JsonProperty( "uri" )]
        public string ApiUrl { get; set; }

        [JsonProperty( "scryfall_uri" )]
        public string ScryUrl { get; set; }


        [JsonProperty( "image_uris" )]
        public ImageUrls ImageUrls { get; set; }


        [JsonProperty( "lang" )]
        public string Lang { get; set; }

        [JsonProperty( "card_faces" )]
        public CardFace[] CardFaces { get; set; }

        [JsonProperty( "all_parts" )]
        public List<CardParts> Parts { get; set; }
    }

    public class ImageUrls
    {
        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("normal")]
        public string Normal { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("png")]
        public string Png { get; set; }

        [JsonProperty("art_crop")]
        public string ArtCrop { get; set; }

        [JsonProperty("border_crop")]
        public string BorderCrop { get; set; }
    }

    public class CardParts
    {
        [JsonProperty( "id" )]
        public Guid Id { get; set; }

        [JsonProperty( "name" )]
        public string Name { get; set; }

        [JsonProperty( "component" )]
        [JsonConverter( typeof( CardComponentTypeConverter ) )]
        public CardComponent Component { get; set; }
    }
    public enum CardComponent
    {
        Token,
        MeldPart,
        MeldResult,
        ComboPiece
    }
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

    public class CardFace
    {
        [JsonProperty( "name" )]
        public string Name { get; set; }


        [JsonProperty( "image_uris" )]
        public ImageUrls ImageUrls { get; set; }
    }
    public enum CardLayout
    {
        Normal,
        Split,
        Flip,
        Transform,
        Meld,
        Leveler,
        Saga,
        Adventure,
        Planar,
        Scheme,
        Vanguard,
        Token,
        DoubleFacedToken,
        Emblem,
        Augment,
        Host,
        ArtSeries,
        DoubleSided
    }
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