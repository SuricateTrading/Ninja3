#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.NinjaScript;
using System.IO;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon.Vp {
    public static class SuriVpBigScripts {
        
	    public static void StoreVpBigToFile(bool dev = false, int commodityIndex = 0, bool onlyRecent = true) {
			KeyValuePair<Commodity, CommodityData> entry;
			try {
				entry = SuriStrings.data.ElementAt(commodityIndex);
			} catch (Exception) { return; }

			if (entry.Key == Commodity.BitcoinMicro) {
				StoreVpBigToFile(dev, commodityIndex + 1, onlyRecent);
				return;
			}

			try {
				Instrument instrument = SuriCommon.GetInstrument(entry.Value);
				DateTime from;
				if (dev) {
					from = entry.Key != Commodity.Rice ? DateTime.Parse("2010-01-01") : DateTime.Parse("2012-01-01");
				} else {
					from = entry.Key != Commodity.Rice ? DateTime.Parse("2000-01-01") : DateTime.Parse("2013-01-01");
				}
				DateTime to = DateTime.Now.AddDays(-1).Date;
				Code.Output.Process("Loading " + entry.Key + " from " + from + " to " + to, PrintTo.OutputTab1);
				
				new BarsRequest(instrument, from, to) {
					MergePolicy = dev ? MergePolicy.MergeNonBackAdjusted : MergePolicy.MergeBackAdjusted,
					BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Minute, Value = 1},
					TradingHours = instrument.MasterInstrument.TradingHours,
				}.Request((bars, errorCode, errorMessage) => {
					if (errorCode != ErrorCode.NoError) {
						Code.Output.Process("Error: " + errorCode, PrintTo.OutputTab1);
						return;
					}
					
					SuriVpBigData suriVpBigData = new SuriVpBigData(instrument.MasterInstrument.TickSize);
					string path = SuriCommon.dbPath + @"vpbig" + (dev ? @"dev\" : @"\") + instrument.MasterInstrument.Name + @"\";
					Directory.CreateDirectory(path);
					for (int i = 0; i < bars.Bars.Count; i++) {
						DateTime date = bars.Bars.GetTime(i).Date;
						if (!onlyRecent && (date - bars.Bars.GetTime(0).Date).Days > 365 * 6) {
							DateTime oldDate = bars.Bars.GetTime(i-1).Date;
							int newWeek = SuriCommon.Week(date);
							int oldWeek = SuriCommon.Week(oldDate);
							if (oldWeek - newWeek > 1 && newWeek == 2) {
								// this happens if a new year started and the first week of the year did not have a single trading day.
								Code.Output.Process("Fehlende erste Woche bei " + instrument.MasterInstrument.Name + " " + date, PrintTo.OutputTab1);
								using (StreamWriter stream = File.CreateText(SuriBigRepo.GetVpBigFilePath(instrument, dev, date.Year, 1))) {
									suriVpBigData.AddMissingValues();
									SuriVpBigDataSerialized suriVp = SuriBigRepo.FromVpBig(suriVpBigData);
									suriVp.date = new DateTime(date.Year, 1, 1);
									stream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(suriVp));
								}
							}
							
							if (oldWeek < newWeek || oldWeek - newWeek > 1) {
								using (StreamWriter stream = File.CreateText(SuriBigRepo.GetVpBigFilePath(instrument, dev, date.Year, newWeek))) {
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
					StoreVpBigToFile(dev, commodityIndex + 1, onlyRecent);
				});
				System.Threading.Thread.Sleep(1000*60);
			} catch (Exception e) {
				Code.Output.Process("Error in " + entry.Key + " " + e, PrintTo.OutputTab1);
				StoreVpBigToFile(dev, commodityIndex + 1, onlyRecent);
			}
		}
	    
    }
}