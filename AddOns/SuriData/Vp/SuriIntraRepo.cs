#region Using declarations
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon.Vp {
    public class SuriIntraRepo {
        private static readonly Dictionary<Commodity, Mutex> state = new Dictionary<Commodity, Mutex>();
        public static readonly DateTime startOfCachedData = DateTime.Parse("2021-05-01");

        static SuriIntraRepo() {
            Directory.CreateDirectory(SuriCommon.dbPath + @"vpintra\");
            foreach (var commodity in Enum.GetValues(typeof(Commodity)).Cast<Commodity>()) {
                state.Add(commodity, new Mutex());
            }
        }
        private static string GetPath(int commId, int year, int month) { return SuriCommon.dbPath + @"vpintra\" + commId + "_" + year + "_" + month + ".vpintra"; }
        
        public static SuriVpIntraData GetVpIntra(Instrument instrument, DateTime start, DateTime end) {
            Commodity? commodity = SuriStrings.GetComm(instrument);
            if (commodity == null) return new SuriVpIntraData();
            state[commodity.Value].WaitOne();
            
            int monthsCount = 1 + (end.Month - start.Month) + (end.Year - start.Year) * 12;
            int? id = SuriStrings.GetId(instrument);
            if (id == null) return new SuriVpIntraData();
            SuriVpIntraData vpIntra = new SuriVpIntraData();
            for (int i = 0; i < monthsCount; i++) {
                int month = ((start.Month + i - 1) % 12) + 1;
                int year = start.Year + (int) Math.Floor((start.Month + i - 1) / 12.0);
                if (year < 2021 || year == 2021 && month < 5) continue;
                
                string path = GetPath(id.Value, year, month);
                SuriVpIntraData vpIntraMonth = JsonConvert.DeserializeObject<SuriVpIntraData>(File.ReadAllText(path));
                vpIntra.barData.AddRange(vpIntraMonth.barData);
            }
            vpIntra.barData.RemoveAll(bar => bar.dateTime < start || bar.dateTime > end);
            
            vpIntra.Prepare();
            state[commodity.Value].ReleaseMutex();
            return vpIntra;
        }

    }
}
