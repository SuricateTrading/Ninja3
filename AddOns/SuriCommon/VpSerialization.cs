#region Using declarations
using System;
using System.Collections.Generic;
using System.Globalization;
using NinjaTrader.NinjaScript;
using System.IO;
using System.Linq;
using System.Net;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Data;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class VpSerialization {
	    public static readonly string dbPath = Globals.UserDataDir + @"db\suri\";

	    private static Instrument GetInstrument(CommodityData commodity) {
		    return Instrument.All.Where(x => x.MasterInstrument.Name == commodity.shortName && x.MasterInstrument.InstrumentType == InstrumentType.Future && x.Expiry.Date > DateTime.Now)
			    .OrderBy(o => o.Expiry.Date).First();
	    }

	    public static void LoadVp() {
		    StoreVpBigToFile();
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
		
		
		
		
		public static void StoreVpBigToFile(int commodityIndex = 0, bool onlyRecent = true) {
			KeyValuePair<Commodity, CommodityData> entry;
			try {
				entry = SuriStrings.data.ElementAt(commodityIndex);
			} catch (Exception) { return; }

			if (entry.Key != Commodity.BitcoinMicro) {
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
					
					VpBigData vpBigData = new VpBigData(instrument.MasterInstrument.TickSize);
					string path = dbPath + @"vpbig\" + instrument.MasterInstrument.Name + @"\";
					Directory.CreateDirectory(path);
					for (int i = 0; i < bars.Bars.Count; i++) {
						DateTime date = bars.Bars.GetTime(i).Date;
						if (!onlyRecent && (date - bars.Bars.GetTime(0).Date).Days > 365 * 6) {
							// this is only executed iff at least 6 years have been loaded
							
							DateTime oldDate = bars.Bars.GetTime(i-1).Date;
							int newWeek = Week(date);
							int oldWeek = Week(oldDate);
							if (oldWeek - newWeek > 1 && newWeek == 2) {
								// this happens if a new year started and the first week of the year did not have a single trading day.
								Code.Output.Process("Fehlende erste Woche bei " + instrument.MasterInstrument.Name + " " + date, PrintTo.OutputTab1);
								using (StreamWriter stream = File.CreateText(GetVpBigFilePath(instrument, date.Year, 1))) {
									VpBigDataSerialized vp = FromVpBig(vpBigData);
									vp.date = new DateTime(date.Year, 1, 1);
									stream.Write("\n" + Newtonsoft.Json.JsonConvert.SerializeObject(vp));
								}
							}
							
							if (oldWeek < newWeek || oldWeek - newWeek > 1) {
								using (StreamWriter stream = File.CreateText(GetVpBigFilePath(instrument, date.Year, newWeek))) {
									VpBigDataSerialized vp = FromVpBig(vpBigData);
									vp.date = date.Date;
									stream.Write("\n" + Newtonsoft.Json.JsonConvert.SerializeObject(vp));
								}
							}
						}
						vpBigData.AddMinuteVolume(bars.Bars.GetVolume(i), bars.Bars.GetHigh(i), bars.Bars.GetLow(i));
					}

					DateTime now = DateTime.Now;
					if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday) {
						using (StreamWriter stream = File.CreateText(GetVpBigFilePath(instrument, onlyRecent ? (int?) null : now.Year, onlyRecent ? (int?) null : Week(now)+1))) {
							VpBigDataSerialized vp = FromVpBig(vpBigData);
							vp.date = now.Date;
							stream.Write("\n" + Newtonsoft.Json.JsonConvert.SerializeObject(vp));
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
		

		public static VpBigData GetVpBig(Instrument instrument, DateTime? date = null) {
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
				if (currentWeek == fileWeek && DateTime.Now.Year == fileCreation.Year) {
					updateVpFile = false;
				}
			}
			if (updateVpFile) {
				string serverFile = @"https://app.suricate-trading.de/ninja/vpbig/" + instrument.MasterInstrument.Name + ".vpbig";
				using (WebClient webClient = new WebClient()) {
					webClient.DownloadFile(serverFile, fileName);
				}
			}
			string json = File.ReadAllText(fileName);
			VpBigDataSerialized s = Newtonsoft.Json.JsonConvert.DeserializeObject<VpBigDataSerialized>(json);
			VpBigData vp = ToVpBig(s);
			vp.Prepare();
			return vp;
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
		
		private static VpBigData ToVpBig(VpBigDataSerialized vpBigDataSerialized) {
			VpBigData vpBigData = new VpBigData(vpBigDataSerialized.tickSize) {
				low = vpBigDataSerialized.low,
				high = vpBigDataSerialized.high,
				totalVolume = vpBigDataSerialized.totalVolume
			};
			int tick = vpBigData.low;
			foreach (var volume in vpBigDataSerialized.tickData) {
				vpBigData.tickData[tick] = new VpTickData(tick);
				vpBigData.tickData[tick].volume = volume;
				tick++;
			}
			return vpBigData;
		}
		private static VpBigDataSerialized FromVpBig(VpBigData vpBigData) {
			VpBigDataSerialized vpBigDataSerialized = new VpBigDataSerialized {
				low = vpBigData.low,
				high = vpBigData.high,
				totalVolume = vpBigData.totalVolume,
				tickSize = vpBigData.tickSize,
				tickData = new List<long>()
			};
			foreach (var pair in vpBigData.tickData) {
				vpBigDataSerialized.tickData.Add((long) Math.Round(pair.Value.volume));
			}
			return vpBigDataSerialized;
		}
		
    }
    
    public sealed class VpBigDataSerialized {
	    public List<long> tickData;
	    public int high;
	    public int low;
	    public double totalVolume;
	    public double tickSize;
	    public DateTime date;
    }
    
}