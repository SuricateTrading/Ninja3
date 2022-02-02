#region Using declarations
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Volumen : Indicator {
		private double max;
		private int maxIndex;
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int days
		{ get; set; }
		#endregion
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Zeigt das Volumen einer Bar";
				Name										= "Volumen";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				Calculate									= Calculate.OnEachTick;
				days										= 125;
				BarsRequiredToPlot							= 0;
				
				AddPlot(new Stroke(Brushes.RoyalBlue, 2), PlotStyle.Bar, "Volumen");
				AddPlot(new Stroke(Brushes.DarkCyan, 1), PlotStyle.Line, "Max Volumen");
			}
		}

        public override string DisplayName {
			get {
				if (Instrument == null) return "Volumen";
				return "Volumen " + days + " Tage - " + SuriStrings.instrumentToName(Instrument.FullName);
			}
        }
        public double Percentage() { return 100 * Values[0][0] / Values[1][0]; }
        public bool IsMegaVolume() { return Values[0][0] == Values[1][0]; }
		
		protected override void OnBarUpdate() {
			if (Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency) {
				Values[0][0] = Core.Globals.ToCryptocurrencyVolume((long)Volume[0]);
			} else {
				Values[0][0] = Volume[0];
			}
			
			if (Values[0][0] > max) {
				maxIndex = CurrentBar;
				max = Values[0][0];
			} else {
				// wenn das Hoch weiter weg ist als die Periode dann neues Hoch innerhalb der Periode suchen
				if (CurrentBar - maxIndex > days) {
					max = 0;
					for (int i=0; i<days; i++) {
						if (max < Values[0][i]) {
							maxIndex = CurrentBar - i;
							max = Values[0][i];
						}
					}
				}
			}
			Values[1][0] = max;
			if(IsMegaVolume()) PlotBrushes[0][0] = Brushes.Yellow;
		}
		
	}
}











































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.Volumen[] cacheVolumen;
		public Suri.Volumen Volumen(int days)
		{
			return Volumen(Input, days);
		}

		public Suri.Volumen Volumen(ISeries<double> input, int days)
		{
			if (cacheVolumen != null)
				for (int idx = 0; idx < cacheVolumen.Length; idx++)
					if (cacheVolumen[idx] != null && cacheVolumen[idx].days == days && cacheVolumen[idx].EqualsInput(input))
						return cacheVolumen[idx];
			return CacheIndicator<Suri.Volumen>(new Suri.Volumen(){ days = days }, input, ref cacheVolumen);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Volumen Volumen(int days)
		{
			return indicator.Volumen(Input, days);
		}

		public Indicators.Suri.Volumen Volumen(ISeries<double> input , int days)
		{
			return indicator.Volumen(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Volumen Volumen(int days)
		{
			return indicator.Volumen(Input, days);
		}

		public Indicators.Suri.Volumen Volumen(ISeries<double> input , int days)
		{
			return indicator.Volumen(input, days);
		}
	}
}

#endregion
