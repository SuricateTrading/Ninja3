#region Using declarations
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriOscillator : Indicator {
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
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				Days										= 125;
				AddPlot(Brushes.Orange, "Oszillator");
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}

		protected override void OnBarUpdate() {
			if (CurrentBar < Days || SuriAddOn.license == License.None) return;
			
			double min = double.MaxValue;
			double max = double.MinValue;
			for (int barsAgo = 0; barsAgo < Days; barsAgo++) {
				double v = Input[barsAgo];
				if (min > v) min = v;
				if (max < v) max = v;
			}
			Value[0] = 100.0 * (Input[0] - min) / (max - min);
		}

		#region Properties
		[NinjaScriptProperty]
		[Browsable(false)]
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
		private Suri.SuriOscillator[] cacheSuriOscillator;
		public Suri.SuriOscillator SuriOscillator(int days)
		{
			return SuriOscillator(Input, days);
		}

		public Suri.SuriOscillator SuriOscillator(ISeries<double> input, int days)
		{
			if (cacheSuriOscillator != null)
				for (int idx = 0; idx < cacheSuriOscillator.Length; idx++)
					if (cacheSuriOscillator[idx] != null && cacheSuriOscillator[idx].Days == days && cacheSuriOscillator[idx].EqualsInput(input))
						return cacheSuriOscillator[idx];
			return CacheIndicator<Suri.SuriOscillator>(new Suri.SuriOscillator(){ Days = days }, input, ref cacheSuriOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriOscillator SuriOscillator(int days)
		{
			return indicator.SuriOscillator(Input, days);
		}

		public Indicators.Suri.SuriOscillator SuriOscillator(ISeries<double> input , int days)
		{
			return indicator.SuriOscillator(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriOscillator SuriOscillator(int days)
		{
			return indicator.SuriOscillator(Input, days);
		}

		public Indicators.Suri.SuriOscillator SuriOscillator(ISeries<double> input , int days)
		{
			return indicator.SuriOscillator(input, days);
		}
	}
}

#endregion
