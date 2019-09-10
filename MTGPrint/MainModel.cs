

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MTGPrint.Models;

using Newtonsoft.Json;

namespace MTGPrint
{
    public class MainModel
    {
        private const string LOCALDATA = @"data\localdata.json";
        private const string PRINTSETTINGS = @"data\printsettings.json";

        // 1 inch = 72 point
        private static float MM_TO_POINT = 2.834645669291339F;

        private static float CARD_HEIGHT = 88 * MM_TO_POINT;
        private static float CARD_WIDTH = 63 * MM_TO_POINT;
        private static float CARD_HEIGHT_WOB = 85 * MM_TO_POINT;
        private static float CARD_WIDTH_WOB = 60 * MM_TO_POINT;
        private static float CARD_MARGIN = 1 * MM_TO_POINT;
        private static float PAGE_MARGIN_V = 15 * MM_TO_POINT;
        private static float PAGE_MARGIN_H = 7.5F * MM_TO_POINT;

        private static int LOCALDATA_VERSION = 1;

        public MainModel()
        {
            updateWorker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                var bulkFile = $@"data\default-temp.json";
                var bulkInfo = e.Argument as Bulk;
                wc.DownloadFile( bulkInfo.PermalinkUri, bulkFile );

                var cards = JsonConvert.DeserializeObject<ScryCard[]>( File.ReadAllText( bulkFile ) );

                if ( localData != null && localData.CardCount != cards.LongLength )
                    return;

                ConvertToLocal( bulkInfo.UpdatedAt, cards );

                localData.Version = LOCALDATA_VERSION;

                File.WriteAllText( LOCALDATA, JsonConvert.SerializeObject( localData, Formatting.Indented ) );
                File.Delete( bulkFile );
            };
            updateWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
            {
                LocalDataUpdated?.Invoke( this, args );
            };

            printWorker.DoWork += DoPrintWork;
            printWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
            {
                PrintFinished?.Invoke( this, args );
            };

