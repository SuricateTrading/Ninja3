#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.NinjaScript;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class VpSerialization {
        
		public static void LoadVpIntra() {
			try {
				foreach (KeyValuePair<Commodity,CommodityData> entry in SuriStrings.data) {
					if (entry.Key != Commodity.Coffee) continue;
					
					Instrument instrument = Instrument.All.Where(x => x.MasterInstrument.Name == entry.Value.shortName && x.MasterInstrument.InstrumentType == InstrumentType.Future && x.Expiry.Date > DateTime.Now)
						.OrderBy(o => o.Expiry.Date).First();
					string dbPath = Globals.UserDataDir + @"db\suri\" + instrument.MasterInstrument.Name + ".vpintra";
					
					DateTime from = DateTime.Now.AddDays(-50);
					DateTime to = DateTime.Now.AddDays(-1);
					
					try {
						string[] lines = File.ReadAllLines(dbPath);
						from = DateTime.Parse(lines.Last().Substring(0, 10)).AddDays(1);
					} catch (Exception) { /**/ }
					Code.Output.Process("Loading from " + from + " to " + to, PrintTo.OutputTab1);
					
					new BarsRequest(instrument, from, to) {
						MergePolicy = MergePolicy.MergeBackAdjusted,
						BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Tick, Value = 1},
						TradingHours = instrument.MasterInstrument.TradingHours,
					}.Request((bars, errorCode, errorMessage) => {
						if (errorCode != ErrorCode.NoError) return;
						StreamWriter stream = File.AppendText(dbPath);

						VpIntraData vpIntraData = new VpIntraData();
						int lastDayOfYear = -1;
						for (int i = 0; i < bars.Bars.Count; i++) {
							DateTime time = bars.Bars.GetTime(i);
							if (lastDayOfYear != time.DayOfYear) {
								if (i != 0) {
									Export(stream, vpIntraData);
								}
								vpIntraData.barData.Add(new VpBarData(instrument.MasterInstrument.TickSize, time.Date));
							}
							lastDayOfYear  = time.DayOfYear;
							vpIntraData.barData.Last().AddCached(bars.Bars.GetClose(i), bars.Bars.GetVolume(i));
						}
						Export(stream, vpIntraData);
						stream.Close();
						Code.Output.Process("Done", PrintTo.OutputTab1);
					});
				}
			} catch (Exception e) {
				Code.Output.Process(e.ToString(), PrintTo.OutputTab1);
			}
		}

		private static void Export(StreamWriter stream, VpIntraData vpIntraData) {
			var serializer = new JavaScriptSerializer();
			
			VpBarData last = vpIntraData.barData.Last();
			last.Prepare();
			stream.Write("\n" + Newtonsoft.Json.JsonConvert.SerializeObject(last));


			/*stream.Write("\n" + last.dateTime + "\t");
			foreach (var pair in last.tickData) {
				stream.Write(pair.Value.volume + "\t");
				
			}*/
		}
		
    }
}