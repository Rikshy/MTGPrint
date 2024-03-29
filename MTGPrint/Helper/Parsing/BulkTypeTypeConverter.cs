﻿using System;

using Newtonsoft.Json;

using MTGPrint.Models;

namespace MTGPrint.Helper.Parsing
{
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

            return value switch
            {
                "oracle_cards" => BulkType.OracleCards,
                "rulings" => BulkType.Rulings,
                "all_cards" => BulkType.AllCards,
                "artworks" => BulkType.ArtWorks,
                "default_cards" => BulkType.DefaultCards,
                "unique_artwork" => (object)BulkType.UniqueArtwork,
                _ => throw new JsonSerializationException($"{value} not found in enum {nameof(BulkType)}"),
            };
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
                case BulkType.ArtWorks:
                    writer.WriteValue("artworks");
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
