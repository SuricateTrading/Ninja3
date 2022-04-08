#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.DirectWrite;
using Brush = System.Windows.Media.Brush;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriCot1 : Indicator {
		private SuriCot suriCotData;
		private SuriSma suriSma;
		private SessionIterator sessionIterator;
		
		private bool isCurrentlyASignal;

		#region Indicator
		[NinjaScriptProperty]
		[Browsable(false)]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int days
		{ get; set; }
		
		//[NinjaScriptProperty]
		[Browsable(false)]
		//[Display(Name="Benutze Wochen (oder Tage)", Order=1, GroupName="Parameter")]
		public bool useWeeks
		{ get; set; }
		
		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der Hauptlinie", Order=2, GroupName="Parameter")]
		public int lineWidth
		{ get; set; }
		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der sekundären Linien", Order=3, GroupName="Parameter")]
		public int lineWidthSecondary
		{ get; set; }
		
		#region Colors
		[XmlIgnore]
		[Display(Name = "Long", Order = 0, GroupName = "Farben")]
		public Brush longBrush { get; set; }
		[Browsable(false)]
		public string longBrushSerialize {
			get { return Serialize.BrushToString(longBrush); }
			set { longBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Short", Order = 1, GroupName = "Farben")]
		public Brush shortBrush { get; set; }
		[Browsable(false)]
		public string shortBrushSerialize {
			get { return Serialize.BrushToString(shortBrush); }
			set { shortBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Normale Linie", Order = 2, GroupName = "Farben")]
		public Brush regularLineBrush { get; set; }
		[Browsable(false)]
		public string regularLineBrushSerialize {
			get { return Serialize.BrushToString(regularLineBrush); }
			set { regularLineBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "50% Linie", Order = 3, GroupName = "Farben")]
		public Brush brush50Percent { get; set; }
		[Browsable(false)]
		public string brush50PercentSerialize {
			get { return Serialize.BrushToString(brush50Percent); }
			set { brush50Percent = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "SMA passt nicht", Order = 4, GroupName = "Farben")]
		public Brush noSignalBrush { get; set; }
		[Browsable(false)]
		public string noSignalBrushSerialize {
			get { return Serialize.BrushToString(noSignalBrush); }
			set { noSignalBrush = Serialize.StringToBrush(value); }
		}
		#endregion

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"CoT 1 Commercials Netto Oszillator 125 Tage";
				Name										= "CoT 1";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				
				longBrush									= Brushes.Green;
				shortBrush									= Brushes.Red;
				brush50Percent								= Brushes.DimGray;
				noSignalBrush								= Brushes.Yellow;
				regularLineBrush							= Brushes.DarkGray;
				lineWidth									= 4;
				lineWidthSecondary							= 2;
				days										= 125;
				//useWeeks									= true;
				suriCotData = SuriCot(SuriCotReportField.CommercialNet);
			} else if (State == State.Configure) {
				suriSma = SuriSma(days);
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "COT1");
				AddPlot(new Stroke(shortBrush, lineWidthSecondary), PlotStyle.Line, "10%");
				AddPlot(new Stroke(brush50Percent, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(longBrush, lineWidthSecondary), PlotStyle.Line, "90%");
			} else if (State == State.DataLoaded) {
				sessionIterator = new SessionIterator(Bars);
			}
		}
        public override string DisplayName { get { return SuriStrings.DisplayName(Name, Instrument); } }
        public override void OnCalculateMinMax() {
	        MinValue = 0;
	        MaxValue = 100;
        }
        
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        /*Print(IsInHitTest + " " + IsSelected);
	        if (IsInHitTest && IsSelected) return;*/
	        base.OnRender(chartControl, chartScale);
	        //if (IsInHitTest && !IsSelected) return;
	        
	        chartScale.Properties.AutoScaleMarginType = AutoScaleMarginType.Percent;
	        chartScale.Properties.AutoScaleMarginUpper = 30;
	        chartScale.Properties.AutoScaleMarginLower = 30;
	        
	        /*RectangleF rect = new RectangleF {
		        X = 0,
		        Y = ChartPanel.Y,
		        Width = 10000f,
		        Height = 10000f,
	        };
	        Brush b = Brushes.Black.Clone();
	        b.Opacity = 0.1;
	        RenderTarget.FillRectangle(rect, b.ToDxBrush(RenderTarget));*/
	        
	        if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
        }
        
        #endregion

        private bool hasStarted;
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None || CurrentBar <= days) return;
			
			if (!(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Day && Bars.BarsPeriod.Value == 1 || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && Bars.BarsPeriod.Value == 1440)) {
				Draw.TextFixed(this, "Warning", "CoT 1 ist nur für ein 1-Tages Chart oder 1440-Minuten Chart verfügbar.", TextPosition.Center);
				return;
			}

			if (Math.Abs(suriCotData[0] - suriCotData[1]) > 0.0001) {
				hasStarted = true;
				double min = double.MaxValue;
				double max = double.MinValue;
				
				/*if (useWeeks) {
					days = 26 * 5; // 130
				}*/
				for (int barsAgo = 0; barsAgo < days; barsAgo++) {
					double v = suriCotData.Value[barsAgo];
					if (min > v) min = v;
					if (max < v) max = v;
					
					/*if (useWeeks) {
						if (!sessionIterator.IsTradingDayDefined(Time[barsAgo])) days--;
					}*/
				}
				Value[0] = 100.0 * (suriCotData.Value[0] - min) / (max - min);
			} else if (!hasStarted) {
				return;
			} else {
				Value[0] = Value[1];
			}

			if (!isCurrentlyASignal || Value[0] < 90 && Value[0] > 10 ) {
				isCurrentlyASignal = IsSignal();
			}
			if (isCurrentlyASignal) {
				if (SuriAddOn.license != License.Basic) {
					if      (suriSma[0] > suriSma[1] && Value[0] >= 90) PlotBrushes[0][0] = longBrush;
					else if (suriSma[0] < suriSma[1] && Value[0] <= 10) PlotBrushes[0][0] = shortBrush;
					else PlotBrushes[0][0] = noSignalBrush;
				}
			}
			
			Values[1][0] = 10;
			Values[2][0] = 50;
			Values[3][0] = 90;
		}
		
		private bool IsSignal() {
			if (CurrentBar <= days) return false;
			if ( Value[0] < 90 && Value[0] > 10 ) return false;
			if ( (Value[1] > 10 && Value[0] <= 10 || Value[1] < 90 && Value[0] >= 90) == false ) return false;
			
			// check if we come from the other side
			for (int i = 2; i <= CurrentBar - days; i++) {
				if (Value[i] <= 10 && Value[i - 1] > 10 || Value[i] >= 90 && Value[i - 1] < 90) {
					return IsValidDataPoint(i) && Math.Abs(Value[i] - Value[0]) >= 80;
				}
			}
			return false;
		}

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriCot1[] cacheSuriCot1;
		public Suri.SuriCot1 SuriCot1(int days)
		{
			return SuriCot1(Input, days);
		}

		public Suri.SuriCot1 SuriCot1(ISeries<double> input, int days)
		{
			if (cacheSuriCot1 != null)
				for (int idx = 0; idx < cacheSuriCot1.Length; idx++)
					if (cacheSuriCot1[idx] != null && cacheSuriCot1[idx].days == days && cacheSuriCot1[idx].EqualsInput(input))
						return cacheSuriCot1[idx];
			return CacheIndicator<Suri.SuriCot1>(new Suri.SuriCot1(){ days = days }, input, ref cacheSuriCot1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCot1 SuriCot1(int days)
		{
			return indicator.SuriCot1(Input, days);
		}

		public Indicators.Suri.SuriCot1 SuriCot1(ISeries<double> input , int days)
		{
			return indicator.SuriCot1(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCot1 SuriCot1(int days)
		{
			return indicator.SuriCot1(Input, days);
		}

		public Indicators.Suri.SuriCot1 SuriCot1(ISeries<double> input , int days)
		{
			return indicator.SuriCot1(input, days);
		}
	}
}

#endregion
