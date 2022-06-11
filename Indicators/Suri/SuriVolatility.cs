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

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriVolatility : Indicator {
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Tage", Order = 0, Description = "Periode in Bars", GroupName = "Parameter")]
		public int days { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Wert in Dollar (an) oder Ticks (aus)", Order = 1, Description = "", GroupName = "Parameter")]
		public bool showInDollar { get; set; }
		
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
				Description									= @"Berechnet die durchschnittliche Bargröße inklusive Gaps der letzten x Tage.";
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
				days										= 50;
				lineBrush									= Brushes.DarkGray;
				showInDollar								= true;
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
	        if (SuriAddOn.license == License.None || CurrentBar <= days) return;

	        double value = 0;
	        for (int i = 0; i <= days; i++) {
		        value += Math.Max(Close[i + 1], High[i]) - Math.Min(Close[i + 1], Low[i]);
	        }
	        Values[0][0] = value / days;
	        if (showInDollar) Values[0][0] *= Instrument.MasterInstrument.PointValue;
	        else Values[0][0] /= Instrument.MasterInstrument.TickSize;
        }
		
	}
}
































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriVolatility[] cacheSuriVolatility;
		public Suri.SuriVolatility SuriVolatility(int days, bool showInDollar)
		{
			return SuriVolatility(Input, days, showInDollar);
		}

		public Suri.SuriVolatility SuriVolatility(ISeries<double> input, int days, bool showInDollar)
		{
			if (cacheSuriVolatility != null)
				for (int idx = 0; idx < cacheSuriVolatility.Length; idx++)
					if (cacheSuriVolatility[idx] != null && cacheSuriVolatility[idx].days == days && cacheSuriVolatility[idx].showInDollar == showInDollar && cacheSuriVolatility[idx].EqualsInput(input))
						return cacheSuriVolatility[idx];
			return CacheIndicator<Suri.SuriVolatility>(new Suri.SuriVolatility(){ days = days, showInDollar = showInDollar }, input, ref cacheSuriVolatility);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriVolatility SuriVolatility(int days, bool showInDollar)
		{
			return indicator.SuriVolatility(Input, days, showInDollar);
		}

		public Indicators.Suri.SuriVolatility SuriVolatility(ISeries<double> input , int days, bool showInDollar)
		{
			return indicator.SuriVolatility(input, days, showInDollar);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriVolatility SuriVolatility(int days, bool showInDollar)
		{
			return indicator.SuriVolatility(Input, days, showInDollar);
		}

		public Indicators.Suri.SuriVolatility SuriVolatility(ISeries<double> input , int days, bool showInDollar)
		{
			return indicator.SuriVolatility(input, days, showInDollar);
		}
	}
}

#endregion
