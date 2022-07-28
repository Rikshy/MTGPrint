using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.IO;
using System;

using Newtonsoft.Json;

using MTGPrint.Helper;
using MTGPrint.Models;

namespace MTGPrint
{
    public class LocalDataStorage
    {
        private const string LOCALDATA = @"data\localdata.gz";
        private const string OLDDATA = @"data\localdata.json";

        private const int LOCALDATA_VERSION = 2;

        private class UpdateArgs
        {
            public Bulk BulkInfo { get; set; }
            public bool Force { get; set; }
        }

        public LocalDataStorage()
        {
            updateWorker.WorkerReportsProgress = true;
            updateWorker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                var args = e.Argument as UpdateArgs;
                updateWorker.ReportProgress(0, $"Downloading bulkdata ({(int)(args.BulkInfo.CompressedSize / 1024) / 1024F:F3}MB)");

                var response = WebHelper.Get(args.BulkInfo.PermalinkUri, args.BulkInfo.ContentType, true);

                var cards = JsonConvert.DeserializeObject<ScryCard[]>(response);

                if (localData == null)
                    localData = new LocalDataInfo();

                int i = 0;
                foreach (var card in cards.OrderBy(sc => sc.ReleasedAt))
                {
                    if (i++ % 100 == 0 || i == cards.LongLength)
                        updateWorker.ReportProgress(0, $"Updating local cache: {i}/{cards.LongLength}");
                    if (!args.Force && card.ReleasedAt < localData.UpdatedAt)
                        continue;
                    ConvertToLocal(card);
                }

                localData.Version = LOCALDATA_VERSION;
                localData.UpdatedAt = args.BulkInfo.UpdatedAt;
                localData.CardCount = cards.LongLength;

                SaveLocalData(true);
            };

            updateWorker.ProgressChanged += (object sender, ProgressChangedEventArgs e)
                => LocalDataUpdating?.Invoke(sender, e.UserState.ToString());
            updateWorker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs args)
                => LocalDataUpdated?.Invoke(null, args);
        }

        private LocalDataInfo localData;

        public bool HasChanges { get; set; }

        public List<LocalCard> LocalCards => localData.Cards;

        private readonly BackgroundWorker updateWorker = new();
        public event EventHandler LocalDataUpdated;
        public event EventHandler<string> LocalDataUpdating;

        public bool CheckForUpdates() { return UpdateCheck(out _); }

        public void UpdateBulkData(bool force = false)
        {
            if (!UpdateCheck(out var bulkInfo) & !force)
                return;

            if (!Directory.Exists(@"data"))
                Directory.CreateDirectory("data");

            updateWorker.RunWorkerAsync(new UpdateArgs { BulkInfo = bulkInfo, Force = force });
        }

        public void SaveLocalData(bool force = false)
        {
            if (HasChanges || force)
            {
                using var file = new FileStream(LOCALDATA, FileMode.Create, FileAccess.ReadWrite);
                using var gzip = new GZipStream(file, CompressionMode.Compress);
                using var writer = new StreamWriter(gzip, Encoding.UTF8);
                writer.Write(JsonConvert.SerializeObject(localData));
            }
            HasChanges = false;
        }

        private bool UpdateCheck(out Bulk bulkInfo)
        {
            var response = WebHelper.Get("https://api.scryfall.com/bulk-data");

            bulkInfo = JsonConvert.DeserializeObject<BulkBase>(response).Data.First(b => b.Type == BulkType.DefaultCards);

            if (localData == null)
                LoadLocalData();

            return localData == null || localData.UpdatedAt < bulkInfo.UpdatedAt || localData.Version != LOCALDATA_VERSION;
        }

        private void LoadLocalData()
        {
            if (File.Exists(OLDDATA))
            {
                localData = JsonConvert.DeserializeObject<LocalDataInfo>(File.ReadAllText(OLDDATA));
                SaveLocalData(true);
                File.Delete(OLDDATA);
            }
            else if (File.Exists(LOCALDATA))
            {
                using var file = new FileStream(LOCALDATA, FileMode.Open, FileAccess.Read);
                using var gzip = new GZipStream(file, CompressionMode.Decompress);
                using var reader = new StreamReader(gzip, Encoding.UTF8);
                localData = JsonConvert.DeserializeObject<LocalDataInfo>(reader.ReadToEnd());
            }
        }

        private void ConvertToLocal(ScryCard card)
        {
            var lcard = card.Layout != CardLayout.Token
                ? localData.Cards.FirstOrDefault(c => c.OracleId == card.OracleId)
                : localData.Cards.FirstOrDefault(c => c.Name == card.Name);

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
                lcard.Prints.Insert(0, new CardPrint
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
