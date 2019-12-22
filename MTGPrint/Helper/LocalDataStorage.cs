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

        private const int LOCALDATA_VERSION = 2;

        public LocalDataStorage()
        {
            updateWorker.WorkerReportsProgress = true;
            updateWorker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                var bulkInfo = e.Argument as Bulk;
                updateWorker.ReportProgress(0, $"Downloading bulkdata ({((int)(bulkInfo.CompressedSize / 1024) / 1024F).ToString("F3")}MB)");

                var request = (HttpWebRequest)WebRequest.Create(bulkInfo.PermalinkUri);
                request.Method = WebRequestMethods.Http.Get;
                request.ContentType = bulkInfo.ContentType;
                request.AutomaticDecompression = DecompressionMethods.GZip;
                var response = (HttpWebResponse)request.GetResponse();
                using var stream = new StreamReader(response.GetResponseStream());
                var responseText = stream.ReadToEnd();

                var cards = JsonConvert.DeserializeObject<ScryCard[]>( responseText );

                if (localData == null)
                    localData = new LocalDataInfo();

                localData.UpdatedAt = bulkInfo.UpdatedAt;
                localData.CardCount = cards.LongLength;

                int i = 0;
                foreach (var card in cards)
                {
                    if (i++ % 100 == 0 || i == cards.LongLength)
                        updateWorker.ReportProgress(0, $"Updating local cache: {i}/{cards.LongLength}");
                    ConvertToLocal(card);
                }

                localData.Version = LOCALDATA_VERSION;

                HasChanges = true;
                SaveLocalData();
            };
            updateWorker.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
            {
                LocalDataUpdating?.Invoke(sender, e.UserState.ToString());
            };
            updateWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
            {
                LocalDataUpdated?.Invoke(null, args);
            };
        }

        private readonly ScryfallClient scry = new ScryfallClient();

        private LocalDataInfo localData;

        public bool HasChanges { get; set; }

        public List<LocalCard> LocalCards => localData.Cards;

        private readonly BackgroundWorker updateWorker = new BackgroundWorker();
        public event EventHandler LocalDataUpdated;
        public event EventHandler<string> LocalDataUpdating;

        public bool CheckForUpdates() { return UpdateCheck(out _); }

        public void UpdateBulkData()
        {
            if (!UpdateCheck(out var bulkInfo))
                return;

            if (!Directory.Exists(@"data"))
                Directory.CreateDirectory("data");

            updateWorker.RunWorkerAsync(bulkInfo);
        }

        public void SaveLocalData()
        {
            if (HasChanges)
                File.WriteAllText(LOCALDATA, JsonConvert.SerializeObject(localData, Formatting.Indented));
            HasChanges = false;
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
            if (!File.Exists(LOCALDATA))
                return;

            localData = JsonConvert.DeserializeObject<LocalDataInfo>(File.ReadAllText(LOCALDATA));
        }

        private void ConvertToLocal(ScryCard card)
        {
            var lcard = card.Layout != CardLayout.Token
                ? localData.Cards.FirstOrDefault( c => c.OracleId == card.OracleId )
                : localData.Cards.FirstOrDefault( c => c.Name == card.Name );
            if (lcard == null)
            {
                lcard = new LocalCard
                {
                    OracleId = card.OracleId,
                    Name = card.Name,
                    ScryUrl = card.ScryUrl,
                    LatestPrint = card.ReleasedAt,
                    Parts = card.Parts
                };
                localData.Cards.Add(lcard);
            }
            else
            {
                if (card.ReleasedAt > lcard.LatestPrint)
                    lcard.LatestPrint = card.ReleasedAt;

                if (card.Parts != null)
                {
                    if (lcard.Parts == null)
                        lcard.Parts = new List<CardParts>();

                    lcard.Parts.AddRange(card.Parts.Where(part => lcard.Parts.All(p => part.Id != p.Id)));
                }
            }

            var iu = card.ImageUrls;
            ImageUrls child = null;
            if (card.Layout == CardLayout.Transform)
            {
                iu = card.CardFaces.First().ImageUrls;
                child = card.CardFaces.Last().ImageUrls;
            }

            if (lcard.Prints.All(p => p.Id != card.Id))
                lcard.Prints.Add(new CardPrint
                {
                    Id = card.Id,
                    Set = card.Set,
                    SetName = card.SetName,
                    ImageUrls = iu,
                    ChildUrls = child
                });
        }
    }
}
