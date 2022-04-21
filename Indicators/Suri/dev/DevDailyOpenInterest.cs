#region Using declarations
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public sealed class SuriDailyOpenInterest : Indicator {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"SuriDailyOpenInterest";
				Name										= "SuriDailyOpenInterest";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
			} else if (State == State.Configure) {
				//AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "COT1");
			}
		}
		public override string DisplayName { get { return Name; } }
        
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        base.OnRender(chartControl, chartScale);
	        if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
        }
        
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None) return;
			
			if (!(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Day && Bars.BarsPeriod.Value == 1 || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && Bars.BarsPeriod.Value == 1440)) {
				Draw.TextFixed(this, "Warning", "CoT 1 ist nur für ein 1-Tages Chart oder 1440-Minuten Chart verfügbar.", TextPosition.Center);
				return;
			}
			
			//..
		}

    }
}



































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.SuriDailyOpenInterest[] cacheSuriDailyOpenInterest;
		public Suri.dev.SuriDailyOpenInterest SuriDailyOpenInterest()
		{
			return SuriDailyOpenInterest(Input);
		}

		public Suri.dev.SuriDailyOpenInterest SuriDailyOpenInterest(ISeries<double> input)
		{
			if (cacheSuriDailyOpenInterest != null)
				for (int idx = 0; idx < cacheSuriDailyOpenInterest.Length; idx++)
					if (cacheSuriDailyOpenInterest[idx] != null &&  cacheSuriDailyOpenInterest[idx].EqualsInput(input))
						return cacheSuriDailyOpenInterest[idx];
			return CacheIndicator<Suri.dev.SuriDailyOpenInterest>(new Suri.dev.SuriDailyOpenInterest(), input, ref cacheSuriDailyOpenInterest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.SuriDailyOpenInterest SuriDailyOpenInterest()
		{
			return indicator.SuriDailyOpenInterest(Input);
		}

		public Indicators.Suri.dev.SuriDailyOpenInterest SuriDailyOpenInterest(ISeries<double> input )
		{
			return indicator.SuriDailyOpenInterest(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.SuriDailyOpenInterest SuriDailyOpenInterest()
		{
			return indicator.SuriDailyOpenInterest(Input);
		}

		public Indicators.Suri.dev.SuriDailyOpenInterest SuriDailyOpenInterest(ISeries<double> input )
		{
			return indicator.SuriDailyOpenInterest(input);
		}
	}
}

#endregion
