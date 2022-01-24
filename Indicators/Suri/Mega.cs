#region Using declarations
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using Indicators_SMA = NinjaTrader.NinjaScript.Indicators.SMA;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Mega : Indicator {
		private SuriRange suriRange;
		private Volumen volumen;
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Mega";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				BarsRequiredToPlot							= 1;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				Periode										= 125;
				
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Bar, "Mega %");
			} else if (State == State.Configure) {
				suriRange = SuriRange(Periode);
				volumen = Volumen(Periode);
			}
		}

		public override string DisplayName {
			get {
				if (Instrument == null) return "Mega";
				return "Mega " + Periode + " Tage - " + SuriStrings.instrumentToName(Instrument.FullName);
			}
		}
		
		protected override void OnBarUpdate() {
			Value[0] = suriRange.Percentage() > 99 || volumen.Percentage() > 99 ? 1 : 0;
		}
		
		#region Properties
		[NinjaScriptProperty] [Range(1, int.MaxValue)]
		[Display(Name = "Tage", Description = "Periode in Bars", GroupName = "Parameter")]
		public int Periode { get; set; }
		#endregion
		
	}
}





























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.Mega[] cacheMega;
		public Suri.Mega Mega(int periode)
		{
			return Mega(Input, periode);
		}

		public Suri.Mega Mega(ISeries<double> input, int periode)
		{
			if (cacheMega != null)
				for (int idx = 0; idx < cacheMega.Length; idx++)
					if (cacheMega[idx] != null && cacheMega[idx].Periode == periode && cacheMega[idx].EqualsInput(input))
						return cacheMega[idx];
			return CacheIndicator<Suri.Mega>(new Suri.Mega(){ Periode = periode }, input, ref cacheMega);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Mega Mega(int periode)
		{
			return indicator.Mega(Input, periode);
		}

		public Indicators.Suri.Mega Mega(ISeries<double> input , int periode)
		{
			return indicator.Mega(input, periode);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Mega Mega(int periode)
		{
			return indicator.Mega(Input, periode);
		}

		public Indicators.Suri.Mega Mega(ISeries<double> input , int periode)
		{
			return indicator.Mega(input, periode);
		}
	}
}

#endregion
