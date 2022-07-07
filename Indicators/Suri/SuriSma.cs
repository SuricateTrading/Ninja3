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
	public sealed class SuriSma : Indicator {
		private double priorSum;
		private double sum;
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=0, GroupName="Parameter")]
		public int days
		{ get; set; }
		
		[Display(Name="Zeige normalen SMA (aus) oder Steigungswinkel (an)", Order=1, GroupName="Parameter")]
		public bool useSlope
		{ get; set; }
		
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der Linien", Order=2, GroupName="Parameter")]
		public int lineWidth
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name = "Farbe wenn steigend", Order = 3, GroupName = "Parameter")]
		public Brush longBrush { get; set; }
		[Browsable(false)]
		public string longBrushSerialize {
			get { return Serialize.BrushToString(longBrush); }
			set { longBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Farbe wenn fallend", Order = 4, GroupName = "Parameter")]
		public Brush shortBrush { get; set; }
		[Browsable(false)]
		public string shortBrushSerialize {
			get { return Serialize.BrushToString(shortBrush); }
			set { shortBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "0% Linie", Order = 5, GroupName = "Parameter")]
		public Brush zeroLineBrush { get; set; }
		[Browsable(false)]
		public string zeroLineBrushSerialize {
			get { return Serialize.BrushToString(zeroLineBrush); }
			set { zeroLineBrush = Serialize.StringToBrush(value); }
		}
		#endregion

		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "SMA";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				lineWidth									= 2;
				longBrush									= Brushes.Green;
				shortBrush									= Brushes.Red;
				zeroLineBrush								= Brushes.DimGray;
				days										= 125;
				useSlope									= false;
			} else if (State == State.Configure) {
				priorSum	= 0;
				sum			= 0;
				AddPlot(new Stroke(Brushes.Yellow, lineWidth), PlotStyle.Line, useSlope ? "Steigungswinkel x 100" : "SMA");
				AddPlot(new Stroke(Brushes.Transparent, 0), PlotStyle.Line, "Calculation only");
				AddPlot(new Stroke(useSlope ? zeroLineBrush : Brushes.Transparent, 1), PlotStyle.Line, "0%");
			}
		}
		public override string DisplayName { get { return Name; } }
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}
		
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None) return;
			
			if (IsFirstTickOfBar) priorSum = sum;
			sum = priorSum + Input[0] - (CurrentBar >= days ? Input[days] : 0.0);
			Values[0][0] = sum / (CurrentBar < days ? CurrentBar + 1 : days);
			Values[1][0] = Values[0][0];

			if (useSlope) {
				if (CurrentBar == 0) {
					Values[0][0] = 0;	
				} else {
					Values[0][0] = Math.Atan(Values[1][0] - Values[1][1]) * 100;
					Values[2][0] = 0;	
				}
			}
			
			if (SuriAddOn.license != License.Basic) {
				if (CurrentBar < days) {
					PlotBrushes[0][0] = Brushes.Transparent;
					PlotBrushes[2][0] = Brushes.Transparent;
				} else {
					if      (useSlope && Value[0] > 0 || !useSlope && Value[0] > Value[1]) PlotBrushes[0][0] = longBrush;
					else if (useSlope && Value[0] < 0 || !useSlope && Value[0] < Value[1]) PlotBrushes[0][0] = shortBrush;
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
		private Suri.SuriSma[] cacheSuriSma;
		public Suri.SuriSma SuriSma(int days)
		{
			return SuriSma(Input, days);
		}

		public Suri.SuriSma SuriSma(ISeries<double> input, int days)
		{
			if (cacheSuriSma != null)
				for (int idx = 0; idx < cacheSuriSma.Length; idx++)
					if (cacheSuriSma[idx] != null && cacheSuriSma[idx].days == days && cacheSuriSma[idx].EqualsInput(input))
						return cacheSuriSma[idx];
			return CacheIndicator<Suri.SuriSma>(new Suri.SuriSma(){ days = days }, input, ref cacheSuriSma);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriSma SuriSma(int days)
		{
			return indicator.SuriSma(Input, days);
		}

		public Indicators.Suri.SuriSma SuriSma(ISeries<double> input , int days)
		{
			return indicator.SuriSma(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriSma SuriSma(int days)
		{
			return indicator.SuriSma(Input, days);
		}

		public Indicators.Suri.SuriSma SuriSma(ISeries<double> input , int days)
		{
			return indicator.SuriSma(input, days);
		}
	}
}

#endregion
