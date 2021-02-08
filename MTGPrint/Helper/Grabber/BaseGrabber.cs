using Caliburn.Micro;
using MTGPrint.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MTGPrint.Helper.Grabber
{
    public abstract class BaseGrabber
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

                var match = Regex.Match(line, "([0-9]+) (.*)");
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
}
