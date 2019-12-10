using System.Linq;
using System.Net;
using System.IO;
using System;

using Newtonsoft.Json;

namespace MTGPrint.Helper
{
    public class DecklistGrabber
    {
        #region Grabber
        private abstract class BaseGrabber
        {
            protected abstract string RefineUrl(string importUrl);
            public string Grab(string importUrl)
            {
                var url = RefineUrl(importUrl);
                
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Http.Get;
                var response = (HttpWebResponse)request.GetResponse();
                using var stream = new StreamReader(response.GetResponseStream());
                var responseText = stream.ReadToEnd();

                return PostProcess(responseText);
            }

            protected virtual string PostProcess(string reponse)
                => reponse;
        }

        private class DeckstatsGrabber : BaseGrabber
        {
            protected override string RefineUrl(string importUrl)
                => $"{importUrl}?export_dec=1";
        }
        private class ScryfallGrabber : BaseGrabber
        {
            protected override string RefineUrl(string importUrl)
            {
                var url = importUrl;

                var idx = url.LastIndexOf('?');
                if (idx > 0)
                    url = url.Substring(0, idx);

                idx = url.LastIndexOf('/');
                var deckId = url.Substring(idx + 1);
                return $"https://api.scryfall.com/decks/{deckId}/export/text";
            }
        }
        private class GoldfishGrabber : BaseGrabber
        {
            protected override string RefineUrl(string importUrl)
            {
                var url = importUrl;

                var idx = url.LastIndexOf('#');
                if (idx > 0)
                    url = url.Substring(0, idx);

                idx = url.LastIndexOf('/');
                var deckId = url.Substring(idx + 1);
                return $"https://mtggoldfish.com/deck/download/{deckId}";
            }
        }
        private class AetherGrabber : BaseGrabber
        {
            protected override string RefineUrl(string importUrl)
            {
                var url = importUrl;

                var idx = url.LastIndexOf('/');
                var deckId = url.Substring(idx + 1);
                return $"https://aetherhub.com/Deck/MtgoDeckExport/{deckId}";
            }
        }
        private class TappedoutGrabber : BaseGrabber
        {
            protected override string RefineUrl(string importUrl)
            {
                var url = importUrl;

                var idx = url.LastIndexOf('/');
                if (idx > 0 && !url.Substring(0, idx).EndsWith("mtg-decks"))
                    url = url.Substring(0, idx);

                return $"{url}?fmt=txt";
            }
        }
        private class ArchidektGrabber : BaseGrabber
        {
            protected override string RefineUrl(string importUrl)
            {
                var url = importUrl;

                var idx = url.LastIndexOf('#');
                if (idx > 0)
                    url = url.Substring(0, idx);

                idx = url.LastIndexOf('/');
                var deckId = url.Substring(idx + 1);
                return $"https://www.archidekt.com/api/decks/{deckId}/";
            }

            protected override string PostProcess(string reponse)
            {
                var data = JsonConvert.DeserializeObject<Archidata>(reponse);
                var list = data.Cards.Select(c => $"{c.Quantity} {c.Card.OracleCard.Name}");
                return string.Join(Environment.NewLine, list);
            }

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
        #endregion

        public static string GrabDecklist(string importUrl)
        {
            BaseGrabber grabber = null;
            if (importUrl.StartsWith("https://deckstats.net") || importUrl.StartsWith("https://www.deckstats.net"))
                grabber = new DeckstatsGrabber();
            if (importUrl.StartsWith("https://scryfall.com") || importUrl.StartsWith("https://www.scryfall.com"))
                grabber = new ScryfallGrabber();
            if (importUrl.StartsWith("https://mtggoldfish.com") || importUrl.StartsWith("https://www.mtggoldfish.com"))
                grabber = new GoldfishGrabber();
            if (importUrl.StartsWith("https://aetherhub.com") || importUrl.StartsWith("https://www.aetherhub.com"))
                grabber = new AetherGrabber();
            if (IsTappedOut(importUrl))
                grabber = new TappedoutGrabber();
            if (importUrl.StartsWith("https://archidekt.com") || importUrl.StartsWith("https://www.archidekt.com"))
                grabber = new ArchidektGrabber();

            return grabber.Grab(importUrl);
        }

        private static bool IsTappedOut(string importUrl)
        {
            return importUrl.StartsWith("http://tappedout.net")
                || importUrl.StartsWith("http://www.tappedout.net")
                || importUrl.StartsWith("tappedout.net")
                || importUrl.StartsWith("www.tappedout.net");
        }
    }
}
