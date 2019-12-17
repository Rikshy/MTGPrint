using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System;

using Newtonsoft.Json;

using Caliburn.Micro;

using MTGPrint.Models;

namespace MTGPrint.Helper
{
    public class DecklistGrabber
    {
        private abstract class BaseGrabber
        {
            private static LocalDataStorage lds;
            protected LocalDataStorage LocalData
            {
                get
                {
                    if (lds == null)
                        lds = IoC.Get<LocalDataStorage>();
                    return lds;
                }
            }

            public IEnumerable<DeckCard> Grab(string input, out IEnumerable<string> errors)
                => Parse(GrabDeckList(input), out errors);

            protected abstract string GrabDeckList(string input);

            protected virtual IEnumerable<DeckCard> Parse(string cardList, out IEnumerable<string> errors)
            {
                var splits = cardList.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                var deckCards = new List<DeckCard>();
                var errs = new List<string>();

                foreach (string line in splits.Where(l => !l.StartsWith("//") && !string.IsNullOrEmpty(l.Trim())))
                {
                    bool isCommander = false;

                    var match = Regex.Match( line, "([0-9]+) (.*)" );
                    if (!match.Success || !int.TryParse(match.Groups[1].Value, out var count) || count <= 0)
                    {
                        errs.Add(line);
                        continue;
                    }

                    //deckstats commander parsing
                    var parsedName = match.Groups[2].Value;
                    if (parsedName.EndsWith("#!Commander"))
                    {
                        isCommander = true;
                        parsedName = parsedName.Substring(0, parsedName.IndexOf("#!Commander"));
                    }

                    var card = LocalData.LocalCards.FirstOrDefault(c => c.Name.ToUpper() == parsedName.Trim().ToUpper());
                    if (card == null)
                    {
                        errs.Add($"card '{line}' not found");
                        continue;
                    }

                    var first = card.Prints.FirstOrDefault();
                    if (first == null)
                    {
                        errs.Add($"no print found for card '{card.Name}'");
                        continue;
                    }

                    var dc = new DeckCard
                    {
                        OracleId = card.OracleId,
                        LocalData = card,
                        SelectedPrintId = card.DefaultPrint ?? first.Id,
                        Count = count
                    };
                    dc.Prints.AddRange(card.Prints);

                    if (isCommander)
                        deckCards.Insert(0, dc);
                    else
                        deckCards.Add(dc);

                    if (first.ChildUrls != null)
                    {
                        dc = new DeckCard
                        {
                            OracleId = card.OracleId,
                            IsChild = true
                        };

                        if (isCommander)
                            deckCards.Insert(1, dc);
                        else
                            deckCards.Add(dc);
                    }
                }

                errors = errs;
                return deckCards;
            }
        }
        #region Grabber
        private class TextGrabber : BaseGrabber
        {
            protected override string GrabDeckList(string input) 
                => input;
        }

        private abstract class BaseWebGrabber : BaseGrabber
        {
            protected abstract string RefineUrl(string importUrl);
            protected override string GrabDeckList(string importUrl)
            {
                var url = RefineUrl(importUrl);
                
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Http.Get;
                var response = (HttpWebResponse)request.GetResponse();
                using var stream = new StreamReader(response.GetResponseStream());
                var responseText = stream.ReadToEnd();

                return RefineResponse(responseText);
            }

            protected virtual string RefineResponse(string reponse)
                => reponse;
        }

        private class DeckstatsGrabber : BaseWebGrabber
        {
            protected override string RefineUrl(string importUrl)
                => importUrl.Contains("?") ? $"{importUrl}&export_dec=1" : $"{importUrl}?export_dec=1";
        }
        private class ScryfallGrabber : BaseWebGrabber
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
        private class GoldfishGrabber : BaseWebGrabber
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
        private class AetherGrabber : BaseWebGrabber
        {
            protected override string RefineUrl(string importUrl)
            {
                var url = importUrl;

                var idx = url.LastIndexOf('/');
                var deckId = url.Substring(idx + 1);
                return $"https://aetherhub.com/Deck/MtgoDeckExport/{deckId}";
            }
        }
        private class TappedoutGrabber : BaseWebGrabber
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
        private class ArchidektGrabber : BaseWebGrabber
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

            protected override string RefineResponse(string reponse)
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

        public static IEnumerable<DeckCard> GrabDecklist(string importUrl, Method method, out IEnumerable<string> errors)
        {
            BaseGrabber grabber = null;
            if (method == Method.Url)
            {
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
            }
            else
                grabber = new TextGrabber();

            if (grabber == null)
                throw new ApplicationException("No decklistgrabber found for your input.");

            return grabber.Grab(importUrl, out errors);
        }

        private static bool IsTappedOut(string importUrl)
        {
            return importUrl.StartsWith("http://tappedout.net")
                || importUrl.StartsWith("http://www.tappedout.net")
                || importUrl.StartsWith("tappedout.net")
                || importUrl.StartsWith("www.tappedout.net");
        }
        public enum Method
        {
            Text = 0,
            Url
        }
    }
}
