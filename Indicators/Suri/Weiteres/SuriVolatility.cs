#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.Weiteres {
	public sealed class SuriVolatility : Indicator {
		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "Bars", Order = 0, Description = "Tage", GroupName = "Parameter")]
		public int? days { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Wert in Dollar (an) oder Ticks (aus)", Order = 1, Description = "", GroupName = "Parameter")]
		public bool showInDollar { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Klassische Vola (an) oder Suri-Vola (aus)", Order = 2, Description = "", GroupName = "Parameter")]
		public bool classicVolatility { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Farbe", Order = 2, GroupName = "Parameter")]
		public Brush lineBrush { get; set; }
		[Browsable(false)]
		public string lineBrushSerialize {
			get { return Serialize.BrushToString(lineBrush); }
			set { lineBrush = Serialize.StringToBrush(value); }
		}
		#endregion

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Volatilität.";
				Name										= "Volatilität";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				lineBrush									= Brushes.DarkGray;
				showInDollar								= true;
				days										= 50;
				classicVolatility							= false;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(lineBrush, 1), PlotStyle.Line, "Volatilität");
			}
		}
		public override string DisplayName { get { return Name + (showInDollar ? " in Dollar" : " in Ticks"); } }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        base.OnRender(chartControl, chartScale);
	        if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
        }

        protected override void OnBarUpdate() {
	        if (SuriAddOn.license == License.None) return;

	        int bars = days ?? CurrentBar;
	        if (classicVolatility) {
		        if (CurrentBar == 0 || days != null && CurrentBar < days) return;

		        double mean = 0.0;
		        for (int i = 0; i < bars; i++) mean += Close[i];
		        mean /= bars;

		        double deviation = 0.0;
		        for (int i = 0; i < bars; i++) deviation += Math.Pow(Close[i] - mean, 2.0);
		        deviation /= bars;

		        Value[0] = deviation;
	        } else {
		        if (bars < 1 || days != null && CurrentBar <= days) return;

		        double value = 0;
		        for (int i = 0; i < bars; i++) {
			        value += Math.Max(Close[i + 1], High[i]) - Math.Min(Close[i + 1], Low[i]);
		        }
		        Value[0] = value / bars;
	        }
	        
	        if (showInDollar) Value[0] *= Instrument.MasterInstrument.PointValue;
	        else Value[0] /= Instrument.MasterInstrument.TickSize;
        }
		
	}
}
































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.Weiteres.SuriVolatility[] cacheSuriVolatility;
		public Suri.Weiteres.SuriVolatility SuriVolatility(int? days, bool showInDollar, bool classicVolatility)
		{
			return SuriVolatility(Input, days, showInDollar, classicVolatility);
		}

		public Suri.Weiteres.SuriVolatility SuriVolatility(ISeries<double> input, int? days, bool showInDollar, bool classicVolatility)
		{
			if (cacheSuriVolatility != null)
				for (int idx = 0; idx < cacheSuriVolatility.Length; idx++)
					if (cacheSuriVolatility[idx] != null && cacheSuriVolatility[idx].days == days && cacheSuriVolatility[idx].showInDollar == showInDollar && cacheSuriVolatility[idx].classicVolatility == classicVolatility && cacheSuriVolatility[idx].EqualsInput(input))
						return cacheSuriVolatility[idx];
			return CacheIndicator<Suri.Weiteres.SuriVolatility>(new Suri.Weiteres.SuriVolatility(){ days = days, showInDollar = showInDollar, classicVolatility = classicVolatility }, input, ref cacheSuriVolatility);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Weiteres.SuriVolatility SuriVolatility(int? days, bool showInDollar, bool classicVolatility)
		{
			return indicator.SuriVolatility(Input, days, showInDollar, classicVolatility);
		}

		public Indicators.Suri.Weiteres.SuriVolatility SuriVolatility(ISeries<double> input , int? days, bool showInDollar, bool classicVolatility)
		{
			return indicator.SuriVolatility(input, days, showInDollar, classicVolatility);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Weiteres.SuriVolatility SuriVolatility(int? days, bool showInDollar, bool classicVolatility)
		{
			return indicator.SuriVolatility(Input, days, showInDollar, classicVolatility);
		}

		public Indicators.Suri.Weiteres.SuriVolatility SuriVolatility(ISeries<double> input , int? days, bool showInDollar, bool classicVolatility)
		{
			return indicator.SuriVolatility(input, days, showInDollar, classicVolatility);
		}
	}
}

#endregion
