using System;

using Newtonsoft.Json;

namespace MTGPrint.Models
{
    public class ScryCard
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("oracle_id")]
        public Guid OracleId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty( "released_at" )]
        public DateTimeOffset ReleasedAt { get; set; }

        [JsonProperty( "set" )]
        public string Set { get; set; }

        [JsonProperty( "set_name" )]
        public string SetName { get; set; }


        [JsonProperty("uri")]
        public string ApiUrl { get; set; }

        [JsonProperty("scryfall_uri")]
        public string ScryUrl { get; set; }


        [JsonProperty("image_uris")]
        public ImageUrls ImageUrls { get; set; }


        [JsonProperty("lang")]
        public string Lang { get; set; }
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
}