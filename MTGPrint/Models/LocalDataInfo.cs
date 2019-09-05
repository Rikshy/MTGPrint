﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MTGPrint.Models
{
    public class LocalDataInfo
    {
        [JsonProperty( "updated_at" )]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty( "card_count" )]
        public long CardCount { get; set; }

        [JsonProperty( "cards" )]
        public List<LocalCard> Cards { get; set; } = new List<LocalCard>();
    }

    public class LocalCard
    {
        [JsonProperty( "oracle_id" )]
        public Guid OracleId { get; set; }

        [JsonProperty( "name" )]
        public string Name { get; set; }

        [JsonProperty( "latest_print" )]
        public DateTimeOffset LatestPrint { get; set; }

        [JsonProperty( "scryfall_uri" )]
        public string ScryUrl { get; set; }

        [JsonProperty( "prints" )]
        public ObservableCollection<CardPrints> Prints { get; set; } = new ObservableCollection<CardPrints>();
    }

    public class CardPrints
    {
        [JsonProperty( "id" )]
        public Guid Id { get; set; }

        [JsonProperty( "set" )]
        public string Set { get; set; }

        [JsonProperty( "set_name" )]
        public string SetName { get; set; }

        [JsonProperty( "downloaded" )]
        public bool Downloaded { get; set; }

        [JsonProperty( "image_uris" )]
        public ImageUrls ImageUrls { get; set; }

        public override string ToString()
        {
            return SetName;
        }
    }
}