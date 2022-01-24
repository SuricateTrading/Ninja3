#region Using declarations
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Sma : Indicator {
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "SMA";
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
				AddPlot(new Stroke(Brushes.DarkOrange, 2), PlotStyle.Line, "SMA");
			}
		}
		
        public override string DisplayName {
          get {
				if (Instrument != null)
					return "SMA " + Days + " - " + SuriStrings.instrumentToName(Instrument.FullName);
				else
					return "SMA";
			}
        }
		
		protected override void OnBarUpdate() {
			Value[0] = SMA(Days)[0];
			
			if (CurrentBar > 0) {
				if (Value[0] > Value[1]) {
					PlotBrushes[0][0] = Brushes.Green;
				} else {
					PlotBrushes[0][0] = Brushes.Red;
				}
			}
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
		private Suri.Sma[] cacheSma;
		public Suri.Sma Sma(int days)
		{
			return Sma(Input, days);
		}

		public Suri.Sma Sma(ISeries<double> input, int days)
		{
			if (cacheSma != null)
				for (int idx = 0; idx < cacheSma.Length; idx++)
					if (cacheSma[idx] != null && cacheSma[idx].Days == days && cacheSma[idx].EqualsInput(input))
						return cacheSma[idx];
			return CacheIndicator<Suri.Sma>(new Suri.Sma(){ Days = days }, input, ref cacheSma);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Sma Sma(int days)
		{
			return indicator.Sma(Input, days);
		}

		public Indicators.Suri.Sma Sma(ISeries<double> input , int days)
		{
			return indicator.Sma(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Sma Sma(int days)
		{
			return indicator.Sma(Input, days);
		}

		public Indicators.Suri.Sma Sma(ISeries<double> input , int days)
		{
			return indicator.Sma(input, days);
		}
	}
}

#endregion
