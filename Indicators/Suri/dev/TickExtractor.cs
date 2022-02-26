#region Using declarations

using System.IO;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class TickExtractor : Indicator {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "TickExtractor";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				IsChartOnly									= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				ZOrder										= 0;
			} else if (State == State.Transition) {
				if (stream != null) stream.Close();
			}
		}
		private int previousYear;
		private StreamWriter stream;
		
		/*protected override void OnMarketData(MarketDataEventArgs e) {
			if (Bars.Count == 0) return;
			if (previousYear != e.Time.Year) {
				previousYear = e.Time.Year;
				if (stream != null) stream.Close();
				stream = File.CreateText(@"C:\Users\Bo\Desktop\ticks\" + Instrument.MasterInstrument.Name + "_" + previousYear + ".csv");
				stream.WriteLine("Price;Volume;Time");
			}
			stream.WriteLine(e.Price + ";" + e.Volume + ";" + e.Time);
		}*/

		protected override void OnBarUpdate() {
			if (Bars.IsTickReplay) return;
			if (previousYear != Time[0].Year) {
				previousYear = Time[0].Year;
				if (stream != null) stream.Close();
				stream = File.CreateText(@"C:\Users\Bo\Desktop\ticks\" + Instrument.MasterInstrument.Name + "_" + previousYear + ".csv");
				stream.WriteLine("Price;Volume;Time");
			}
			stream.WriteLine(Close[0] + ";" + Volume[0] + ";" + Time[0]);
		}
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
