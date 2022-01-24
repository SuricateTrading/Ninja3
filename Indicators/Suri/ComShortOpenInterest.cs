#region Using declarations

using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class ComShortOpenInterest : AbstractRange {
		private CotBase comShort;
		private CotBase openInterest;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "Com Short / OI in %");
				base.OnStateChange();
				Plots[1].Width = 1; // 75%
				Plots[3].Width = 1; // 25%
				
				Description									= @"Com Short / OI";
				Name										= "Com Short / OI in %";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
				comShort = CotBase(SuriCotReportField.CommercialShort);
				openInterest = CotBase(SuriCotReportField.OpenInterest);
			}
		}

		protected override void OnBarUpdate() {
			Values[0][0] = 100 * comShort.Value[0] / openInterest.Value[0];
			CalcMinMax(Values[0][0]);
		}
		
		public override void OnCalculateMinMax() {
			MinValue = 0;
			MaxValue = 100;
		}
	}
}


































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.ComShortOpenInterest[] cacheComShortOpenInterest;
		public Suri.ComShortOpenInterest ComShortOpenInterest()
		{
			return ComShortOpenInterest(Input);
		}

		public Suri.ComShortOpenInterest ComShortOpenInterest(ISeries<double> input)
		{
			if (cacheComShortOpenInterest != null)
				for (int idx = 0; idx < cacheComShortOpenInterest.Length; idx++)
					if (cacheComShortOpenInterest[idx] != null &&  cacheComShortOpenInterest[idx].EqualsInput(input))
						return cacheComShortOpenInterest[idx];
			return CacheIndicator<Suri.ComShortOpenInterest>(new Suri.ComShortOpenInterest(), input, ref cacheComShortOpenInterest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest()
		{
			return indicator.ComShortOpenInterest(Input);
		}

		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest(ISeries<double> input )
		{
			return indicator.ComShortOpenInterest(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest()
		{
			return indicator.ComShortOpenInterest(Input);
		}

		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest(ISeries<double> input )
		{
			return indicator.ComShortOpenInterest(input);
		}
	}
}

#endregion
