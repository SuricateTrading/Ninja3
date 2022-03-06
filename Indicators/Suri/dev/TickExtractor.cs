#region Using declarations
using System;
using System.IO;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class TickExtractor : Indicator {
		private static readonly string dbPath = Globals.UserDataDir + @"db\suri\";
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "TickExtractor";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				BarsRequiredToPlot							= 0;
			} else if (State == State.DataLoaded) {
				Directory.CreateDirectory(dbPath);
				if (!Bars.IsTickReplay) return;
				VpIntraData vpIntraData = new VpIntraData();
				
				new BarsRequest(Instrument, DateTime.Now.AddYears(-10), DateTime.Now.AddDays(-1)) {
					MergePolicy		= MergePolicy.MergeBackAdjusted,
					BarsPeriod		= new BarsPeriod { BarsPeriodType = BarsPeriodType.Tick, Value = 1 },
					TradingHours	= TradingHours,
				}.Request((bars, errorCode, errorMessage) => {
					if (errorCode != ErrorCode.NoError) {
						Print(string.Format("Error on requesting bars: {0}, {1}", errorCode, errorMessage));
						return;
					}
					Print("Start");
					StreamWriter stream = File.CreateText(dbPath + @"\" + Instrument.MasterInstrument.Name + ".vpintra");

					int lastDayOfYear = -1;
					for (int i = 0; i < bars.Bars.Count; i++) {
						DateTime time = bars.Bars.GetTime(i);
						if (lastDayOfYear != time.DayOfYear) {
							if (i != 0) {
								VpBarData last = vpIntraData.barData.Last();
								last.Prepare();
								stream.Write("\n" + last.dateTime + "\t");
								foreach (var pair in last.tickData) {
									stream.Write(pair.Value.volume + "\t");
								}
							}
							vpIntraData.barData.Add(new VpBarData(TickSize, time.Date));
						}
						lastDayOfYear  = time.DayOfYear;
						vpIntraData.barData.Last().AddCached(bars.Bars.GetClose(i), bars.Bars.GetVolume(i));
					}
					Print("Done");
					stream.Close();
				});
			}
		}
		
		/*
		 protected override void OnMarketData(MarketDataEventArgs e) {
			VpIntraExtractor(e);
		}

		private readonly VpIntraData vpIntraData = new VpIntraData();
		private int? lastBar;
		private void VpIntraExtractor(MarketDataEventArgs e) {
			if (Bars.Count <= 0 || !Bars.IsTickReplay) return;
			if (lastBar != CurrentBar) {
				if (lastBar != null) {
					VpBarData d = vpIntraData.barData.Last();
					d.Prepare();
					stream.Write("\n" + d.dateTime + "\t");
					foreach (var pair in d.tickData) {
						stream.Write(pair.Value.volume + "\t");
					}
				}
				lastBar = CurrentBar;
				vpIntraData.barData.Add(new VpBarData(TickSize, e.Time.Date));
			}
			vpIntraData.barData.Last().AddTick(e);
		}
		
		
		protected override void OnBarUpdate() {
			//ExportBarData();
		}

		private void ExportTicks(MarketDataEventArgs e) {
			if (Bars.Count == 0) return;
			if (previousYear != e.Time.Year) {
				previousYear = e.Time.Year;
				if (stream != null) stream.Close();
				stream = File.CreateText(@"C:\Users\Bo\Desktop\ticks\" + Instrument.MasterInstrument.Name + "_" + previousYear + ".csv");
				stream.WriteLine("Price;Volume;Time");
			}
			stream.WriteLine(e.Price + ";" + e.Volume + ";" + e.Time);
		}

		private void ExportBarData() {
			if (Bars.IsTickReplay) return;
			if (previousYear != Time[0].Year) {
				previousYear = Time[0].Year;
				if (stream != null) stream.Close();
				stream = File.CreateText(@"C:\Users\Bo\Desktop\ticks\" + Instrument.MasterInstrument.Name + "_" + previousYear + ".csv");
				stream.WriteLine("Price;Volume;Time");
			}
			stream.WriteLine(Close[0] + ";" + Volume[0] + ";" + Time[0]);
		}
		*/
	}
}































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.TickExtractor[] cacheTickExtractor;
		public Suri.dev.TickExtractor TickExtractor()
		{
			return TickExtractor(Input);
		}

		public Suri.dev.TickExtractor TickExtractor(ISeries<double> input)
		{
			if (cacheTickExtractor != null)
				for (int idx = 0; idx < cacheTickExtractor.Length; idx++)
					if (cacheTickExtractor[idx] != null &&  cacheTickExtractor[idx].EqualsInput(input))
						return cacheTickExtractor[idx];
			return CacheIndicator<Suri.dev.TickExtractor>(new Suri.dev.TickExtractor(), input, ref cacheTickExtractor);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.TickExtractor TickExtractor()
		{
			return indicator.TickExtractor(Input);
		}

		public Indicators.Suri.dev.TickExtractor TickExtractor(ISeries<double> input )
		{
			return indicator.TickExtractor(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.TickExtractor TickExtractor()
		{
			return indicator.TickExtractor(Input);
		}

		public Indicators.Suri.dev.TickExtractor TickExtractor(ISeries<double> input )
		{
			return indicator.TickExtractor(input);
		}
	}
}

#endregion
