

using System;
using System.Collections.Generic;
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
        private Downloader downloader = new Downloader();

        public MainModel()
        {
            downloader.DownloadsCompleted += delegate { OnWorkFinished(); };
        }

        public event EventHandler WorkFinished;

        public void UpdateBulkData()
        {
            var bulkInfo = Scry.GetBulkInfo().Data.First(b => b.Type == BulkType.DefaultCards);

            if ( localData == null)
                LoadLocalData();

            if ( localData != null && bulkInfo.UpdatedAt >= localData.UpdatedAt)
                return;

            if (!Directory.Exists(@"data"))
                Directory.CreateDirectory("data");

            var bulkFile = $@"data\default-temp.json";

            using (var wc = new WebClient())
                wc.DownloadFile(bulkInfo.PermalinkUri.ToString(), bulkFile );
            
            var cards = JsonConvert.DeserializeObject<ScryCard[]>(File.ReadAllText( bulkFile ) );

            if ( localData != null && localData.CardCount != cards.LongLength )
                return;

            ConvertToLocal( bulkInfo.UpdatedAt, cards );

            File.WriteAllText( LOCALDATA, JsonConvert.SerializeObject( localData, Formatting.Indented ) );
            File.Delete( bulkFile );
        }

        public List<DeckCard> ParseCardList(string cardList, out List<string> errors)
        {
            var splits = cardList.Split( new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries );

            var deckCards = new List<DeckCard>();
            errors = new List<string>();

            foreach (string line in splits)
            {
                var ci = line.Split( new[] { " " }, StringSplitOptions.RemoveEmptyEntries );
                if ( ci.Length != 2 )
                {
                    errors.Add( $"could not parse: {line}" );
                    continue;
                }

                if ( !int.TryParse( ci[0].Trim(), out var count ) )
                {
                    errors.Add( $"could not parse number: {ci[0]}" );
                    continue;
                }

                var card = localData.Cards.FirstOrDefault( c => c.Name.ToUpper() == ci[1].Trim().ToUpper() );
                if ( card == null )
                {
                    errors.Add( $"no card found with name: {ci[1]}" );
                    continue;
                }

                var first = card.Prints.FirstOrDefault();
                if ( first == null )
                {
                    errors.Add( $"no prints found? {card.Name}" );
                    continue;
                }

                //DownloadPrintSet( card.OracleId, card.Prints );

                var dc = new DeckCard
                {
                    OracleId = card.OracleId,
                    SelectPrint = first, //new DeckPrint { OracleId = card.OracleId, Id = first.Id, Set = first.Set, SetName = first.SetName },
                    Prints = card.Prints,
                    Count = count
                };

                //card.Prints.ForEach( cp => dc.Prints.Add( new DeckPrint { OracleId = card.OracleId, Id = cp.Id, Set = cp.Set, SetName = cp.SetName } ) );
                deckCards.Add( dc );
            }

            return deckCards;
        }

        public void LoadDeckPrints(Deck deck)
        {
            foreach (DeckCard dc in deck.Cards)
            {
                var lcard = localData.Cards.FirstOrDefault( lc => lc.OracleId == dc.OracleId );
                dc.Prints = lcard.Prints;
                //lcard.Prints.ForEach( cp => dc.Prints.Add( new DeckPrint { OracleId = lcard.OracleId, Id = cp.Id, Set = cp.Set, SetName = cp.SetName } ) );
            }
        }

        private void OnWorkFinished()
        {
            WorkFinished?.Invoke( this, EventArgs.Empty );
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

                if (!lcard.Prints.Any(p => p.Set == card.Set))
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

        private void DownloadPrintSet(Guid oracleId, IEnumerable<CardPrints> prints)
        {
            using ( var wc = new WebClient() )
            {
                foreach ( var print in prints )
                {
                    var printSetDir = $@"data\prints\{oracleId}\{print.Id}";
                    if ( !print.Downloaded )
                    {
                        Directory.CreateDirectory( printSetDir );
                        downloader.QueueDownload( print.ImageUrls.Png, Path.Combine( printSetDir, "png.png" ) );
                        downloader.QueueDownload( print.ImageUrls.BorderCrop, Path.Combine( printSetDir, "border.jpg" ) );
                        downloader.QueueDownload( print.ImageUrls.ArtCrop, Path.Combine( printSetDir, "art.jpg" ) );
                        print.Downloaded = true;
                    }
                }
            }
        }
    }
}
