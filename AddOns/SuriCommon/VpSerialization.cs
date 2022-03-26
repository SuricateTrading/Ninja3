#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.NinjaScript;
using System.IO;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class VpSerialization {
	    public static readonly string dbPath = Globals.UserDataDir + @"db\suri\";

	    private static Instrument GetInstrument(CommodityData commodity) {
		    return Instrument.All.Where(x => x.MasterInstrument.Name == commodity.shortName && x.MasterInstrument.InstrumentType == InstrumentType.Future && x.Expiry.Date > DateTime.Now)
			    .OrderBy(o => o.Expiry.Date).First();
	    }

	    public static void LoadVp() {
		    LoadVpBig();
	    }
	    
		public static void LoadVpIntra() {
			try {
				foreach (KeyValuePair<Commodity,CommodityData> entry in SuriStrings.data) {
					if (entry.Key != Commodity.BrazilianReal) continue;
					Instrument instrument = GetInstrument(entry.Value);
					DateTime from = DateTime.Parse("2017-07-30");
					DateTime to = DateTime.Now.AddDays(-1).Date;
					Code.Output.Process("Loading from " + from + " to " + to, PrintTo.OutputTab1);
					
					new BarsRequest(instrument, from, to) {
						MergePolicy = MergePolicy.MergeBackAdjusted,
						BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Tick, Value = 1},
						TradingHours = instrument.MasterInstrument.TradingHours,
					}.Request((bars, errorCode, errorMessage) => {
						if (errorCode != ErrorCode.NoError) return;
						StreamWriter stream = File.AppendText(dbPath + instrument.MasterInstrument.Name + ".vpintra");

						VpIntraData vpIntraData = new VpIntraData();
						int lastDayOfYear = -1;
						for (int i = 0; i < bars.Bars.Count; i++) {
							DateTime time = bars.Bars.GetTime(i);
							if (lastDayOfYear != time.DayOfYear) {
								if (i != 0) vpIntraData.barData.Last().Prepare();
								vpIntraData.barData.Add(new VpBarData(instrument.MasterInstrument.TickSize, time.Date));
							}
							lastDayOfYear  = time.DayOfYear;
							vpIntraData.barData.Last().AddTick(bars.Bars.GetClose(i), bars.Bars.GetVolume(i), bars.Bars.GetAsk(i), bars.Bars.GetBid(i));
						}
						vpIntraData.barData.Last().Prepare();
						stream.Write("\n" + Newtonsoft.Json.JsonConvert.SerializeObject(vpIntraData));
						stream.Close();
						Code.Output.Process("Done", PrintTo.OutputTab1);
					});
				}
			} catch (Exception e) {
				Code.Output.Process(e.ToString(), PrintTo.OutputTab1);
			}
		}
		
		public static void LoadVpBig() {
			try {
				foreach (KeyValuePair<Commodity,CommodityData> entry in SuriStrings.data) {
					if (entry.Key != Commodity.BrazilianReal) continue;
					Instrument instrument = GetInstrument(entry.Value);
					DateTime from = DateTime.Parse("2000-01-01");
					DateTime to = DateTime.Now.AddDays(-1).Date;
					Code.Output.Process("Loading from " + from + " to " + to, PrintTo.OutputTab1);
					
					new BarsRequest(instrument, from, to) {
						MergePolicy = MergePolicy.MergeBackAdjusted,
						BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Minute, Value = 1},
						TradingHours = instrument.MasterInstrument.TradingHours,
					}.Request((bars, errorCode, errorMessage) => {
						if (errorCode != ErrorCode.NoError) {
							Code.Output.Process("Error: " + errorCode, PrintTo.OutputTab1);
							return;
						}
						
						VpBigData vpBigData = new VpBigData(instrument.MasterInstrument.TickSize);
						string path = dbPath + @"vpbig\" + instrument.MasterInstrument.Name + @"\";
						for (int i = 0; i < bars.Bars.Count; i++) {
							DateTime date = bars.Bars.GetTime(i).Date;
							if ((date - bars.Bars.GetTime(0).Date).Days > 365 * 6) {
								if (IsNewMonth(bars.Bars.GetTime(i-1), date)) {
									string json = Newtonsoft.Json.JsonConvert.SerializeObject(vpBigData);
									VpBigData preparedVpBigData = Newtonsoft.Json.JsonConvert.DeserializeObject<VpBigData>(json);
									preparedVpBigData.Prepare();
									Directory.CreateDirectory(path);
									using (StreamWriter stream = File.CreateText(GetVpBigFilePath(instrument, date))) {
										stream.Write("\n" + Newtonsoft.Json.JsonConvert.SerializeObject(preparedVpBigData));
									}
								}
							}
							if (date.Year == DateTime.Now.Year && date.Month == DateTime.Now.Month) break;
							vpBigData.AddMinuteVolume(bars.Bars.GetVolume(i), bars.Bars.GetHigh(i), bars.Bars.GetLow(i));
						}
						
						Code.Output.Process("Done", PrintTo.OutputTab1);
					});
				}
			} catch (Exception e) {
				Code.Output.Process(e.ToString(), PrintTo.OutputTab1);
			}
		}
		
		public static void LoadVpBigNotPrepared() {
			try {
				foreach (KeyValuePair<Commodity,CommodityData> entry in SuriStrings.data) {
					if (entry.Key != Commodity.BrazilianReal) continue;
					Instrument instrument = GetInstrument(entry.Value);
					DateTime from = DateTime.Parse("2000-01-01");
					DateTime to = DateTime.Now.AddDays(-1).Date;
					Code.Output.Process("Loading from " + from + " to " + to, PrintTo.OutputTab1);
					
					new BarsRequest(instrument, from, to) {
						MergePolicy = MergePolicy.MergeBackAdjusted,
						BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Minute, Value = 1},
						TradingHours = instrument.MasterInstrument.TradingHours,
					}.Request((bars, errorCode, errorMessage) => {
						if (errorCode != ErrorCode.NoError) {
							Code.Output.Process("Error: " + errorCode, PrintTo.OutputTab1);
							return;
						}
						
						VpBigData vpBigData = new VpBigData(instrument.MasterInstrument.TickSize);
						string path = dbPath + @"vpbig\" + instrument.MasterInstrument.Name + @"\";
						for (int i = 0; i < bars.Bars.Count; i++) {
							DateTime date = bars.Bars.GetTime(i).Date;
							if ((date - bars.Bars.GetTime(0).Date).Days > 365 * 6) {
								if (IsNewMonth(bars.Bars.GetTime(i-1), date)) {
									string json = Newtonsoft.Json.JsonConvert.SerializeObject(vpBigData);
									VpBigData preparedVpBigData = Newtonsoft.Json.JsonConvert.DeserializeObject<VpBigData>(json);
									preparedVpBigData.Prepare();
									Directory.CreateDirectory(path);
									using (StreamWriter stream = File.CreateText(GetVpBigFilePath(instrument, date))) {
										stream.Write("\n" + Newtonsoft.Json.JsonConvert.SerializeObject(preparedVpBigData));
									}
								}
							}
							if (date.Year == DateTime.Now.Year && date.Month == DateTime.Now.Month) break;
							vpBigData.AddMinuteVolume(bars.Bars.GetVolume(i), bars.Bars.GetHigh(i), bars.Bars.GetLow(i));
						}
						
						Code.Output.Process("Done", PrintTo.OutputTab1);
					});
				}
			} catch (Exception e) {
				Code.Output.Process(e.ToString(), PrintTo.OutputTab1);
			}
		}

		public static string GetVpBigFilePath(Instrument instrument, DateTime date) {
			return dbPath + @"vpbig\" + instrument.MasterInstrument.Name + @"\"  + date.Year + "_" + date.Month + ".vpbig";
		}

		public static bool IsNewMonth(DateTime oldDate, DateTime newDate) {
			if (oldDate.Month == 12 && newDate.Month == 1) return true;
			return oldDate.Month < newDate.Month;
		}
    }
}