using System;

using Newtonsoft.Json;

using MTGPrint.Models;

namespace MTGPrint.Helper.Parsing
{
    public sealed class CardLayoutTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(string);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;

            return value switch
            {
                "normal" => CardLayout.Normal,
                "split" => CardLayout.Split,
                "flip" => CardLayout.Flip,
                "transform" => CardLayout.Transform,
                "meld" => CardLayout.Meld,
                "leveler" => CardLayout.Leveler,
                "saga" => CardLayout.Saga,
                "adventure" => CardLayout.Adventure,
                "planar" => CardLayout.Planar,
                "scheme" => CardLayout.Scheme,
                "vanguard" => CardLayout.Vanguard,
                "token" => CardLayout.Token,
                "double_faced_token" => CardLayout.DoubleFacedToken,
                "emblem" => CardLayout.Emblem,
                "augment" => CardLayout.Augment,
                "host" => CardLayout.Host,
                "art_series" => CardLayout.ArtSeries,
                "double_sided" => CardLayout.DoubleSided,
                "modal_dfc" => CardLayout.ModalDualface,
                "class" => CardLayout.Class,
                _ => throw new JsonSerializationException($"{value} not found in enum {nameof(CardLayout)}"),
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var val = (CardLayout)value;

            switch (val)
            {
                case CardLayout.Normal:
                    writer.WriteValue("normal");
                    break;
                case CardLayout.Split:
                    writer.WriteValue("split");
                    break;
                case CardLayout.Flip:
                    writer.WriteValue("flip");
                    break;
                case CardLayout.Transform:
                    writer.WriteValue("transform");
                    break;
                case CardLayout.Meld:
                    writer.WriteValue("meld");
                    break;
                case CardLayout.Leveler:
                    writer.WriteValue("leveler");
                    break;
                case CardLayout.Saga:
                    writer.WriteValue("saga");
                    break;
                case CardLayout.Adventure:
                    writer.WriteValue("adventure");
                    break;
                case CardLayout.Planar:
                    writer.WriteValue("planar");
                    break;
                case CardLayout.Scheme:
                    writer.WriteValue("scheme");
                    break;
                case CardLayout.Vanguard:
                    writer.WriteValue("vanguard");
                    break;
                case CardLayout.Token:
                    writer.WriteValue("token");
                    break;
                case CardLayout.DoubleFacedToken:
                    writer.WriteValue("double_faced_token");
                    break;
                case CardLayout.Emblem:
                    writer.WriteValue("emblem");
                    break;
                case CardLayout.Augment:
                    writer.WriteValue("augment");
                    break;
                case CardLayout.Host:
                    writer.WriteValue("host");
                    break;
                case CardLayout.ArtSeries:
                    writer.WriteValue("art_series");
                    break;
                case CardLayout.DoubleSided:
                    writer.WriteValue("double_sided");
                    break;
                case CardLayout.ModalDualface:
                    writer.WriteValue("modal_dfc");
                    break;
                case CardLayout.Class:
                    writer.WriteValue("class");
                    break;
                default:
                    throw new JsonSerializationException($"{val} not found in enum {nameof(CardLayout)}");
            }
        }
    }
}
