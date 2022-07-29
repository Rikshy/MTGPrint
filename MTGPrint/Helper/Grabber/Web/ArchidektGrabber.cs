using Newtonsoft.Json;
using System;
using System.Linq;

namespace MTGPrint.Helper.Grabber.Web
{
    class ArchidektGrabber : BaseWebGrabber
    {
        protected override string RefineUrl(string importUrl)
        {
            var url = importUrl;

            var idx = url.LastIndexOf('#');
            if (idx > 0)
                url = url.Substring(0, idx);

            idx = url.LastIndexOf('/');
            var deckId = url[(idx + 1)..];
            return $"https://www.archidekt.com/api/decks/{deckId}/";
        }

        protected override string RefineResponse(string reponse)
        {
            var data = JsonConvert.DeserializeObject<Archidata>(reponse);
            var list = data.Cards.Select(c => $"{c.Quantity} {c.Card.OracleCard.Name}");
            return string.Join(Environment.NewLine, list);
        }

        public override bool IsMatching(string url)
            => url.StartsWith("https://archidekt.com") || url.StartsWith("https://www.archidekt.com");

        public class Archidata
        {
            [JsonProperty("cards")]
            public CardElement[] Cards { get; set; }
        }

        public class CardElement
        {
            [JsonProperty("card")]
            public CardCard Card { get; set; }

            [JsonProperty("quantity")]
            public long Quantity { get; set; }
        }

        public class CardCard
        {
            [JsonProperty("oracleCard")]
            public OracleCard OracleCard { get; set; }
        }

        public class OracleCard
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}
