using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.IO;
using System;

using Newtonsoft.Json;

using MTGPrint.Models;

namespace MTGPrint
{
    public class LocalDataStorage
    {
        private const string LOCALDATA = @"data\localdata.json";
        private static readonly WebClient cardLoader = new WebClient();

        private const int LOCALDATA_VERSION = 2;

        public LocalDataStorage()
        {
            updateWorker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                var bulkFile = $@"data\default-temp.json";
                var bulkInfo = e.Argument as Bulk;
                cardLoader.DownloadFile( bulkInfo.PermalinkUri, bulkFile );

                var cards = JsonConvert.DeserializeObject<ScryCard[]>( File.ReadAllText( bulkFile ) );

                ConvertToLocal( bulkInfo.UpdatedAt, cards );

                localData.Version = LOCALDATA_VERSION;

                HasChanges = true;
                SaveLocalData();
                File.Delete( bulkFile );
            };
            updateWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
            {
                LocalDataUpdated?.Invoke( null, args );
            };
        }

        private readonly ScryfallClient scry = new ScryfallClient();

        private LocalDataInfo localData;

        public bool HasChanges { get; set; }

        public List<LocalCard> LocalCards => localData.Cards;

        private readonly BackgroundWorker updateWorker = new BackgroundWorker();
        public event EventHandler LocalDataUpdated;

        public bool CheckForUpdates() { return UpdateCheck(out _); }

        public void UpdateBulkData()
        {
            if (!UpdateCheck(out var bulkInfo))
                return;

            if (!Directory.Exists(@"data"))
                Directory.CreateDirectory("data");

            updateWorker.RunWorkerAsync( bulkInfo );
        }

        public void SaveLocalData()
        {
            if (HasChanges)
                File.WriteAllText( LOCALDATA, JsonConvert.SerializeObject( localData, Formatting.Indented ) );
            HasChanges = false;
        }

        public List<DeckCard> ParseCardList(string cardList, out List<string> errors)
        {
            var splits = cardList.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var deckCards = new List<DeckCard>();
            errors = new List<string>();

            foreach (string line in splits.Where(l => !l.StartsWith("//") && !string.IsNullOrEmpty(l.Trim())))
            {
                bool isCommander = false;

                var match = Regex.Match( line, "([0-9]+) (.*)" );
                if (!match.Success || !int.TryParse(match.Groups[1].Value, out var count) || count <= 0)
                {
                    errors.Add(line);
                    continue;
                }

                //deckstats commander parsing
                var parsedName = match.Groups[2].Value;
                if (parsedName.EndsWith("#!Commander"))
                {
                    isCommander = true;
                    parsedName = parsedName.Substring(0, parsedName.IndexOf("#!Commander"));
                }

                var card = localData.Cards.FirstOrDefault(c => c.Name.ToUpper() == parsedName.Trim().ToUpper());
                if (card == null)
                {
                    errors.Add($"card '{line}' not found");
                    continue;
                }

                var first = card.Prints.FirstOrDefault();
                if (first == null)
                {
                    errors.Add($"no print found for card '{card.Name}'");
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

            return deckCards;
        }
        
        private bool UpdateCheck(out Bulk bulkInfo)
        {
            bulkInfo = scry.GetBulkInfo().Data.First(b => b.Type == BulkType.DefaultCards);

            if (localData == null)
                LoadLocalData();

            return localData == null || bulkInfo.UpdatedAt < localData.UpdatedAt || localData.Version != LOCALDATA_VERSION;
        }

        private void LoadLocalData()
        {
            if ( !File.Exists( LOCALDATA ) )
                return;

            localData = JsonConvert.DeserializeObject<LocalDataInfo>( File.ReadAllText( LOCALDATA ) );
        }

        private void ConvertToLocal(DateTimeOffset updatedAt, ScryCard[] cards)
        {
            if ( localData == null )
                localData = new LocalDataInfo();

            localData.UpdatedAt = updatedAt;
            localData.CardCount = cards.LongLength;

            foreach (var card in cards)
            {
                var lcard = card.Layout != CardLayout.Token 
                    ? localData.Cards.FirstOrDefault( c => c.OracleId == card.OracleId )
                    : localData.Cards.FirstOrDefault( c => c.Name == card.Name );
                if ( lcard == null )
                {
                    lcard = new LocalCard
                    {
                        OracleId = card.OracleId,
                        Name = card.Name,
                        ScryUrl = card.ScryUrl,
                        LatestPrint = card.ReleasedAt,
                        Parts = card.Parts
                    };
                    localData.Cards.Add( lcard );
                }
                else
                {
                    if ( card.ReleasedAt > lcard.LatestPrint )
                        lcard.LatestPrint = card.ReleasedAt;

                    if ( card.Parts != null )
                    {
                        if ( lcard.Parts == null )
                            lcard.Parts = new List<CardParts>();

                        lcard.Parts.AddRange( card.Parts.Where( part => lcard.Parts.All( p => part.Id != p.Id ) ) );
                    }
                }

                var iu = card.ImageUrls;
                ImageUrls child = null;
                if ( card.Layout == CardLayout.Transform )
                {
                    iu = card.CardFaces.First().ImageUrls;
                    child = card.CardFaces.Last().ImageUrls;
                }

                if (lcard.Prints.All( p => p.Id != card.Id ))
                    lcard.Prints.Add( new CardPrint
                    {
                        Id = card.Id,
                        Set = card.Set,
                        SetName = card.SetName,
                        ImageUrls = iu,
                        ChildUrls = child
                    } );
            }
        }
    }
}
