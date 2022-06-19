#region Using declarations
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.Weiteres {
	public sealed class SuriOscillator : Indicator {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Oszillator";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				days										= 125;
				bottomLine									= 10;
				topLine										= 90;
				AddPlot(Brushes.Orange, "Oszillator");
				AddPlot(Brushes.DimGray, "Obere Linie");
				AddPlot(Brushes.DimGray, "50%");
				AddPlot(Brushes.DimGray, "Untere Linie");
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}

		protected override void OnBarUpdate() {
			if (CurrentBar < days || SuriAddOn.license == License.None) return;
			
			double min = double.MaxValue;
			double max = double.MinValue;
			for (int barsAgo = 0; barsAgo < days; barsAgo++) {
				double v = Input[barsAgo];
				if (min > v) min = v;
				if (max < v) max = v;
			}
			Value[0] = 100.0 * (Input[0] - min) / (max - min);
			Values[1][0] = topLine;
			Values[2][0] = 50;
			Values[3][0] = bottomLine;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int days { get; set; }
		
		[Range(1, 100)]
		[Display(Name="Untere Linie", Order=2, GroupName="Parameter")]
		public int bottomLine { get; set; }
		[Range(1, 100)]
		[Display(Name="Obere Linie", Order=3, GroupName="Parameter")]
		public int topLine { get; set; }
		#endregion

	}
}























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.Weiteres.SuriOscillator[] cacheSuriOscillator;
		public Suri.Weiteres.SuriOscillator SuriOscillator(int days)
		{
			return SuriOscillator(Input, days);
		}

		public Suri.Weiteres.SuriOscillator SuriOscillator(ISeries<double> input, int days)
		{
			if (cacheSuriOscillator != null)
				for (int idx = 0; idx < cacheSuriOscillator.Length; idx++)
					if (cacheSuriOscillator[idx] != null && cacheSuriOscillator[idx].days == days && cacheSuriOscillator[idx].EqualsInput(input))
						return cacheSuriOscillator[idx];
			return CacheIndicator<Suri.Weiteres.SuriOscillator>(new Suri.Weiteres.SuriOscillator(){ days = days }, input, ref cacheSuriOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Weiteres.SuriOscillator SuriOscillator(int days)
		{
			return indicator.SuriOscillator(Input, days);
		}

		public Indicators.Suri.Weiteres.SuriOscillator SuriOscillator(ISeries<double> input , int days)
		{
			return indicator.SuriOscillator(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Weiteres.SuriOscillator SuriOscillator(int days)
		{
			return indicator.SuriOscillator(Input, days);
		}

		public Indicators.Suri.Weiteres.SuriOscillator SuriOscillator(ISeries<double> input , int days)
		{
			return indicator.SuriOscillator(input, days);
		}
	}
}

#endregion
