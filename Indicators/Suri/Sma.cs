#region Using declarations

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.SuriCommon;
using NinjaTrader.Gui;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Sma : Indicator {
		private double priorSum;
		private double sum;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "SMA";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				lineWidth									= 2;
				longBrush									= Brushes.Green;
				shortBrush									= Brushes.Red;
				days										= 125;
			} else if (State == State.Configure) {
				priorSum	= 0;
				sum			= 0;
				AddPlot(new Stroke(Brushes.Yellow, lineWidth), PlotStyle.Line, "SMA");
			}
		}
		public override string DisplayName { get { return SuriStrings.DisplayName(Name, Instrument); } }
		
		protected override void OnBarUpdate() {
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported) {
				if (CurrentBar == 0) {
					Value[0] = Input[0];
				} else {
					double last = Value[1] * Math.Min(CurrentBar, days);
					if (CurrentBar >= days) {
						Value[0] = (last + Input[0] - Input[days]) / Math.Min(CurrentBar, days);
					} else {
						Value[0] = (last + Input[0]) / (Math.Min(CurrentBar, days) + 1);
					}
				}
			} else {
				if (IsFirstTickOfBar) priorSum = sum;
				sum = priorSum + Input[0] - (CurrentBar >= days ? Input[days] : 0);
				Value[0] = sum / (CurrentBar < days ? CurrentBar + 1 : days);
			}
			
			if (CurrentBar > 0) {
				if (Value[0] > Value[1]) {
					PlotBrushes[0][0] = longBrush;
				} else if (Value[0] < Value[1]) {
					PlotBrushes[0][0] = shortBrush;
				} else {
					 PlotBrushes[0][0] = PlotBrushes[0][1];
				}
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=0, GroupName="Parameter")]
		public int days
		{ get; set; }
		
		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der Linien", Order=1, GroupName="Parameter")]
		public int lineWidth
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name = "Farbe wenn steigend", Order = 2, GroupName = "Parameter")]
		public Brush longBrush { get; set; }
		[Browsable(false)]
		public string longBrushSerialize {
			get { return Serialize.BrushToString(longBrush); }
			set { longBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Farbe wenn fallend", Order = 3, GroupName = "Parameter")]
		public Brush shortBrush { get; set; }
		[Browsable(false)]
		public string shortBrushSerialize {
			get { return Serialize.BrushToString(shortBrush); }
			set { shortBrush = Serialize.StringToBrush(value); }
		}
		#endregion

	}
}









































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.Sma[] cacheSma;
		public Suri.Sma Sma(int days)
		{
			return Sma(Input, days);
		}

		public Suri.Sma Sma(ISeries<double> input, int days)
		{
			if (cacheSma != null)
				for (int idx = 0; idx < cacheSma.Length; idx++)
					if (cacheSma[idx] != null && cacheSma[idx].days == days && cacheSma[idx].EqualsInput(input))
						return cacheSma[idx];
			return CacheIndicator<Suri.Sma>(new Suri.Sma(){ days = days }, input, ref cacheSma);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Sma Sma(int days)
		{
			return indicator.Sma(Input, days);
		}

		public Indicators.Suri.Sma Sma(ISeries<double> input , int days)
		{
			return indicator.Sma(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Sma Sma(int days)
		{
			return indicator.Sma(Input, days);
		}

		public Indicators.Suri.Sma Sma(ISeries<double> input , int days)
		{
			return indicator.Sma(input, days);
		}
	}
}

#endregion
