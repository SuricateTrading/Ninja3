#region Using declarations
using System;
using System.Collections.Generic;
using System.Globalization;
using NinjaTrader.NinjaScript;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Data;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class SuriVpSerialization {
	    public static readonly string dbPath = Globals.UserDataDir + @"db\suri\";

	    private static Instrument GetInstrument(CommodityData commodity) {
		    return Instrument.GetInstrument(commodity.shortName + Instrument.GetInstrument(commodity.shortName+" ##-##").MasterInstrument.GetNextExpiry(DateTime.Now).ToString(" MM-yy"));
	    }

	    public static void LoadVp() {
		    //StoreVpBigToFile();
	    }
	    
	    /*private static void LoadVpIntra() {
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

						SuriVpIntraData suriVpIntraData = new SuriVpIntraData();
						int lastDayOfYear = -1;
						for (int i = 0; i < bars.Bars.Count; i++) {
							DateTime time = bars.Bars.GetTime(i);
							if (lastDayOfYear != time.DayOfYear) {
								if (i != 0) suriVpIntraData.barData.Last().Prepare();
								suriVpIntraData.barData.Add(new SuriVpBarData(instrument.MasterInstrument.TickSize, time.Date));
							}
							lastDayOfYear  = time.DayOfYear;
							suriVpIntraData.barData.Last().AddTick(bars.Bars.GetClose(i), bars.Bars.GetVolume(i), bars.Bars.GetAsk(i), bars.Bars.GetBid(i));
						}
						suriVpIntraData.barData.Last().Prepare();
						stream.Write("\n" + Newtonsoft.Json.JsonConvert.SerializeObject(suriVpIntraData));
						stream.Close();
						Code.Output.Process("Done", PrintTo.OutputTab1);
					});
				}
			} catch (Exception e) {
				Code.Output.Process(e.ToString(), PrintTo.OutputTab1);
			}
		}
		
		
		private static void StoreVpBigToFile(int commodityIndex = 0, bool onlyRecent = true) {
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
									stream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(suriVp));
								}
							}
							
							if (oldWeek < newWeek || oldWeek - newWeek > 1) {
								using (StreamWriter stream = File.CreateText(GetVpBigFilePath(instrument, date.Year, newWeek))) {
									suriVpBigData.AddMissingValues();
									SuriVpBigDataSerialized suriVp = FromVpBig(suriVpBigData);
									suriVp.date = date.Date;
									stream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(suriVp));
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
		*/

		public static SuriVpBigData GetVpBig(Instrument instrument, DateTime? date = null) {
			int? year = null, week = null;
			if (date != null) {
				year = date.Value.Year;
				week = Week(date.Value);
			}
			
			string fileName = GetVpBigFilePath(instrument, year, week);
			bool updateVpFile = true;
			if (File.Exists(fileName)) {
				DateTime fileCreation = File.GetCreationTime(fileName);
				int fileWeek = Week(fileCreation);
				int currentWeek = Week(DateTime.Now);
				if (currentWeek == fileWeek && DateTime.Now.Year == fileCreation.Year && (DateTime.Now - fileCreation).TotalDays <= 3) {
					updateVpFile = false;
				}
			}
			if (updateVpFile) {
				string serverFile = @"https://app.suricate-trading.de/ninja/vpbig/" + instrument.MasterInstrument.Name + ".vpbig";
				try {
					using (WebClient webClient = new WebClient()) {
						webClient.DownloadFile(serverFile, fileName);
					}
				} catch (Exception) {
					return null;
				}
			}
			string json = File.ReadAllText(fileName);
			var serializer = new JavaScriptSerializer();
			SuriVpBigDataSerialized s = serializer.Deserialize<SuriVpBigDataSerialized>(json);
			
			SuriVpBigData suriVp = ToVpBig(s);
			suriVp.Prepare();
			return suriVp;
		}

		private static string GetVpBigFilePath(Instrument instrument, int? year = null, int? week = null) {
			if (year == null) {
				string folder = dbPath + @"vpbig\main\";
				Directory.CreateDirectory(folder);
				return folder + instrument.MasterInstrument.Name + ".vpbig";
			} else {
				string folder = dbPath + @"vpbig\" + instrument.MasterInstrument.Name + @"\";
				Directory.CreateDirectory(folder);
				return folder + year + "_" + week + ".vpbig";
			}
		}

		private static int Week(DateTime time) {
			return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
		}
		
		private static SuriVpBigData ToVpBig(SuriVpBigDataSerialized suriVpBigDataSerialized) {
			SuriVpBigData suriVpBigData = new SuriVpBigData(suriVpBigDataSerialized.tickSize) {
				low = suriVpBigDataSerialized.low,
				high = suriVpBigDataSerialized.high,
				totalVolume = suriVpBigDataSerialized.totalVolume
			};
			int tick = suriVpBigData.low;
			foreach (var volume in suriVpBigDataSerialized.tickData) {
				suriVpBigData.tickData[tick] = new SuriVpTickData(tick);
				suriVpBigData.tickData[tick].volume = volume;
				tick++;
			}
			return suriVpBigData;
		}
		private static SuriVpBigDataSerialized FromVpBig(SuriVpBigData suriVpBigData) {
			SuriVpBigDataSerialized suriVpBigDataSerialized = new SuriVpBigDataSerialized {
				low = suriVpBigData.low,
				high = suriVpBigData.high,
				totalVolume = suriVpBigData.totalVolume,
				tickSize = suriVpBigData.tickSize,
				tickData = new List<long>()
			};
			foreach (var pair in suriVpBigData.tickData) {
				suriVpBigDataSerialized.tickData.Add((long) Math.Round(pair.Value.volume));
			}
			return suriVpBigDataSerialized;
		}
		
    }
    
    public sealed class SuriVpBigDataSerialized {
	    public List<long> tickData;
	    public int high;
	    public int low;
	    public double totalVolume;
	    public double tickSize;
	    public DateTime date;
    }
    
}
