#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.NinjaScript;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriMega : Indicator {
		private SuriVolume volume;
		private SuriBarRange range;
		
		#region Properties
		[NinjaScriptProperty]
		[Browsable(false)]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=0, GroupName="Parameter")]
		public int days { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Bar Farbe", Order = 1, GroupName = "Parameter")]
		public Brush barBrush { get; set; }
		[Browsable(false)]
		public string barBrushSerialize {
			get { return Serialize.BrushToString(barBrush); }
			set { barBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Mega Farbe", Order = 2, GroupName = "Parameter")]
		public Brush signalBrush { get; set; }
		[Browsable(false)]
		public string signalBrushSerialize {
			get { return Serialize.BrushToString(signalBrush); }
			set { signalBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Mega";
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
				barBrush									= Brushes.RoyalBlue;
				signalBrush									= Brushes.Yellow;
				days										= 125;
			} else if (State == State.Configure) {
				volume = SuriVolume(days);
				range = SuriBarRange(days);
				AddPlot(new Stroke(barBrush, 2), PlotStyle.Bar, "Mega");
				AddPlot(new Stroke(barBrush, 0), PlotStyle.Bar, "Range");
				AddPlot(new Stroke(barBrush, 0), PlotStyle.Bar, "Volumen");
			}
		}

		public override void OnCalculateMinMax() {
			MinValue = 0;
			MaxValue = 100;
		}
		public override string DisplayName { get { return SuriStrings.DisplayName(Name, Instrument); } }
		
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None) return;
			
			Values[1][0] = 100 * range.Values[0][0] / range.Values[1][0];
			Values[2][0] = 100 * volume.Values[0][0] / volume.Values[1][0];
			Value[0] = Math.Max(Values[1][0], Values[2][0]);
			
			if(SuriAddOn.license != License.Basic && Value[0] > 99.99) PlotBrushes[0][0] = signalBrush;
		}
		
	}
}











































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriMega[] cacheSuriMega;
		public Suri.SuriMega SuriMega(int days)
		{
			return SuriMega(Input, days);
		}

		public Suri.SuriMega SuriMega(ISeries<double> input, int days)
		{
			if (cacheSuriMega != null)
				for (int idx = 0; idx < cacheSuriMega.Length; idx++)
					if (cacheSuriMega[idx] != null && cacheSuriMega[idx].days == days && cacheSuriMega[idx].EqualsInput(input))
						return cacheSuriMega[idx];
			return CacheIndicator<Suri.SuriMega>(new Suri.SuriMega(){ days = days }, input, ref cacheSuriMega);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriMega SuriMega(int days)
		{
			return indicator.SuriMega(Input, days);
		}

		public Indicators.Suri.SuriMega SuriMega(ISeries<double> input , int days)
		{
			return indicator.SuriMega(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriMega SuriMega(int days)
		{
			return indicator.SuriMega(Input, days);
		}

		public Indicators.Suri.SuriMega SuriMega(ISeries<double> input , int days)
		{
			return indicator.SuriMega(input, days);
		}
	}
}

#endregion
