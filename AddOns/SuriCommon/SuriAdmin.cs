#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.NinjaScript;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class SuriAdmin : SuriVpSerialization {
        
	    public static void LoadVpIntra() {
			try {
				foreach (KeyValuePair<Commodity,CommodityData> entry in SuriStrings.data) {
					if (entry.Key != Commodity.Cotton) continue;
					Instrument instrument = GetInstrument(entry.Value);
					DateTime from = DateTime.Parse("2021-10-01");
					DateTime to = DateTime.Now.AddDays(-1).Date;
					Code.Output.Process("Loading " + instrument.MasterInstrument.Name + " from " + from + " to " + to, PrintTo.OutputTab1);
					
					new BarsRequest(instrument, from, to) {
						MergePolicy = MergePolicy.MergeBackAdjusted,
						BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Tick, Value = 1},
						TradingHours = instrument.MasterInstrument.TradingHours,
						/*IsResetOnNewTradingDay = true,
						LookupPolicy = LookupPolicies.Repository,
						IsDividendAdjusted = true,
						IsSplitAdjusted = true,*/
					}.Request((bars, errorCode, errorMessage) => {
						if (errorCode != ErrorCode.NoError) return;
						SessionIterator session = new SessionIterator(bars.Bars);
						
						SuriVpIntraData suriVpIntraData = new SuriVpIntraData();
						int prevMonth = bars.Bars.GetTime(0).Month;
						for (int i = 0; i < bars.Bars.Count; i++) {
							if (bars.Bars.IsFirstBarOfSessionByIndex(i)) {
								DateTime time = bars.Bars.GetTime(i);
								
								if (prevMonth != time.Month) {
									File.WriteAllText(dbPath + @"vpintra\" + instrument.MasterInstrument.Name + "_" + (prevMonth == 12 ? time.Year-1 : time.Year) + "_" + prevMonth + ".vpintra", JsonConvert.SerializeObject(suriVpIntraData));
									prevMonth = time.Month;
									suriVpIntraData = new SuriVpIntraData();
								}
								
								session.GetNextSession(time, true);
								DateTime closeTime = session.ActualSessionEnd;
								suriVpIntraData.barData.Add(new SuriVpBarData(instrument.MasterInstrument.TickSize, closeTime.Date));
							}
							suriVpIntraData.barData.Last().AddTick(bars.Bars.GetTime(i), bars.Bars.GetClose(i), bars.Bars.GetVolume(i), bars.Bars.GetAsk(i), bars.Bars.GetBid(i));
						}
						DateTime lastDate = bars.Bars.GetTime(bars.Bars.Count - 1);
						File.WriteAllText(dbPath + @"vpintra\" + instrument.MasterInstrument.Name + "_" + lastDate.Year + "_" + lastDate.Month + ".vpintra", JsonConvert.SerializeObject(suriVpIntraData));
						Code.Output.Process("Done", PrintTo.OutputTab1);
					});
				}
			} catch (Exception e) {
				Code.Output.Process(e.ToString(), PrintTo.OutputTab1);
			}
		}
	    
	    public static void StoreVpBigToFile(int commodityIndex = 0, bool onlyRecent = true) {
			KeyValuePair<Commodity, CommodityData> entry;
			try {
				entry = SuriStrings.data.ElementAt(commodityIndex);
			} catch (Exception) { return; }

			if (entry.Key == Commodity.BitcoinMicro) {
				StoreVpBigToFile(commodityIndex + 1, onlyRecent);
				return;
			}

			try {
				Instrument instrument = GetInstrument(entry.Value);
				DateTime from = entry.Key != Commodity.Rice ? DateTime.Parse("2000-01-01") : DateTime.Parse("2015-01-01");
				DateTime to = DateTime.Now.AddDays(-1).Date;
				Code.Output.Process("Loading " + entry.Key + " from " + from + " to " + to, PrintTo.OutputTab1);
				
				new BarsRequest(instrument, from, to) {
					MergePolicy = MergePolicy.MergeBackAdjusted,
					BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Minute, Value = 1},
					TradingHours = instrument.MasterInstrument.TradingHours,
				}.Request((bars, errorCode, errorMessage) => {
					if (errorCode != ErrorCode.NoError) {
						Code.Output.Process("Error: " + errorCode, PrintTo.OutputTab1);
						return;
					}
					
					SuriVpBigData suriVpBigData = new SuriVpBigData(instrument.MasterInstrument.TickSize);
					string path = dbPath + @"vpbig\" + instrument.MasterInstrument.Name + @"\";
					Directory.CreateDirectory(path);
					for (int i = 0; i < bars.Bars.Count; i++) {
						DateTime date = bars.Bars.GetTime(i).Date;
						if (!onlyRecent && (date - bars.Bars.GetTime(0).Date).Days > 365 * 6) {
							DateTime oldDate = bars.Bars.GetTime(i-1).Date;
							int newWeek = Week(date);
							int oldWeek = Week(oldDate);
							if (oldWeek - newWeek > 1 && newWeek == 2) {
								// this happens if a new year started and the first week of the year did not have a single trading day.
								Code.Output.Process("Fehlende erste Woche bei " + instrument.MasterInstrument.Name + " " + date, PrintTo.OutputTab1);
								using (StreamWriter stream = File.CreateText(GetVpBigFilePath(instrument, date.Year, 1))) {
									suriVpBigData.AddMissingValues();
									SuriVpBigDataSerialized suriVp = FromVpBig(suriVpBigData);
									suriVp.date = new DateTime(date.Year, 1, 1);
									stream.Write(JsonConvert.SerializeObject(suriVp));
								}
							}
							
							if (oldWeek < newWeek || oldWeek - newWeek > 1) {
								using (StreamWriter stream = File.CreateText(GetVpBigFilePath(instrument, date.Year, newWeek))) {
									suriVpBigData.AddMissingValues();
									SuriVpBigDataSerialized suriVp = FromVpBig(suriVpBigData);
									suriVp.date = date.Date;
									stream.Write(JsonConvert.SerializeObject(suriVp));
								}
							}
						}
						
						suriVpBigData.AddMinuteVolume(bars.Bars.GetVolume(i), bars.Bars.GetHigh(i), bars.Bars.GetLow(i));
					}

					DateTime now = DateTime.Now;
					if (onlyRecent) {
						using (StreamWriter stream = File.CreateText(GetVpBigFilePath(instrument))) {
							suriVpBigData.AddMissingValues();
							SuriVpBigDataSerialized suriVp = FromVpBig(suriVpBigData);
							suriVp.date = now.Date;
							stream.Write(JsonConvert.SerializeObject(suriVp));
						}
					}

					Code.Output.Process("Done " + instrument.MasterInstrument.Name, PrintTo.OutputTab1);
					StoreVpBigToFile(commodityIndex + 1, onlyRecent);
				});
				System.Threading.Thread.Sleep(1000*60);
			} catch (Exception e) {
				Code.Output.Process("Error in " + entry.Key + " " + e, PrintTo.OutputTab1);
				StoreVpBigToFile(commodityIndex + 1, onlyRecent);
			}
		}
		
    }
}
