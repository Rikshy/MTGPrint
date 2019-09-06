﻿

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;

using MTGPrint.Models;

using Newtonsoft.Json;

namespace MTGPrint
{
    public class MainModel
    {
        private const string LOCALDATA = @"data\localdata.json";
        public ScryfallClient Scry { get; } = new ScryfallClient();

        private LocalDataInfo localData;

        private BackgroundWorker tmpWorker;
        //private readonly Downloader downloader = new Downloader();

        //public MainModel()
        //{
        //    //downloader.DownloadsCompleted += delegate { OnWorkFinished(); };
        //}

        //public event EventHandler WorkFinished;
        public event EventHandler LocalDataUpdated;

        public Deck Deck { get; } = new Deck();

        public bool CheckForUpdates() { return UpdateCheck(out _); }

        public void UpdateBulkData()
        {
            if (!UpdateCheck(out var bulkInfo))
                return;

            if (!Directory.Exists(@"data"))
                Directory.CreateDirectory("data");

            var bulkFile = $@"data\default-temp.json";

            tmpWorker = new BackgroundWorker();
            tmpWorker.DoWork += delegate
            {
                using (var wc = new WebClient())
                    wc.DownloadFile(bulkInfo.PermalinkUri, bulkFile);

                var cards = JsonConvert.DeserializeObject<ScryCard[]>(File.ReadAllText(bulkFile));

                if (localData != null && localData.CardCount != cards.LongLength)
                    return;

                ConvertToLocal(bulkInfo.UpdatedAt, cards);

                File.WriteAllText(LOCALDATA, JsonConvert.SerializeObject(localData, Formatting.Indented));
                File.Delete(bulkFile);
            };
            tmpWorker.RunWorkerCompleted += TmpWorkerFinished;
            tmpWorker.RunWorkerAsync();
        }

        public void AddCardsToDeck(string cardList, out List<string> errors)
        {
            var deckCards = ParseCardList(cardList, out errors);
            deckCards.ForEach(dc => Deck.Cards.Add(dc));
            Deck.HasChanges = true;
        }

        public void SaveDeck(string path)
        {
            var savePath = string.IsNullOrEmpty(path) ? Deck.FileName : path;
            Deck.FileName = savePath;
            File.WriteAllText(savePath, JsonConvert.SerializeObject(Deck));
            Deck.HasChanges = false;
        }

        public void OpenDeck(string path)
        {
            var tempDeck = JsonConvert.DeserializeObject<Deck>(File.ReadAllText(path));
            if (!tempDeck.Cards.Any())
                throw new FileLoadException("invalid deck file");
            Deck.Cards.Clear();
            foreach (var c in tempDeck.Cards)
                Deck.Cards.Add(c);
            Deck.FileName = path;
            Deck.HasChanges = false;
        }

        //public void LoadDeckPrints(Deck deck)
        //{
        //    foreach (var dc in deck.Cards)
        //    {
        //        var lcard = localData.Cards.FirstOrDefault( lc => lc.OracleId == dc.OracleId );
        //        if (lcard != null) dc.Prints = lcard.Prints;
        //    }
        //}

        //private void OnWorkFinished()
        //{
        //    WorkFinished?.Invoke( this, EventArgs.Empty );
        //}

        private bool UpdateCheck(out Bulk bulkInfo)
        {
            bulkInfo = Scry.GetBulkInfo().Data.First(b => b.Type == BulkType.DefaultCards);

            if (localData == null)
                LoadLocalData();

            return localData == null || bulkInfo.UpdatedAt < localData.UpdatedAt;
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
                var lcard = localData.Cards.FirstOrDefault( c => c.OracleId == card.OracleId );
                if ( lcard == null )
                {
                    lcard = new LocalCard
                    {
                        OracleId = card.OracleId,
                        Name = card.Name,
                        ScryUrl = card.ScryUrl,
                        LatestPrint = card.ReleasedAt
                    };
                    localData.Cards.Add( lcard );
                }
                else if ( card.ReleasedAt > lcard.LatestPrint )
                    lcard.LatestPrint = card.ReleasedAt;

                if (lcard.Prints.All(p => p.Set != card.Set))
                {
                    lcard.Prints.Add( new CardPrints
                    {
                        Id = card.Id,
                        Set = card.Set,
                        SetName = card.SetName,
                        ImageUrls = card.ImageUrls
                    } );
                }
            }
        }

        private List<DeckCard> ParseCardList(string cardList, out List<string> errors)
        {
            var splits = cardList.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var deckCards = new List<DeckCard>();
            errors = new List<string>();

            foreach (string line in splits)
            {
                var ci = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (ci.Length != 2)
                {
                    errors.Add(line);
                    continue;
                }

                if (!int.TryParse(ci[0].Trim(), out var count))
                {
                    errors.Add(line);
                    continue;
                }

                var card = localData.Cards.FirstOrDefault(c => c.Name.ToUpper() == ci[1].Trim().ToUpper());
                if (card == null)
                {
                    errors.Add(line);
                    continue;
                }

                var first = card.Prints.FirstOrDefault();
                if (first == null)
                {
                    errors.Add($"no prints found? {card.Name}");
                    continue;
                }

                var dc = new DeckCard
                         {
                                     OracleId = card.OracleId,
                                     SelectPrint = first,
                                     Prints = card.Prints,
                                     Count = count
                         };

                deckCards.Add(dc);
            }

            return deckCards;
        }

        private void TmpWorkerFinished(object sender, RunWorkerCompletedEventArgs args)
        {
            LocalDataUpdated?.Invoke(this, EventArgs.Empty);
            tmpWorker.Dispose();
        }

        //private void DownloadPrintSet(Guid oracleId, IEnumerable<CardPrints> prints)
        //{
        //    using ( var wc = new WebClient() )
        //    {
        //        foreach ( var print in prints )
        //        {
        //            var printSetDir = $@"data\prints\{oracleId}\{print.Id}";
        //            if ( !print.Downloaded )
        //            {
        //                Directory.CreateDirectory( printSetDir );
        //                downloader.QueueDownload( print.ImageUrls.Png, Path.Combine( printSetDir, "png.png" ) );
        //                downloader.QueueDownload( print.ImageUrls.BorderCrop, Path.Combine( printSetDir, "border.jpg" ) );
        //                downloader.QueueDownload( print.ImageUrls.ArtCrop, Path.Combine( printSetDir, "art.jpg" ) );
        //                print.Downloaded = true;
        //            }
        //        }
        //    }
        //}
    }
}
