using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGPrint.Helper.Grabber.Web
{
    class ScryfallGrabber : BaseWebGrabber
    {
        protected override string RefineUrl(string importUrl)
        {
            var url = importUrl;

            var idx = url.LastIndexOf('?');
            if (idx > 0)
                url = url.Substring(0, idx);

            idx = url.LastIndexOf('/');
            var deckId = url.Substring(idx + 1);
            return $"https://api.scryfall.com/decks/{deckId}/export/json";
        }

        protected override string RefineResponse(string reponse)
        {
            var data = JsonConvert.DeserializeObject<ScryDeckData>(reponse);
            var list = new List<string>();
            list.AddRange(data.Entries.Commanders.Select(c => c.RawText));
            list.AddRange(data.Entries.Nonlands.Select(c => c.RawText));
            list.AddRange(data.Entries.Lands.Select(c => c.RawText));
            list.AddRange(data.Entries.Maybeboard.Select(c => c.RawText));
            return string.Join(Environment.NewLine, list);
        }

        public override bool IsMatching(string url)
            => url.StartsWith("https://scryfall.com") || url.StartsWith("https://www.scryfall.com");

        public class ScryDeckData
        {
            [JsonProperty("entries")]
            public Entries Entries { get; set; }
        }

        public class Entries
        {
            [JsonProperty("commanders")]
            public List<Entry> Commanders { get; set; }

            [JsonProperty("nonlands")]
            public List<Entry> Nonlands { get; set; }

            [JsonProperty("maybeboard")]
            public List<Entry> Maybeboard { get; set; }

            [JsonProperty("lands")]
            public List<Entry> Lands { get; set; }
        }

        public class Entry
        {
            [JsonProperty("raw_text")]
            public string RawText { get; set; }
        }
    }
}
