#region Using declarations

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.SuriCommon;
using NinjaTrader.Gui;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Volumen : Indicator {
		private double max;
		private int maxIndex;
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Tage", Description = "Periode in Bars", GroupName = "Parameter")]
		public int days { get; set; }
		
		#region Colors
		[XmlIgnore]
		[Display(Name = "Volumen", Order = 0, GroupName = "Farben")]
		public Brush volumeBrush { get; set; }
		[Browsable(false)]
		public string volumeBrushSerialize {
			get { return Serialize.BrushToString(volumeBrush); }
			set { volumeBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Megavolumen", Order = 1, GroupName = "Farben")]
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
				BarsRequiredToPlot							= 0;
				signalBrush									= Brushes.Yellow;
				maxBrush									= Brushes.DarkCyan;
				volumeBrush									= Brushes.RoyalBlue;
				days										= 125;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(volumeBrush, 2), PlotStyle.Bar, "Volumen");
				AddPlot(new Stroke(maxBrush, 1), PlotStyle.Line, "Max Volumen");
			}
		}
		public override string DisplayName { get { return SuriStrings.DisplayName(Name, Instrument); } }
        public double Percentage() { return 100 * Values[0][0] / Values[1][0]; }
        public bool IsMegaVolume() { return Math.Abs(Values[0][0] - Values[1][0]) < 0.00001; }
		
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
			if(IsMegaVolume()) PlotBrushes[0][0] = signalBrush;
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
