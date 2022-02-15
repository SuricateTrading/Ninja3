#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class SuriRange : Indicator {
		private double max;
		private int maxIndex;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Zeigt die Range einer Bar plus Gap zum Vortag.";
				Name										= "Bargröße (Megarange)";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				BarsRequiredToPlot							= 0;
				days										= 125;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				signalBrush									= Brushes.Yellow;
				maxBrush									= Brushes.DarkCyan;
				volumeBrush									= Brushes.RoyalBlue;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(volumeBrush, 2), PlotStyle.Bar, "Megarange");
				AddPlot(new Stroke(maxBrush, 1), PlotStyle.Line, "Max");
			}
		}

        public override string DisplayName {
			get {
				if (Instrument == null) return "Bargröße";
				return "Bargröße " + days + " Tage - " + SuriStrings.instrumentToName(Instrument.FullName);
			}
        }
        public double Percentage() { return 100 * Values[0][0] / Values[1][0]; }
        public bool IsMegaRange() { return CurrentBar > days && Math.Abs(Values[0][0] - Values[1][0]) < 0.001; }

        protected override void OnBarUpdate() {
	        if (CurrentBar != 0) {
		        Values[0][0] = Math.Max(Close[1], High[0]) - Math.Min(Close[1], Low[0]);
	        } else {
		        Values[0][0] = Close[0] - High[0];
	        }
			
	        if (Values[0][0] > max) {
		        maxIndex = CurrentBar;
		        max = Values[0][0];
	        } else {
		        // wenn das Hoch weiter weg ist als die Periode dann neues Hoch innerhalb der Periode suchen
		        if (CurrentBar - maxIndex > days) {
			        max = 0;
			        for (int i = days; i > 0 ; i--) {
				        if (Values[0][i] > max) {
					        maxIndex = CurrentBar - i;
					        max = Values[0][i];
				        }
			        }
		        }
	        }
	        Values[1][0] = max;
	        if(IsMegaRange()) PlotBrushes[0][0] = Brushes.Yellow;
        }
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Tage", Description = "Periode in Bars", GroupName = "Parameter")]
		public int days { get; set; }
		
		#region Colors
		[XmlIgnore]
		[Display(Name = "Signal", Order = 2, GroupName = "Farben")]
		public Brush signalBrush { get; set; }
		[Browsable(false)]
		public string signalBrushSerialize {
			get { return Serialize.BrushToString(signalBrush); }
			set { signalBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Volumen", Order = 0, GroupName = "Farben")]
		public Brush volumeBrush { get; set; }
		[Browsable(false)]
		public string volumeBrushSerialize {
			get { return Serialize.BrushToString(volumeBrush); }
			set { volumeBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Max", Order = 0, GroupName = "Farben")]
		public Brush maxBrush { get; set; }
		[Browsable(false)]
		public string maxBrushSerialize {
			get { return Serialize.BrushToString(maxBrush); }
			set { maxBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		#endregion
		
	}
}






























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriRange[] cacheSuriRange;
		public Suri.SuriRange SuriRange(int days)
		{
			return SuriRange(Input, days);
		}

		public Suri.SuriRange SuriRange(ISeries<double> input, int days)
		{
			if (cacheSuriRange != null)
				for (int idx = 0; idx < cacheSuriRange.Length; idx++)
					if (cacheSuriRange[idx] != null && cacheSuriRange[idx].days == days && cacheSuriRange[idx].EqualsInput(input))
						return cacheSuriRange[idx];
			return CacheIndicator<Suri.SuriRange>(new Suri.SuriRange(){ days = days }, input, ref cacheSuriRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriRange SuriRange(int days)
		{
			return indicator.SuriRange(Input, days);
		}

		public Indicators.Suri.SuriRange SuriRange(ISeries<double> input , int days)
		{
			return indicator.SuriRange(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriRange SuriRange(int days)
		{
			return indicator.SuriRange(Input, days);
		}

		public Indicators.Suri.SuriRange SuriRange(ISeries<double> input , int days)
		{
			return indicator.SuriRange(input, days);
		}
	}
}

#endregion
