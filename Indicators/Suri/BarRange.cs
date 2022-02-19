#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class BarRange : Indicator {
		private double max;
		private int maxIndex;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Zeigt die Range einer Bar plus Gap zum Vortag.";
				Name										= "Bargröße";
				Calculate									= Calculate.OnEachTick;
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
				signalBrush									= Brushes.Yellow;
				maxBrush									= Brushes.DarkCyan;
				barBrush									= Brushes.RoyalBlue;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(barBrush, 2), PlotStyle.Bar, "Bargröße");
				AddPlot(new Stroke(maxBrush, 1), PlotStyle.Line, "Max");
			}
		}
		public override string DisplayName { get { return SuriStrings.DisplayName(Name, Instrument); } }
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
		[Display(Name = "Bargröße", Order = 0, GroupName = "Farben")]
		public Brush barBrush { get; set; }
		[Browsable(false)]
		public string barBrushSerialize {
			get { return Serialize.BrushToString(barBrush); }
			set { barBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Megabar", Order = 1, GroupName = "Farben")]
		public Brush signalBrush { get; set; }
		[Browsable(false)]
		public string signalBrushSerialize {
			get { return Serialize.BrushToString(signalBrush); }
			set { signalBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Max", Order = 2, GroupName = "Farben")]
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
		private Suri.BarRange[] cacheBarRange;
		public Suri.BarRange BarRange(int days)
		{
			return BarRange(Input, days);
		}

		public Suri.BarRange BarRange(ISeries<double> input, int days)
		{
			if (cacheBarRange != null)
				for (int idx = 0; idx < cacheBarRange.Length; idx++)
					if (cacheBarRange[idx] != null && cacheBarRange[idx].days == days && cacheBarRange[idx].EqualsInput(input))
						return cacheBarRange[idx];
			return CacheIndicator<Suri.BarRange>(new Suri.BarRange(){ days = days }, input, ref cacheBarRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.BarRange BarRange(int days)
		{
			return indicator.BarRange(Input, days);
		}

		public Indicators.Suri.BarRange BarRange(ISeries<double> input , int days)
		{
			return indicator.BarRange(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.BarRange BarRange(int days)
		{
			return indicator.BarRange(Input, days);
		}

		public Indicators.Suri.BarRange BarRange(ISeries<double> input , int days)
		{
			return indicator.BarRange(input, days);
		}
	}
}

#endregion
