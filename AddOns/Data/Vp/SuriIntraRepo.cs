#region Using declarations
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon.Vp {
    public class SuriIntraRepo : SuriRepo {
        private static readonly Dictionary<Commodity, Mutex> state = new Dictionary<Commodity, Mutex>();
        public static readonly DateTime startOfCachedData = DateTime.Parse("2021-05-01");

        static SuriIntraRepo() {
            Directory.CreateDirectory(dbPath + @"vpintra\");
            foreach (var commodity in Enum.GetValues(typeof(Commodity)).Cast<Commodity>()) {
                state.Add(commodity, new Mutex());
            }
        }
        private static string GetPath(int commId, int year, int month) { return dbPath + @"vpintra\" + commId + "_" + year + "_" + month + ".vpintra"; }
        
        
        public static void GetVpIntra(Instrument instrument, DateTime start, DateTime end, Action<SuriVpIntraData> result) {
            /*
            state[commodity].WaitOne();
            state[commodity].ReleaseMutex();
             */
            Mutex mutex = new Mutex();
            mutex.WaitOne();
            int monthsCount = 1 + (end.Month - start.Month) + (end.Year - start.Year) * 12;
            int month, year;
            int? id = SuriStrings.GetId(instrument);
            if (id == null) return;
            SuriVpIntraData vpIntra = new SuriVpIntraData();
            for (int i = 0; i < monthsCount; i++) {
                month = ((start.Month + i - 1) % 12) + 1;
                year = start.Year + (int) Math.Floor((start.Month + i - 1) / 12.0);
                if (year < 2021 || year == 2021 && month < 5) continue;
                
                string path = GetPath(id.Value, year, month);
                if (!File.Exists(path) ||
                     (DateTime.Now - File.GetCreationTime(path)).Days > 14
                     || year == DateTime.Now.Year && month == DateTime.Now.Month && (DateTime.Now - File.GetCreationTime(path)).TotalDays >= 5
                ) {
                    // load vp file
                    string serverFile = @"https://app.suricate-trading.de/ninja/vpintra/" + id + "_" + year + "_" + month + ".vpintra";
                    try {
                        using (WebClient webClient = new WebClient()) {
                            webClient.DownloadFile(serverFile, path);
                        }
                    } catch (Exception) {
                        return;
                    }
                }
                SuriVpIntraData vpIntraMonth = Newtonsoft.Json.JsonConvert.DeserializeObject<SuriVpIntraData>(File.ReadAllText(path));
                vpIntra.barData.AddRange(vpIntraMonth.barData);
            }
            vpIntra.barData.RemoveAll(bar => bar.dateTime.Date < start || bar.dateTime.Date > end);
            
            if (vpIntra.barData.Last().dateTime.Date < end.Date) {
                // the cached vp data does not contain everything. We have to attach current data.
                Code.Output.Process("From " + vpIntra.barData.Last().dateTime.AddDays(1).Date + " to " + end.AddDays(1).Date, PrintTo.OutputTab1);
                new BarsRequest(instrument, vpIntra.barData.Last().dateTime.AddDays(1).Date, end.AddDays(1).Date) {
                    MergePolicy = MergePolicy.MergeBackAdjusted,
                    BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Tick, Value = 1},
                    TradingHours = instrument.MasterInstrument.TradingHours,
                }.Request((bars, errorCode, errorMessage) => {
                    if (errorCode != ErrorCode.NoError) return;
                    SessionIterator session = new SessionIterator(bars.Bars);
                    for (int i = 0; i < bars.Bars.Count; i++) {
                        if (bars.Bars.IsFirstBarOfSessionByIndex(i)) {
                            DateTime time = bars.Bars.GetTime(i);
                            session.GetNextSession(time, true);
                            DateTime closeTime = session.ActualSessionEnd;
                            vpIntra.barData.Add(new SuriVpBarData(instrument.MasterInstrument.TickSize, closeTime.Date));
                        }
                        vpIntra.barData.Last().AddTick(bars.Bars.GetTime(i), bars.Bars.GetClose(i), bars.Bars.GetVolume(i), bars.Bars.GetAsk(i), bars.Bars.GetBid(i));
                    }
                    vpIntra.Prepare();
                    result(vpIntra);
                });
            } else {
                vpIntra.Prepare();
                result(vpIntra);
            }
        }

    }
}
