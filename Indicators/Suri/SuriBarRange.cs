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
		[Display(Name="Benutze Währung (an) oder Punkte (aus)", Order=0, GroupName="Parameter")]
		public bool useCurrency { get; set; }
		
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
				useCurrency									= false;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(barBrush, 2), PlotStyle.Bar, "Bargröße");
				AddPlot(new Stroke(maxBrush, 1), PlotStyle.Line, "Max");
			}
		}
		public override string DisplayName { get { return Name; } }
        public double Percentage() { return 100 * Values[0][0] / Values[1][0]; }
        public bool IsMegaBar() { return CurrentBar > days && Math.Abs(Values[0][0] - Values[1][0]) < 0.00000001; }
        public bool IsMegaBar(int barIndex) { return Math.Abs(Values[0].GetValueAt(barIndex) - Values[1].GetValueAt(barIndex)) < 0.00000001; }

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
	        if (useCurrency) Values[0][0] *= Instrument.MasterInstrument.PointValue;
			
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
	        } else if(SuriAddOn.license != License.Basic && IsMegaBar()) {
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
		public Suri.SuriBarRange SuriBarRange(bool useCurrency, int days)
		{
			return SuriBarRange(Input, useCurrency, days);
		}

		public Suri.SuriBarRange SuriBarRange(ISeries<double> input, bool useCurrency, int days)
		{
			if (cacheSuriBarRange != null)
				for (int idx = 0; idx < cacheSuriBarRange.Length; idx++)
					if (cacheSuriBarRange[idx] != null && cacheSuriBarRange[idx].useCurrency == useCurrency && cacheSuriBarRange[idx].days == days && cacheSuriBarRange[idx].EqualsInput(input))
						return cacheSuriBarRange[idx];
			return CacheIndicator<Suri.SuriBarRange>(new Suri.SuriBarRange(){ useCurrency = useCurrency, days = days }, input, ref cacheSuriBarRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriBarRange SuriBarRange(bool useCurrency, int days)
		{
			return indicator.SuriBarRange(Input, useCurrency, days);
		}

		public Indicators.Suri.SuriBarRange SuriBarRange(ISeries<double> input , bool useCurrency, int days)
		{
			return indicator.SuriBarRange(input, useCurrency, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriBarRange SuriBarRange(bool useCurrency, int days)
		{
			return indicator.SuriBarRange(Input, useCurrency, days);
		}

		public Indicators.Suri.SuriBarRange SuriBarRange(ISeries<double> input , bool useCurrency, int days)
		{
			return indicator.SuriBarRange(input, useCurrency, days);
		}
	}
}

#endregion
