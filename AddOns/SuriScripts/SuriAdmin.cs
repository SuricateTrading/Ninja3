#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.NinjaScript;
using System.IO;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using NinjaTrader.Data;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class SuriAdmin {
        
	    public static void StoreVpIntra(int commodityIndex = 23) {
		    KeyValuePair<Commodity, CommodityData> entry;
		    try {
			    entry = SuriStrings.data.ElementAt(commodityIndex);
		    } catch (Exception) {
			    Code.Output.Process("Could not find commodityIndex " + commodityIndex, PrintTo.OutputTab1);
			    return;
		    }
			try {
				Instrument instrument = SuriRepo.GetInstrument(entry.Value);
				DateTime from = DateTime.Parse("2018-01-01").Date;
				DateTime to = DateTime.Now.AddDays(-1).Date;
				Code.Output.Process("Loading " + instrument.MasterInstrument.Name + " from " + from + " to " + to, PrintTo.OutputTab1);
				
				new BarsRequest(instrument, from, to) {
					MergePolicy = MergePolicy.MergeBackAdjusted,
					BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Tick, Value = 1},
					TradingHours = instrument.MasterInstrument.TradingHours,
				}.Request((bars, errorCode, errorMessage) => {
					if (errorCode != ErrorCode.NoError) return;
					SessionIterator session = new SessionIterator(bars.Bars);
					
					SuriVpIntraData suriVpIntraData = new SuriVpIntraData();
					int prevMonth = bars.Bars.GetTime(0).Month;
					for (int i = 0; i < bars.Bars.Count; i++) {
						if (bars.Bars.IsFirstBarOfSessionByIndex(i)) {
							DateTime time = bars.Bars.GetTime(i);
							
							if (prevMonth != time.Month) {
								File.WriteAllText(
									SuriRepo.dbPath + @"vpintra\" + entry.Value.id + "_" + (prevMonth == 12 ? time.Year-1 : time.Year) + "_" + prevMonth + ".vpintra",
									Newtonsoft.Json.JsonConvert.SerializeObject(suriVpIntraData)
								);
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
					File.WriteAllText(
						SuriRepo.dbPath + @"vpintra\" + entry.Value.id + "_" + lastDate.Year + "_" + lastDate.Month + ".vpintra",
						Newtonsoft.Json.JsonConvert.SerializeObject(suriVpIntraData)
					);
					Code.Output.Process("Done", PrintTo.OutputTab1);
					StoreVpIntra(commodityIndex + 1);
				});
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
				Instrument instrument = SuriRepo.GetInstrument(entry.Value);
				DateTime from = entry.Key != Commodity.Rice ? DateTime.Parse("2000-01-01") : DateTime.Parse("2012-01-01");
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
					string path = SuriRepo.dbPath + @"vpbig\" + instrument.MasterInstrument.Name + @"\";
					Directory.CreateDirectory(path);
					for (int i = 0; i < bars.Bars.Count; i++) {
						DateTime date = bars.Bars.GetTime(i).Date;
						if (!onlyRecent && (date - bars.Bars.GetTime(0).Date).Days > 365 * 6) {
							DateTime oldDate = bars.Bars.GetTime(i-1).Date;
							int newWeek = SuriRepo.Week(date);
							int oldWeek = SuriRepo.Week(oldDate);
							if (oldWeek - newWeek > 1 && newWeek == 2) {
								// this happens if a new year started and the first week of the year did not have a single trading day.
								Code.Output.Process("Fehlende erste Woche bei " + instrument.MasterInstrument.Name + " " + date, PrintTo.OutputTab1);
								using (StreamWriter stream = File.CreateText(SuriBigRepo.GetVpBigFilePath(instrument, date.Year, 1))) {
									suriVpBigData.AddMissingValues();
									SuriVpBigDataSerialized suriVp = SuriBigRepo.FromVpBig(suriVpBigData);
									suriVp.date = new DateTime(date.Year, 1, 1);
									stream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(suriVp));
								}
							}
							
							if (oldWeek < newWeek || oldWeek - newWeek > 1) {
								using (StreamWriter stream = File.CreateText(SuriBigRepo.GetVpBigFilePath(instrument, date.Year, newWeek))) {
									suriVpBigData.AddMissingValues();
									SuriVpBigDataSerialized suriVp = SuriBigRepo.FromVpBig(suriVpBigData);
									suriVp.date = date.Date;
									stream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(suriVp));
								}
							}
						}
						
						suriVpBigData.AddMinuteVolume(bars.Bars.GetVolume(i), bars.Bars.GetHigh(i), bars.Bars.GetLow(i));
					}

					DateTime now = DateTime.Now;
					if (onlyRecent) {
						using (StreamWriter stream = File.CreateText(SuriBigRepo.GetVpBigFilePath(instrument))) {
							suriVpBigData.AddMissingValues();
							SuriVpBigDataSerialized suriVp = SuriBigRepo.FromVpBig(suriVpBigData);
							suriVp.date = now.Date;
							stream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(suriVp));
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

	    public static void StoreTickData(int commodityIndex = 0) {
		    //if (commodityIndex >= 5) return;
		    KeyValuePair<Commodity, CommodityData> entry;
		    try {
			    entry = SuriStrings.data.ElementAt(commodityIndex);
		    } catch (Exception) {
			    Code.Output.Process("Could not find commodityIndex " + commodityIndex, PrintTo.OutputTab1);
			    return;
		    }
		    /*if (entry.Key != Commodity.Soybeans) {
			    StoreTickData(commodityIndex + 1);
			    return;
		    }*/
			try {
				Instrument instrument = SuriRepo.GetInstrument(entry.Value);
				DateTime from = DateTime.Parse("2015-01-01").Date;
				DateTime to = DateTime.Now.AddDays(-1).Date;
				Code.Output.Process("Loading " + instrument.MasterInstrument.Name + " from " + from + " to " + to, PrintTo.OutputTab1);
				
				new BarsRequest(instrument, from, to) {
					MergePolicy = MergePolicy.MergeBackAdjusted,
					BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Tick, Value = 1},
					TradingHours = instrument.MasterInstrument.TradingHours,
				}.Request((bars, errorCode, errorMessage) => {
					if (errorCode != ErrorCode.NoError) return;
					StreamWriter stream = File.CreateText(SuriRepo.dbPath + @"ticks\" + instrument.MasterInstrument.Name + "_" + bars.Bars.GetTime(0).Year + "_" + bars.Bars.GetTime(0).Month + ".tickdata");
					for (int i = 0; i < bars.Bars.Count; i++) {
						if (i > 0 && bars.Bars.GetTime(i).Month != bars.Bars.GetTime(i - 1).Month) {
							stream.Close();
							stream = File.CreateText(SuriRepo.dbPath + @"ticks\" + instrument.MasterInstrument.Name + "_" + bars.Bars.GetTime(i).Year + "_" + bars.Bars.GetTime(i).Month + ".tickdata");
						}
						if (Math.Abs(3.0 * bars.Bars.GetClose(i) - bars.Bars.GetLow(i) - bars.Bars.GetHigh(i) - bars.Bars.GetOpen(i)) > 0.0000000001) {
							Code.Output.Process("ERROR", PrintTo.OutputTab1);
						}
						stream.Write(bars.Bars.GetTime(i) + "\t" + bars.Bars.GetAsk(i) + "\t" + bars.Bars.GetBid(i) + "\t" + bars.Bars.GetOpen(i) + "\t" + bars.Bars.GetVolume(i) + "\n");
					}
					stream.Close();
					Code.Output.Process("Done", PrintTo.OutputTab1);
					StoreTickData(commodityIndex + 1);
				});
			} catch (Exception e) {
				Code.Output.Process(e.ToString(), PrintTo.OutputTab1);
			}
	    }

	    private class SuriStoredTickData {
		    public DateTime time;
		    public double ask;
		    public double bid;
		    public double price;
		    public long volume;
		    public static SuriStoredTickData FromCsvLine(string csvLine) {
			    string[] values = csvLine.Split('\t');
			    SuriStoredTickData dailyValues = new SuriStoredTickData() {
				    time = DateTime.Parse(values[0]),
				    ask = Convert.ToDouble(values[1]),
				    bid = Convert.ToDouble(values[2]),
				    price = Convert.ToDouble(values[3]),
				    volume = Convert.ToInt64(values[4]),
			    };
			    return dailyValues;
		    }
		    public static List<SuriStoredTickData> FromCsv(string path) {
			    return File.ReadAllLines(path).Skip(1).Select(FromCsvLine).ToList();
		    }
	    }
	    
    }
}
