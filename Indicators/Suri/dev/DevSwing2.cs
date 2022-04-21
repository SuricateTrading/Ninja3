#region Using declarations

using System.Linq;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class SuriSwing2 : Indicator {
		private Brush brushHigh;
		private Brush brushLow;

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Name						= "Suri Swing 2";
				Description					= "Suri Swing 2";
				DisplayInDataBox			= false;
				PaintPriceMarkers			= false;
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
			} else if (State == State.DataLoaded) {
				brushHigh = Brushes.Green.Clone();
				brushLow  = Brushes.Red  .Clone();
				brushHigh.Opacity = 0.8;
				brushLow .Opacity = 0.8;
			}
		}

		private double lastHigh;
		private double lastLow;
		private int lastHighBar;
		private int lastLowBar;
		private int noNewHighInaRow;
		private int noNewLowInaRow;

		private int noNewCount = 9;

		private bool comesFromHigh;
		private bool comesFromLow = true;
		
		protected override void OnBarUpdate() {
			if (CurrentBar == 0) {
				lastHigh = High[0];
				lastLow = Low[0];
				return;
			}
			if (comesFromLow)  Check(true);
			if (comesFromHigh) Check(false);
		}

		private void Check(bool high) {
			for (int i = 1; i < noNewCount && CurrentBar + i < Bars.Count; i++) {
				double value = high ? Bars.GetHigh(CurrentBar + i) : Bars.GetLow(CurrentBar + i);
				if (high ? value > High[0] : value < Low[0]) break;
				if (i == noNewCount - 1) {
					// we did not find a lower low or higher high
					comesFromHigh = high;
					comesFromLow = !high;
					Draw.Dot(this, (high ? "High_" : "Low_") + SuriCommon.random, true, 0, high ? High[0] : Low[0], high ? brushHigh : brushLow);
				}
			}
		}

	}
}



























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.SuriSwing2[] cacheSuriSwing2;
		public Suri.dev.SuriSwing2 SuriSwing2()
		{
			return SuriSwing2(Input);
		}

		public Suri.dev.SuriSwing2 SuriSwing2(ISeries<double> input)
		{
			if (cacheSuriSwing2 != null)
				for (int idx = 0; idx < cacheSuriSwing2.Length; idx++)
					if (cacheSuriSwing2[idx] != null &&  cacheSuriSwing2[idx].EqualsInput(input))
						return cacheSuriSwing2[idx];
			return CacheIndicator<Suri.dev.SuriSwing2>(new Suri.dev.SuriSwing2(), input, ref cacheSuriSwing2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.SuriSwing2 SuriSwing2()
		{
			return indicator.SuriSwing2(Input);
		}

		public Indicators.Suri.dev.SuriSwing2 SuriSwing2(ISeries<double> input )
		{
			return indicator.SuriSwing2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.SuriSwing2 SuriSwing2()
		{
			return indicator.SuriSwing2(Input);
		}

		public Indicators.Suri.dev.SuriSwing2 SuriSwing2(ISeries<double> input )
		{
			return indicator.SuriSwing2(input);
		}
	}
}

#endregion
