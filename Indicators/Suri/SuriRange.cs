#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
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
				Name										= "Bargröße";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				BarsRequiredToPlot							= 0;
				Periode										= 125;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
				AddPlot(new Stroke(Brushes.RoyalBlue, 2), PlotStyle.Bar, "SuriRange");
				AddPlot(new Stroke(Brushes.DarkCyan, 1), PlotStyle.Line, "Max");
			}
		}

        public override string DisplayName {
			get {
				if (Instrument == null) return "Range";
				return "Range " + Periode + " Tage - " + SuriStrings.instrumentToName(Instrument.FullName);
			}
        }
        public double Percentage() { return 100 * Values[0][0] / Values[1][0]; }
        public bool IsMegaRange() { return Math.Abs(Values[0][0] - Values[1][0]) < 0.001; }

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
		        if (CurrentBar - maxIndex > Periode) {
			        max = 0;
			        for (int i = Periode; i > 0 ; i--) {
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

        
        //todo: delete
        protected void OnBarUpdateAlt() {
	        if (CurrentBar != 0) {
		        Values[0][0] = High[0] - Low[0]; // Normalfall ohne Gap
		        if (Low[0]  > High[1]) Values[0][0] = High[0] - High[1]; // Gap nach oben
		        if (High[0] < Low[1] ) Values[0][0] = Low[1]  - Low[0];  // Gab nach unten
	        }
			
	        if (Values[0][0] > max) {
		        maxIndex = CurrentBar;
		        max = Values[0][0];
	        } else {
		        // wenn das Hoch weiter weg ist als die Periode dann neues Hoch innerhalb der Periode suchen
		        if (CurrentBar - maxIndex > Periode) {
			        max = 0;
			        for (int i = Periode; i > 0 ; i--) {
				        if (Values[0][i] > max) {
					        maxIndex = CurrentBar - i;
					        max = Values[0][i];
				        }
			        }
		        }
	        }
	        Values[1][0] = max;
        }
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
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
		private Suri.SuriRange[] cacheSuriRange;
		public Suri.SuriRange SuriRange(int periode)
		{
			return SuriRange(Input, periode);
		}

		public Suri.SuriRange SuriRange(ISeries<double> input, int periode)
		{
			if (cacheSuriRange != null)
				for (int idx = 0; idx < cacheSuriRange.Length; idx++)
					if (cacheSuriRange[idx] != null && cacheSuriRange[idx].Periode == periode && cacheSuriRange[idx].EqualsInput(input))
						return cacheSuriRange[idx];
			return CacheIndicator<Suri.SuriRange>(new Suri.SuriRange(){ Periode = periode }, input, ref cacheSuriRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriRange SuriRange(int periode)
		{
			return indicator.SuriRange(Input, periode);
		}

		public Indicators.Suri.SuriRange SuriRange(ISeries<double> input , int periode)
		{
			return indicator.SuriRange(input, periode);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriRange SuriRange(int periode)
		{
			return indicator.SuriRange(Input, periode);
		}

		public Indicators.Suri.SuriRange SuriRange(ISeries<double> input , int periode)
		{
			return indicator.SuriRange(input, periode);
		}
	}
}

#endregion
