using System.ComponentModel;
using System.Net.Http;
using System.IO;
using System;

using iText.Layout.Element;
using iText.Kernel.Pdf;
using iText.IO.Image;
using iText.Layout;

using MTGPrint.Models;

namespace MTGPrint.Helper
{
    public class BackgroundPrinter
    {
        // 1 inch = 72 point
        private const float MM_TO_POINT = 2.834645669291339F;

        private const float CARD_HEIGHT = 88 * MM_TO_POINT;
        private const float CARD_WIDTH = 63 * MM_TO_POINT;
        private const float CARD_HEIGHT_WOB = 85 * MM_TO_POINT;
        private const float CARD_WIDTH_WOB = 60 * MM_TO_POINT;
        private const float PAGE_MARGIN_V = 15 * MM_TO_POINT;
        private const float PAGE_MARGIN_H = 5.5F * MM_TO_POINT;


        private readonly HttpClient cardLoader = new();
        private readonly BackgroundWorker printWorker = new();
        public event EventHandler PrintStarted;
        public event RunWorkerCompletedEventHandler PrintFinished
        {
            add { printWorker.RunWorkerCompleted += value; }
            remove { printWorker.RunWorkerCompleted -= value; }
        }

        public BackgroundPrinter()
            => printWorker.DoWork += DoPrintWork;

        public void Print(string filename, Deck deck)
        {
            PrintStarted?.Invoke(this, EventArgs.Empty);
            printWorker.RunWorkerAsync(new PrintParams { FileName = filename, Deck = deck });
        }

        private class PrintParams
        {
            public Deck Deck { get; set; }
            public string FileName { get; set; }
        }

        private void DoPrintWork(object sender, DoWorkEventArgs args)
        {
            var pp = (PrintParams)args.Argument;
            var deck = pp.Deck;
            var po = deck.PrintOptions;

            var cs = (float)po.CardScaling / 100F;
            var cw = (po.CardBorder == CardBorder.With ? CARD_WIDTH : CARD_WIDTH_WOB) * cs;
            var ch = (po.CardBorder == CardBorder.With ? CARD_HEIGHT : CARD_HEIGHT_WOB) * cs;
            var cm = po.CardMargin * MM_TO_POINT;

            using var stream = new FileStream(pp.FileName, FileMode.Create);
            using var writer = new PdfWriter(stream);
            var doc = new Document(new PdfDocument(writer));

            int cardCount = 0;
            for (int i = 0; i < deck.Cards.Count; i++)
            {
                var currentCard = deck.Cards[i];

                if (!currentCard.CanPrint)
                    continue;

                string cardUrl;
                if (currentCard.IsChild)
                {
                    currentCard = deck.Cards[i - 1];
                    cardUrl = po.CardBorder == CardBorder.With
                        ? currentCard.SelectPrint.ChildUrls.Normal
                        : currentCard.SelectPrint.ChildUrls.BorderCrop;
                }
                else
                {
                    cardUrl = po.CardBorder == CardBorder.With
                        ? currentCard.SelectPrint.ImageUrls.Normal
                        : currentCard.SelectPrint.ImageUrls.BorderCrop;
                }

                //get image  
                var b = cardLoader.GetByteArrayAsync(cardUrl).Result;
                var img = new Image(ImageDataFactory.Create(b));
                img.ScaleToFit(cw, ch);

                for (int j = 0; j < currentCard.Count; j++)
                {
                    var x = (cardCount % 3) * (cw + cm) + PAGE_MARGIN_H;
                    var y = ((cardCount / 3) % 3) * (ch + cm) + PAGE_MARGIN_V;

                    img.SetFixedPosition((cardCount / 9) + 1, (float)x, (float)y);

                    doc.Add(img);

                    cardCount++;
                }
            }
            doc.Close();

            args.Result = po.OpenPDF ? pp.FileName : null;
        }
    }
}
