using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

using Newtonsoft.Json;

using Caliburn.Micro;

using MTGPrint.Models;

namespace MTGPrint.Helper
{
    public class DecklistGrabber
    {
        private static readonly List<BaseGrabber> grabbers = new List<BaseGrabber>
        {
            new TextGrabber(),
            new DeckstatsGrabber(),
            new ScryfallGrabber(),
            new GoldfishGrabber(),
            new AetherGrabber(),
            new TappedoutGrabber(),
            new ArchidektGrabber()
        };

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

            public abstract GrabMethod GrabMethod { get; }

            public abstract bool IsMatching(string input);

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
            public override GrabMethod GrabMethod => GrabMethod.Text;

            public override bool IsMatching(string input) => true;

            protected override string GrabDeckList(string input) 
                => input;
        }

        private abstract class BaseWebGrabber : BaseGrabber
        {
            public override GrabMethod GrabMethod => GrabMethod.Url;

            protected abstract string RefineUrl(string importUrl);
            protected override string GrabDeckList(string importUrl)
            {
                var url = RefineUrl(importUrl);
                
                var responseText = WebHelper.Get(url);

                return RefineResponse(responseText);
            }

            protected virtual string RefineResponse(string reponse)
                => reponse;
        }

        private class DeckstatsGrabber : BaseWebGrabber
        {
            protected override string RefineUrl(string importUrl)
                => importUrl.Contains("?") ? $"{importUrl}&export_dec=1" : $"{importUrl}?export_dec=1";

            public override bool IsMatching(string url) 
                => url.StartsWith( "https://deckstats.net" ) || url.StartsWith( "https://www.deckstats.net" );
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

            public override bool IsMatching(string url)
                => url.StartsWith("https://scryfall.com") || url.StartsWith("https://www.scryfall.com");
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

            public override bool IsMatching(string url)
                => url.StartsWith("https://mtggoldfish.com") || url.StartsWith("https://www.mtggoldfish.com");
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

            public override bool IsMatching(string url)
                => url.StartsWith("https://aetherhub.com") || url.StartsWith("https://www.aetherhub.com");
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

            public override bool IsMatching(string url)
            {
                return url.StartsWith("http://tappedout.net")
                    || url.StartsWith("http://www.tappedout.net")
                    || url.StartsWith("tappedout.net")
                    || url.StartsWith("www.tappedout.net");
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
        #endregion

        public static IEnumerable<DeckCard> Grab(string source, GrabMethod method, out IEnumerable<string> errors)
        {
            var grabber = grabbers.FirstOrDefault(g => g.GrabMethod == method && g.IsMatching(source));

            if (grabber == null)
                throw new ApplicationException("No decklistgrabber found for your input.");

            return grabber.Grab(source, out errors);
        }
    }

    public enum GrabMethod
    {
        Text = 0,
        Url
    }
}
