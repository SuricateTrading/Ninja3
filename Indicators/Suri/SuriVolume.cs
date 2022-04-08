#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriVolume : Indicator {
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
		
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        base.OnRender(chartControl, chartScale);
	        if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
        }
        
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None) return;
			
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
			if (SuriAddOn.license == License.Premium && IsMegaVolume()) PlotBrushes[0][0] = signalBrush;
		}
	}
}











































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriVolume[] cacheSuriVolume;
		public Suri.SuriVolume SuriVolume(int days)
		{
			return SuriVolume(Input, days);
		}

		public Suri.SuriVolume SuriVolume(ISeries<double> input, int days)
		{
			if (cacheSuriVolume != null)
				for (int idx = 0; idx < cacheSuriVolume.Length; idx++)
					if (cacheSuriVolume[idx] != null && cacheSuriVolume[idx].days == days && cacheSuriVolume[idx].EqualsInput(input))
						return cacheSuriVolume[idx];
			return CacheIndicator<Suri.SuriVolume>(new Suri.SuriVolume(){ days = days }, input, ref cacheSuriVolume);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriVolume SuriVolume(int days)
		{
			return indicator.SuriVolume(Input, days);
		}

		public Indicators.Suri.SuriVolume SuriVolume(ISeries<double> input , int days)
		{
			return indicator.SuriVolume(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriVolume SuriVolume(int days)
		{
			return indicator.SuriVolume(Input, days);
		}

		public Indicators.Suri.SuriVolume SuriVolume(ISeries<double> input , int days)
		{
			return indicator.SuriVolume(input, days);
		}
	}
}

#endregion
