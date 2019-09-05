using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MTGPrint.Models
{
    public class Bulk
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(BulkTypeTypeConverter))]
        public BulkType Type { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("compressed_size")]
        public long CompressedSize { get; set; }

        [JsonProperty("permalink_uri")]
        public Uri PermalinkUri { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("content_encoding")]
        public string ContentEncoding { get; set; }

    }

    public class BulkBase
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("data")]
        public Bulk[] Data { get; set; }
    }

    public enum BulkType
    {
        OracleCards,
        Rulings,
        AllCards,
        DefaultCards
    }
    public sealed class BulkTypeTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType,
                    object existingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;

            switch (value)
            {
            case "oracle_cards":
                return BulkType.OracleCards;
            case "rulings":
                return BulkType.Rulings;
            case "all_cards":
                return BulkType.AllCards;
            case "default_cards":
                return BulkType.DefaultCards;
            default:
                throw new JsonSerializationException($"{value} not found in enum {nameof(BulkType)}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value,
                    JsonSerializer serializer)
        {
            var val = (BulkType)value;

            switch (val)
            {
            case BulkType.OracleCards:
                writer.WriteValue("oracle_cards");
                break;
            case BulkType.Rulings:
                writer.WriteValue("rulings");
                break;
            case BulkType.AllCards:
                writer.WriteValue("all_cards");
                break;
            case BulkType.DefaultCards:
                writer.WriteValue("default_cards");
                break;
            default:
                throw new JsonSerializationException($"{val} not found in enum {nameof(BulkType)}");
            }
        }
    }
}