            artWorker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                var pair = (KeyValuePair<string, string>)e.Argument;
                wc2.DownloadFile( pair.Key, pair.Value );
            };
            artWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
            {
                ArtDownloaded?.Invoke( this, args );
            };
        }

        private WebClient wc = new WebClient();
        private WebClient wc2 = new WebClient();
        private ScryfallClient scry = new ScryfallClient();

        private LocalDataInfo localData;

        private BackgroundWorker updateWorker = new BackgroundWorker();
        public event EventHandler LocalDataUpdated;

        private BackgroundWorker printWorker = new BackgroundWorker();
        public event EventHandler<RunWorkerCompletedEventArgs> PrintFinished;

        private BackgroundWorker artWorker = new BackgroundWorker();
        public event EventHandler<RunWorkerCompletedEventArgs> ArtDownloaded;

        public Deck Deck { get; } = new Deck();

        public bool CheckForUpdates() { return UpdateCheck(out _); }

        public void UpdateBulkData()
        {
            if (!UpdateCheck(out var bulkInfo))
                return;

            if (!Directory.Exists(@"data"))
                Directory.CreateDirectory("data");

            updateWorker.RunWorkerAsync( bulkInfo );
        }

        #region Menu
        public void AddCardsToDeck(string cardList, out List<string> errors)
        {
            var deckCards = ParseCardList(cardList, out var tokens, out errors);
            deckCards.ForEach(dc => Deck.Cards.Add(dc));
            Deck.HasChanges = true;
            foreach ( var tid in tokens )
            {
                if ( !Deck.Tokens.Contains( tid ) )
                    Deck.Tokens.Add( tid );
            }
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
            {
                var lcard = localData.Cards.FirstOrDefault( lc => lc.OracleId == c.OracleId );
                c.Prints = lcard.Prints;
                c.SelectPrint = lcard.Prints.FirstOrDefault( p => p.Id == c.SelectPrint.Id );
                Deck.Cards.Add( c );
            }
            Deck.FileName = path;
            Deck.HasChanges = false;
        }

        public void GenerateTokens()
        {
            foreach (var token in Deck.Tokens)
            {
                var card = localData.Cards.FirstOrDefault( c => c.Prints.Any( cp => cp.Id == token.Id ) );

                Deck.Cards.Add( new DeckCard
                {
                    OracleId = card.OracleId,
                    SelectPrint = card.Prints.First( cp => cp.Id == token.Id ),
                    Prints = card.Prints,
                    Count = 5
                } );
            }
        }

        public void Print(PrintOptions po)
        {
            try
            {
                File.WriteAllText( PRINTSETTINGS, JsonConvert.SerializeObject( po ) );
            }
            catch
            {
                // ignore
            }

            printWorker.RunWorkerAsync( po );
        }
        #endregion

        #region Context
        public void OpenScryfall(DeckCard card)
        {
            var localCard = localData.Cards.FirstOrDefault( lc => lc.Prints.Any( p => p.Id == card.SelectPrint.Id ) );

            if (localCard != null)
            {
                System.Diagnostics.Process.Start( localCard.ScryUrl );
            }
        }

        public void RemoveCardFromDeck(DeckCard card)
        {
            var index = Deck.Cards.IndexOf( card );
            if (Deck.Cards.Count < index && Deck.Cards[index + 1].IsChild )
                Deck.Cards.RemoveAt( index + 1 );
            Deck.Cards.Remove( card );
            Deck.HasChanges = true;
        }

        public void DuplicateCard(DeckCard card)
        {
            var newCard = new DeckCard
            {
                Count = card.Count,
                OracleId = card.OracleId,
                Prints = card.Prints,
                SelectPrint = card.SelectPrint
            };

            Deck.Cards.Add( newCard );


            var index = Deck.Cards.IndexOf( card );
            if ( Deck.Cards.Count > index && Deck.Cards[index + 1].IsChild )
            {
                var child = Deck.Cards[index + 1];
                newCard = new DeckCard
                {
                    IsChild = child.IsChild,
                    OracleId = child.OracleId
                };

                Deck.Cards.Add( newCard );
            }

            Deck.HasChanges = true;
        }

        public void SaveArtCrop(DeckCard card, string filePath)
        {
            string dlPath;
            if ( card.IsChild )
            {
                var index = Deck.Cards.IndexOf( card );
                dlPath = Deck.Cards[index - 1].SelectPrint.ChildUrls.ArtCrop;
            }
            else
                dlPath = card.SelectPrint.ImageUrls.ArtCrop;

            artWorker.RunWorkerAsync( new KeyValuePair<string, string>( dlPath, filePath ) );
        }
        #endregion

        public PrintOptions LoadPrintSettings()
        {
            if ( !File.Exists( PRINTSETTINGS ) )
                return new PrintOptions();
            try
            {
                return JsonConvert.DeserializeObject<PrintOptions>( File.ReadAllText( PRINTSETTINGS ) );
            }
            catch
            {
                return new PrintOptions();
            }
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

                lcard.Prints.Add( new CardPrints
                {
                    Id = card.Id,
                    Set = card.Set,
                    SetName = card.SetName,
                    ImageUrls = iu,
                    ChildUrls = child
                } );
            }
        }

        private List<DeckCard> ParseCardList(string cardList, out List<CardParts> tokens, out List<string> errors)
        {
            var splits = cardList.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var deckCards = new List<DeckCard>();
            tokens = new List<CardParts>();
            errors = new List<string>();

            foreach (string line in splits)
            {
                var match = Regex.Match( line, "([0-9]+) (.*)" );
                if ( !match.Success || !int.TryParse( match.Groups[1].Value, out var count ) || count <= 0 )
                {
                    errors.Add(line);
                    continue;
                }

                var card = localData.Cards.FirstOrDefault(c => c.Name.ToUpper() == match.Groups[2].Value.Trim().ToUpper());
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

                deckCards.Add( dc );

                if ( first.ChildUrls != null )
                {
                    dc = new DeckCard
                    {
                        OracleId = card.OracleId,
                        IsChild = true
                    };
                    deckCards.Add( dc );
                }

                if ( card.Parts != null )
                {
                    var tc = card.Parts.Where( p => p.Component == CardComponent.Token || p.Component == CardComponent.ComboPiece );
                    foreach ( var t in tc )
                    {
                        if ( tokens.All( t1 => t1.Name != t.Name ) && card.Prints.All( cp => cp.Id != t.Id ) )
                            tokens.Add( t );
                    }
                }
            }

            return deckCards;
        }

        private void DoPrintWork(object sender, DoWorkEventArgs args)
        {
            var po = args.Argument as PrintOptions;
            var doc = new Document( PageSize.A4 );

            var cw = po.CardBorder == CardBorder.With
                    ? CARD_WIDTH
                    : CARD_WIDTH_WOB;
            var ch = po.CardBorder == CardBorder.With
                    ? CARD_HEIGHT
                    : CARD_HEIGHT_WOB;
            var cm = po.CardMargin * MM_TO_POINT;

            int cardCount = 0;
            using ( var writer = new FileStream( po.FileName, FileMode.Create ) )
            {
                PdfWriter.GetInstance( doc, writer );
                doc.Open();
                for ( int i = 0; i < Deck.Cards.Count; i++ )
                {
                    string cardUrl;
                    DeckCard currentCard = Deck.Cards[i];

                    if ( !currentCard.CanPrint )
                        continue;

                    if ( currentCard.IsChild )
                    {
                        currentCard = Deck.Cards[i - 1];
                        cardUrl = po.CardBorder == CardBorder.With
                            ? currentCard.SelectPrint.ChildUrls.Normal
                            : currentCard.SelectPrint.ChildUrls.BorderCrop;
                    }
                    else
                        cardUrl = po.CardBorder == CardBorder.With
                            ? currentCard.SelectPrint.ImageUrls.Normal
                            : currentCard.SelectPrint.ImageUrls.BorderCrop;


                    Image img;
                    //get image  
                    using ( var mem = new MemoryStream() )
                    {
                        var b = wc.DownloadData( cardUrl );
                        mem.Write( b, 0, b.Length );
                        mem.Seek( 0, SeekOrigin.Begin );

                        img = Image.GetInstance( mem );
                        img.ScaleToFit( cw, ch );
                    }

                    for ( int j = 0; j < currentCard.Count; j++ )
                    {
                        if ( cardCount != 0 && cardCount % 9 == 0 )
                            doc.NewPage();

                        var x = (cardCount % 3) * (cw + cm) + PAGE_MARGIN_H;
                        var y = ((cardCount / 3) % 3) * (ch + cm) + PAGE_MARGIN_V;

                        img.SetAbsolutePosition( (float)x, (float)y );
                        doc.Add( img );

                        cardCount++;
                    }
                }
                doc.Close();

                args.Result = po;
            }
        }
    }
}
