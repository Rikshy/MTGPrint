using System.Collections.Generic;
using System;

using Newtonsoft.Json;

using MTGPrint.Helper.Parsing;

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
        public IEnumerable<Bulk> Data { get; set; }
    }
}