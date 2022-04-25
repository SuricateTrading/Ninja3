using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NinjaTrader.Core;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public static class SuriCotRepo {
        private static readonly string dbPath = Globals.UserDataDir + @"db\suri\cot\";
        private static readonly Dictionary<Commodity, Mutex> commState = new Dictionary<Commodity, Mutex>();

        static SuriCotRepo() {
            Directory.CreateDirectory(dbPath);
            foreach (var commodity in Enum.GetValues(typeof(Commodity)).Cast<Commodity>()) {
                commState.Add(commodity, new Mutex());
            }
        }
        private static string GetPath(int commId, int year) { return dbPath + commId + "_" + year + ".cot"; }

        public static List<DbCotData> GetCotData(Commodity commodity, DateTime start, DateTime end) {
            commState[commodity].WaitOne();
            var data = new List<DbCotData>();
            try {
                for (int year = start.Year; year <= end.Year; year++) {
                    int commId = SuriStrings.data[commodity].id;
                    string path = GetPath(commId, year);
                    TimeSpan fileAge = DateTime.Now - File.GetCreationTime(path);
                    if (!File.Exists(path) || fileAge.TotalDays >= 10 || year == DateTime.Now.Year && fileAge.TotalHours >= 8 ) {
                        // load cot file
                        List<DbCotData> part = SuriServer.GetCotData(commId, DateTime.Parse(year + "-01-01"), DateTime.Parse(year + "-12-31"));
                        File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(part));
                    }
                    data.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<DbCotData>>(File.ReadAllText(path)));
                }
            } finally {
                commState[commodity].ReleaseMutex();
            }
            return data;
        }
    }
}
