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
	public sealed class SuriBarRange : Indicator {
		private double max;
		private int maxIndex;
		
		#region Properties
		[NinjaScriptProperty]
		[Browsable(false)]
		[Range(1, int.MaxValue)]
		[Display(Name = "Tage", Description = "Periode in Bars", GroupName = "Parameter")]
		public int days { get; set; }
		
		#region Colors
		[XmlIgnore]
		[Display(Name = "Bargröße", Order = 0, GroupName = "Farben")]
		public Brush barBrush { get; set; }
		[Browsable(false)]
		public string barBrushSerialize {
			get { return Serialize.BrushToString(barBrush); }
			set { barBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Megabar steigend", Order = 2, GroupName = "Farben")]
		public Brush signalUpBrush { get; set; }
		[Browsable(false)]
		public string signalUpBrushSerialize {
			get { return Serialize.BrushToString(signalUpBrush); }
			set { signalUpBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Megabar fallend", Order = 3, GroupName = "Farben")]
		public Brush signalDownBrush { get; set; }
		[Browsable(false)]
		public string signalDownBrushSerialize {
			get { return Serialize.BrushToString(signalDownBrush); }
			set { signalDownBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Max", Order = 1, GroupName = "Farben")]
		public Brush maxBrush { get; set; }
		[Browsable(false)]
		public string maxBrushSerialize {
			get { return Serialize.BrushToString(maxBrush); }
			set { maxBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Nicht genügend Daten", Order = 3, GroupName = "Farben")]
		public Brush notEnoughDataBrush { get; set; }
		[Browsable(false)]
		public string notEnoughDataBrushSerialize {
			get { return Serialize.BrushToString(notEnoughDataBrush); }
			set { notEnoughDataBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		#endregion

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Zeigt die Range einer Bar plus Gap zum Vortag.";
				Name										= "Bargröße";
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
				days										= 125;
				signalUpBrush								= Brushes.Green;
				signalDownBrush								= Brushes.Red;
				maxBrush									= Brushes.DarkCyan;
				barBrush									= Brushes.RoyalBlue;
				notEnoughDataBrush							= Brushes.Gray;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(barBrush, 2), PlotStyle.Bar, "Bargröße");
				AddPlot(new Stroke(maxBrush, 1), PlotStyle.Line, "Max");
			}
		}
		public override string DisplayName { get { return Name; } }
        public double Percentage() { return 100 * Values[0][0] / Values[1][0]; }
        public bool IsMegaRange() { return CurrentBar > days && Math.Abs(Values[0][0] - Values[1][0]) < 0.00000001; }
        public bool IsMegaRange(int barIndex) { return Math.Abs(Values[0].GetValueAt(barIndex) - Values[1].GetValueAt(barIndex)) < 0.00000001; }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        base.OnRender(chartControl, chartScale);
	        if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
        }

        protected override void OnBarUpdate() {
	        if (SuriAddOn.license == License.None) return;
	        if (CurrentBar != 0) {
		        Values[0][0] = Math.Max(Close[1], High[0]) - Math.Min(Close[1], Low[0]);
	        } else {
		        Values[0][0] = Close[0] - High[0];
	        }
			
	        if (Values[0][0] >= max) {
		        maxIndex = CurrentBar;
		        max = Values[0][0];
	        } else {
		        // wenn das Hoch weiter weg ist als die Periode dann neues Hoch innerhalb der Periode suchen
		        if (CurrentBar - maxIndex > days) {
			        max = 0;
			        for (int i = days; i > 0 ; i--) {
				        if (Values[0][i] >= max) {
					        maxIndex = CurrentBar - i;
					        max = Values[0][i];
				        }
			        }
		        }
	        }
	        Values[1][0] = max;
	        if (CurrentBar < days) {
		        PlotBrushes[0][0] = notEnoughDataBrush;
	        } else if(SuriAddOn.license != License.Basic && IsMegaRange()) {
		        if (Open[0] <= Close[0]) {
			        PlotBrushes[0][0] = signalUpBrush;
		        } else {
			        PlotBrushes[0][0] = signalDownBrush;
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
		private Suri.SuriBarRange[] cacheSuriBarRange;
		public Suri.SuriBarRange SuriBarRange(int days)
		{
			return SuriBarRange(Input, days);
		}

		public Suri.SuriBarRange SuriBarRange(ISeries<double> input, int days)
		{
			if (cacheSuriBarRange != null)
				for (int idx = 0; idx < cacheSuriBarRange.Length; idx++)
					if (cacheSuriBarRange[idx] != null && cacheSuriBarRange[idx].days == days && cacheSuriBarRange[idx].EqualsInput(input))
						return cacheSuriBarRange[idx];
			return CacheIndicator<Suri.SuriBarRange>(new Suri.SuriBarRange(){ days = days }, input, ref cacheSuriBarRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriBarRange SuriBarRange(int days)
		{
			return indicator.SuriBarRange(Input, days);
		}

		public Indicators.Suri.SuriBarRange SuriBarRange(ISeries<double> input , int days)
		{
			return indicator.SuriBarRange(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriBarRange SuriBarRange(int days)
		{
			return indicator.SuriBarRange(Input, days);
		}

		public Indicators.Suri.SuriBarRange SuriBarRange(ISeries<double> input , int days)
		{
			return indicator.SuriBarRange(input, days);
		}
	}
}

#endregion
