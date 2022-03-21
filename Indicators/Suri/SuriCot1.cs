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
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.Suri;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;
using SharpDX;
using Brush = System.Windows.Media.Brush;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriCot1 : StrategyIndicator2 {
		private SuriCot suriCotData;
		private SuriSma suriSma;
		
		private bool isCurrentlyASignal;
		
		private TradePosition? lastSignal;
		public bool LastSignalWasLong()  { return lastSignal == TradePosition.Long; }
		public bool LastSignalWasShort() { return lastSignal == TradePosition.Short; }
		
		private int? lastSignalBar;
		public int? GetLastSignalBar() { return lastSignalBar; }
		
		private bool _lookOutForEntry;
		[Browsable(false)]
		public bool doEnter {
			get {
				return
					_lookOutForEntry &&
					lastSignalBar != null &&
					lastEntryValue != null &&
					(Time[0].DayOfWeek == DayOfWeek.Monday || CurrentBar - lastSignalBar >= 4) &&
					lastSignalBar != null && (Time[0] - Time[lastSignalBar.Value]).Days > 36 &&
					LastSignalWasLong() && High[0] > lastEntryValue || LastSignalWasShort() && Low[0] < lastEntryValue
				;
			}
			set { _lookOutForEntry = value; }
		}
		private double? lastEntryValue;
		public double? LastEntryValue() { return lastEntryValue; }

		#region Indicator
		[NinjaScriptProperty]
		[Browsable(false)]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int days
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
				suriCotData = SuriCot(SuriCotReportField.CommercialNet);
			} else if (State == State.Configure) {
				suriSma = SuriSma(days);
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "COT1");
				AddPlot(new Stroke(shortBrush, lineWidthSecondary), PlotStyle.Line, "10%");
				AddPlot(new Stroke(brush50Percent, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(longBrush, lineWidthSecondary), PlotStyle.Line, "90%");
				SuriServer.GetSuri(Cbi.License.MachineId);
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
        }
        
        #endregion

        private bool hasStarted;
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None) return;
			
			if (CurrentBar <= days) return;
			if (!(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Day && Bars.BarsPeriod.Value == 1 || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && Bars.BarsPeriod.Value == 1440)) {
				Draw.TextFixed(this, "Warning", "CoT 1 ist nur für ein 1-Tages Chart oder 1440-Minuten Chart verfügbar.", TextPosition.Center);
				return;
			}

			if (Math.Abs(suriCotData[0] - suriCotData[1]) > 0.0001) {
				hasStarted = true;
				double min = double.MaxValue;
				double max = double.MinValue;
				for (int barsAgo = 0; barsAgo < days; barsAgo++) {
					double v = suriCotData.Value[barsAgo];
					if (min > v) min = v;
					if (max < v) max = v;
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
					if      (suriSma[0] > suriSma[1] && IsLong())  PlotBrushes[0][0] = longBrush;
					else if (suriSma[0] < suriSma[1] && IsShort()) PlotBrushes[0][0] = shortBrush;
					else PlotBrushes[0][0] = noSignalBrush;
				}
			}
			
			Values[1][0] = 10;
			Values[2][0] = 50;
			Values[3][0] = 90;
		}
		
		

		public override bool IsLong()  { return Value[0] >= 90; }
		public override bool IsShort() { return Value[0] <= 10; }
		public override bool IsInLongHalf()  { return Value[0] >= 50; }
		public override bool IsInShortHalf() { return Value[0] <= 50; }
		public override TradePosition GetTradePosition() {
			if (Value[0] > 50) return TradePosition.Long;
			if (Value[0] < 50) return TradePosition.Short;
			return TradePosition.Middle;
		}
		
		public override bool IsSignal() {
			if (CurrentBar <= days) return false;
			if (Value[0] < 90 && Value[0] > 10) return false;
			if ( (Value[1] > 10 && Value[0] <= 10 || Value[1] < 90 && Value[0] >= 90) == false ) return false;
			
			// check if we come from the other side
			for (int i = 2; i <= CurrentBar - days; i++) {
				if (Value[i] <= 10 && Value[i - 1] > 10 || Value[i] >= 90 && Value[i - 1] < 90) {
					if (Math.Abs(Value[i] - Value[0]) >= 80) {
						if (IsShort()) lastSignal = TradePosition.Short;
						if (IsLong())  lastSignal = TradePosition.Long;
						lastSignalBar = CurrentBar;
						doEnter = true;
						return true;
					}
					return false;
				}
			}
			return false;
		}

		public override bool? IsEntry() {
			if (IsSignal() && (
				    suriSma[0] > suriSma[1] && IsLong() ||
				    suriSma[0] < suriSma[1] && IsShort()
			)) {
				SetEntryValue();
				return true;
			}
			return false;
		}
		
		public override DateTime? FirstSignalDate() {
			for (int i = 1; i <= CurrentBar - days; i++) {
				if (Value[i] > 10 && Value[i - 1] <= 10 ||
				    Value[i] < 90 && Value[i - 1] >= 90) {
					return Time[i-1];
				}
			}
			return null;
		}
		
		/** Expects to be called on tuesday, right when the signal occured.*/
		private double? SetEntryValue() {
			double max = double.MinValue;
			double min = double.MaxValue;
			for (int i = CurrentBar - 1; i < CurrentBar + 4; i++) {
				if (i >= CurrentBar && Bars.GetTime(i).DayOfWeek == DayOfWeek.Monday) {
					break; // break when the next week begins.
				}
				if (Bars.GetHigh(i) > max) max = Bars.GetHigh(i);
				if (Bars.GetLow(i)  < min) min =  Bars.GetLow(i);
			}
			if (IsLong())  return max + TickSize;
			if (IsShort()) return min - TickSize;
			return null;
		}
		
		public override double GetStopValue() {
			double max = double.MinValue;
			double min = double.MaxValue;
			for (int i = 0; i < 10; i++) {
				if (High[i] > max) max = High[i];
				if (Low[i]  < min) min =  Low[i];
			}
			return LastSignalWasLong() ? min - TickSize : max + TickSize;
		}

		public override bool ShouldExit(TradePosition tradePosition) {
			return tradePosition == TradePosition.Long && Value[0] <= 10 ||
			       tradePosition == TradePosition.Short && Value[0] >= 90;
		}
		
		
		
		
		
		/** todo: delete?
		 * Returns the value of the last given day of week.
		 */
		public double ValueOfLast(DayOfWeek dayOfWeek) {
			for (int i = 0; i < 10; i++) {
				if (Time[i].DayOfWeek == dayOfWeek) {
					return Value[i];
				}
			}
			return 0;
		}

		public override double? GetPositionStrength() {
			throw new NotImplementedException();
		}
		
    }
}


























//

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
