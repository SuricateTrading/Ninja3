#region Using declarations
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Oscillator : Indicator {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Oszillator";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				Days										= 125;
				AddPlot(Brushes.Orange, "Oszillator");
			}
		}
		
		protected override void OnBarUpdate() {
			if (CurrentBar < Days) return;
			
			double? min = null, max = null;
			for (int barsAgo = 0; barsAgo < Days; barsAgo++) {
				double v = Input[barsAgo];
				if (min == null || min > v) min = v;
				if (max == null || max < v) max = v;
			}
			// min and max cannot be null at this point
			Value[0] = 100.0 * (Input[0] - min.Value) / (max.Value - min.Value);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int Days
		{ get; set; }
		#endregion

	}
}























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.Oscillator[] cacheOscillator;
		public Suri.Oscillator Oscillator(int days)
		{
			return Oscillator(Input, days);
		}

		public Suri.Oscillator Oscillator(ISeries<double> input, int days)
		{
			if (cacheOscillator != null)
				for (int idx = 0; idx < cacheOscillator.Length; idx++)
					if (cacheOscillator[idx] != null && cacheOscillator[idx].Days == days && cacheOscillator[idx].EqualsInput(input))
						return cacheOscillator[idx];
			return CacheIndicator<Suri.Oscillator>(new Suri.Oscillator(){ Days = days }, input, ref cacheOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Oscillator Oscillator(int days)
		{
			return indicator.Oscillator(Input, days);
		}

		public Indicators.Suri.Oscillator Oscillator(ISeries<double> input , int days)
		{
			return indicator.Oscillator(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Oscillator Oscillator(int days)
		{
			return indicator.Oscillator(Input, days);
		}

		public Indicators.Suri.Oscillator Oscillator(ISeries<double> input , int days)
		{
			return indicator.Oscillator(input, days);
		}
	}
}

#endregion
