﻿using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System;

using Newtonsoft.Json;

using Caliburn.Micro;

namespace MTGPrint.Models
{
    public class LocalDataInfo
    {
        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.MinValue;

        [JsonProperty("card_count")]
        public long CardCount { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("cards")]
        public List<LocalCard> Cards { get; set; } = new List<LocalCard>();
    }

    public class LocalCard
    {
        [JsonProperty("oracle_id")]
        public Guid OracleId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("latest_print")]
        public DateTimeOffset LatestPrint { get; set; }

        [JsonProperty("default_print")]
        public Guid? DefaultPrint { get; set; }

        [JsonProperty("scryfall_uri")]
        public Uri ScryUrl { get; set; }

        [JsonProperty("all_parts")]
        public List<CardParts> Parts { get; set; } = new List<CardParts>();

        [JsonProperty("prints")]
        public BindableCollection<CardPrint> Prints { get; set; } = new BindableCollection<CardPrint>();

        [JsonProperty("is_custom")]
        public bool IsCustom { get; set; } = false;

        [JsonIgnore]
        public bool IsOfficial => !IsCustom;

        public override string ToString()
            => Name;
    }

    public class CardPrint
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("set")]
        public string Set { get; set; }

        [JsonProperty("set_name")]
        public string SetName { get; set; }

        [JsonProperty("child")]
        public ImageUrls ChildUrls { get; set; }

        [JsonProperty("image_uris")]
        public ImageUrls ImageUrls { get; set; }

        [JsonIgnore]
        public object ImageSource
        {
            get
            {
                if (IsCustom)
                {
                    var fp = Path.Combine(Environment.CurrentDirectory, ImageUrls.Large);
                    if (File.Exists(fp))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = new Uri(fp);
                        image.EndInit();
                        return image;
                    }
                    return "";
                }
                else return ImageUrls.Large;
            }
        }

        [JsonProperty("gameplay_info")]
        public GameplayInfo Gameplay { get; set; }

        [JsonProperty("is_custom")]
        public bool IsCustom { get; set; } = false;

        [JsonIgnore]
        public bool IsOfficial => !IsCustom;

        public override string ToString()
            => SetName;

        public static CardPrint CreateCustom(string localPath)
        {
            var baseDir = Path.Combine(Environment.CurrentDirectory, @"data\custom_prints");
            var printId = Guid.NewGuid();
            var crop = new Rectangle(9, 9, 357, 505);

            using var img_stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
            using var img = new Bitmap(img_stream);

            if (img.Width != 375 && img.Height != 523)
                throw new ArgumentException("Unsupported image resolution");

            using var img_crop = img.Clone(crop, img.PixelFormat);

            var normPath = Path.Combine(baseDir, $"{printId}.png");
            var cropPath = Path.Combine(baseDir, $"{printId}-crop.png");

            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            img.Save(normPath, ImageFormat.Png);
            img_crop.Save(cropPath, ImageFormat.Png);

            var normRelPath = normPath.Substring(Environment.CurrentDirectory.Length + 1);
            var cropRelPath = cropPath.Substring(Environment.CurrentDirectory.Length + 1);

            var cp = new CardPrint
            {
                Id = printId,
                Set = "CUS",
                SetName = "Custom Set",
                IsCustom = true,
                ImageUrls = new ImageUrls
                {
                    Small = normRelPath,
                    Large = normRelPath,
                    Normal = normRelPath,
                    Png = normRelPath,
                    BorderCrop = cropRelPath
                }
            };

            return cp;
        }
    }

    public class GameplayInfo
    {
        [JsonProperty("cmc")]
        public float CMC { get; set; }

        [JsonProperty("mana_cost")]
        public string ManaCost { get; set; }

        [JsonProperty("colors")]
        public IEnumerable<string> Colors { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
