using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NinjaTrader.Core;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public static class SuriCotRepo {
        private static readonly string dbPath = Globals.UserDataDir + @"db\suri\cot\";
        private static readonly Dictionary<Commodity, Mutex> commState = new Dictionary<Commodity, Mutex>();

        static SuriCotRepo() {
            Directory.CreateDirectory(dbPath);
            foreach (Commodity c in Enum.GetValues(typeof(Commodity)).Cast<Commodity>()) {
                commState.Add(c, new Mutex());
            }
        }
        private static string GetPath(int commId, int year) { return dbPath + commId + "_" + year + ".cot"; }

        public static async Task<List<DbCotData>> GetCotData(Commodity commodity, DateTime start, DateTime end) {
            commState[commodity].WaitOne();
            List<DbCotData> data = new List<DbCotData>();
            try {
                for (int year = start.Year; year <= end.Year; year++) {
                    int commId = SuriStrings.data[commodity].id;
                    string path = GetPath(commId, year);
                    if (!File.Exists(path) || year == DateTime.Now.Year && (DateTime.Now - File.GetCreationTime(path)).Hours > 10 ) {
                        List<DbCotData> part = SuriServer.GetCotData(commId, DateTime.Parse(year + "-01-01"), DateTime.Parse(year + "-12-31"));
                        File.WriteAllText(path, JsonSerializer.Serialize(part));
                    }
                    data.AddRange(JsonSerializer.Deserialize<List<DbCotData>>(File.ReadAllText(path)));
                }
            } finally {
                commState[commodity].ReleaseMutex();
            }
            return data;
        }
    }
}
