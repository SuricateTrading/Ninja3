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
    public static class SuriVpIntraScripts {
        
	    public static void StoreVpIntra(int commodityIndex = 0, BarsPeriodType barsPeriodType = BarsPeriodType.Minute, int barsPeriodValue = 60) {
		    KeyValuePair<Commodity, CommodityData> entry;
		    try {
			    entry = SuriStrings.data.ElementAt(commodityIndex);
		    } catch (Exception) {
			    Code.Output.Process("Could not find commodityIndex " + commodityIndex, PrintTo.OutputTab1);
			    return;
		    }
			Instrument instrument = SuriCommon.GetInstrument(entry.Value);
			DateTime from = DateTime.Now.AddYears(-1).Date;
			DateTime to = DateTime.Now.AddDays(-1).Date;
			Code.Output.Process("Loading " + instrument.MasterInstrument.Name + " from " + from + " to " + to, PrintTo.OutputTab1);

			new BarsRequest(instrument, from.AddDays(-10), to.AddDays(10)) {
				MergePolicy = MergePolicy.MergeBackAdjusted,
				BarsPeriod = new BarsPeriod {BarsPeriodType = barsPeriodType, Value = barsPeriodValue},
				TradingHours = instrument.MasterInstrument.TradingHours,
			}.Request((minuteBars, minuteErrorCode, minuteErrorMessage) => {
				if (minuteErrorCode != ErrorCode.NoError) return;
				new BarsRequest(instrument, from, to) {
					MergePolicy = MergePolicy.MergeBackAdjusted,
					BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Tick, Value = 1},
					TradingHours = instrument.MasterInstrument.TradingHours,
				}.Request((bars, errorCode, errorMessage) => {
					if (errorCode != ErrorCode.NoError) return;

					int minuteBarIndex = 0;
					var suriVpIntraData = new SuriVpIntraData();
					for (int i = 0; i < bars.Bars.Count; i++) {
						bool newBar = false;
						while (minuteBars.Bars.GetTime(minuteBarIndex) <= bars.Bars.GetTime(i)) {
							minuteBarIndex++;
							newBar = true;
						}
						if (!newBar) {
							suriVpIntraData.barData.Last().AddTick(bars.Bars.GetTime(i), bars.Bars.GetClose(i), bars.Bars.GetVolume(i), bars.Bars.GetAsk(i), bars.Bars.GetBid(i));
							continue;
						}
						if (minuteBars.Bars.GetTime(minuteBarIndex).Month != minuteBars.Bars.GetTime(minuteBarIndex - 1).Month) {
							// new month detected. store old month to file.
							File.WriteAllText(
								SuriCommon.dbPath + @"vpintra\" + entry.Value.id + "_" + minuteBars.Bars.GetTime(minuteBarIndex - 1).Year + "_" + minuteBars.Bars.GetTime(minuteBarIndex - 1).Month + ".vpintra",
								Newtonsoft.Json.JsonConvert.SerializeObject(suriVpIntraData)
							);
							suriVpIntraData = new SuriVpIntraData();
						}
						suriVpIntraData.barData.Add(new SuriVpBarData(instrument.MasterInstrument.TickSize, minuteBars.Bars.GetTime(minuteBarIndex)));
					}
					DateTime lastDate = bars.Bars.GetTime(bars.Bars.Count - 1);
					File.WriteAllText(
						SuriCommon.dbPath + @"vpintra\" + entry.Value.id + "_" + lastDate.Year + "_" + lastDate.Month + ".vpintra",
						Newtonsoft.Json.JsonConvert.SerializeObject(suriVpIntraData)
					);
					
					Code.Output.Process("Done", PrintTo.OutputTab1);
					//StoreVpIntra(commodityIndex + 1);
				});
			});
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
				Instrument instrument = SuriCommon.GetInstrument(entry.Value);
				DateTime from = DateTime.Parse("2015-01-01").Date;
				DateTime to = DateTime.Now.AddDays(-1).Date;
				Code.Output.Process("Loading " + instrument.MasterInstrument.Name + " from " + from + " to " + to, PrintTo.OutputTab1);
				
				new BarsRequest(instrument, from, to) {
					MergePolicy = MergePolicy.MergeBackAdjusted,
					BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Tick, Value = 1},
					TradingHours = instrument.MasterInstrument.TradingHours,
				}.Request((bars, errorCode, errorMessage) => {
					if (errorCode != ErrorCode.NoError) return;
					StreamWriter stream = File.CreateText(SuriCommon.dbPath + @"ticks\" + instrument.MasterInstrument.Name + "_" + bars.Bars.GetTime(0).Year + "_" + bars.Bars.GetTime(0).Month + ".tickdata");
					for (int i = 0; i < bars.Bars.Count; i++) {
						if (i > 0 && bars.Bars.GetTime(i).Month != bars.Bars.GetTime(i - 1).Month) {
							stream.Close();
							stream = File.CreateText(SuriCommon.dbPath + @"ticks\" + instrument.MasterInstrument.Name + "_" + bars.Bars.GetTime(i).Year + "_" + bars.Bars.GetTime(i).Month + ".tickdata");
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
