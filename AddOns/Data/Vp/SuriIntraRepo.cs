#region Using declarations
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using NinjaTrader.Core;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon.Vp {
    public class SuriIntraRepo : SuriRepo {
        private static readonly string dbPath = Globals.UserDataDir + @"db\suri\vpintra\";
        private static readonly Dictionary<Commodity, Mutex> state = new Dictionary<Commodity, Mutex>();

        static SuriIntraRepo() {
            Directory.CreateDirectory(dbPath);
            foreach (var commodity in Enum.GetValues(typeof(Commodity)).Cast<Commodity>()) {
                state.Add(commodity, new Mutex());
            }
        }
        private static string GetPath(int commId, int year, int month) { return dbPath + commId + "_" + year + "_" + month + ".vpintra"; }

        public static List<DbCotData> GetCotData(Commodity commodity, DateTime start, DateTime end) {
            state[commodity].WaitOne();
            var data = new List<DbCotData>();
            try {
                for (int year = start.Year; year <= end.Year; year++) {
                    int commId = SuriStrings.data[commodity].id;
                    string path = GetPath(commId, year, 9999999);
                    if (!File.Exists(path) || (DateTime.Now - File.GetCreationTime(path)).Days > 14 || year == DateTime.Now.Year && (DateTime.Now - File.GetCreationTime(path)).Hours > 10 ) {
                        // load cot file
                        List<DbCotData> part = SuriServer.GetCotData(commId, DateTime.Parse(year + "-01-01"), DateTime.Parse(year + "-12-31"));
                        File.WriteAllText(path, JsonSerializer.Serialize(part));
                    }
                    data.AddRange(JsonSerializer.Deserialize<List<DbCotData>>(File.ReadAllText(path)));
                }
            } finally {
                state[commodity].ReleaseMutex();
            }
            return data;
        }
        
        
        
        
        public static SuriVpIntraData GetVpIntra(Instrument instrument, DateTime start, DateTime end) {
            int monthsCount = 1 + (end.Month - start.Month) + (end.Year - start.Year) * 12;
            int month, year;
            SuriVpIntraData vpIntra = new SuriVpIntraData();
            for (int i = 0; i < monthsCount; i++) {
                month = ((start.Month + i - 1) % 12) + 1;
                year = start.Year + (int) Math.Floor((start.Month + i - 1) / 12.0);
			    
                // TODO: LOAD FROM SERVER ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! !
                // TODO: LOAD FROM SERVER ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! !
			    
                string json = File.ReadAllText(@"C:\Users\Bo\Documents\NinjaTrader 8\db\suri\vpintra\" + instrument.MasterInstrument.Name + "_" + year + "_" + month + ".vpintra");
                SuriVpIntraData vpIntraMonth = Newtonsoft.Json.JsonConvert.DeserializeObject<SuriVpIntraData>(json);
			    
                vpIntra.barData.AddRange(vpIntraMonth.barData);
            }
            vpIntra.barData.RemoveAll(bar => bar.dateTime.Date < start || bar.dateTime.Date > end);
            vpIntra.Prepare();
            return vpIntra;
        }

    }
    
    public sealed class SuriVpIntraSingleSerialized {
        public List<SuriVpIntraTickSerialized> tickData;
        public DateTime date;
        public int high;
        public int low;
        public double totalVolume;
        public double tickSize;
        public int totalAsks;
        public int totalBids;
    }
    
    public sealed class SuriVpIntraTickSerialized {
        public long tick;
        public long bid;
        public long ask;
    }
}
