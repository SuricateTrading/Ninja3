#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Cot1 : StrategyIndicator2 {
		private CotBase cotData;
		private Sma sma;
		
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
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int days
		{ get; set; }

		/*public Cot1() {
			VendorLicense("SuricateTradingGmbH", "Basis", "https://www.suricate-trading.de/", "info@suricate-trading.de",null);
		}*/
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"CoT 1 Commercials Netto";
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
				days										= 125;
				cotData = CotBase(SuriCotReportField.CommercialNet);
				
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "COT1");
				AddLine(new Stroke(Brushes.Red, 3), 10.0, "10%");
				AddLine(Brushes.DimGray, 50.0, "50%");
				AddLine(new Stroke(Brushes.Green, 3), 90.0, "90%");
			} else if (State == State.Configure) {
				sma = Sma(days);
			}
		}
        public override string DisplayName {
			get {
				if (Instrument == null) return "COT 1";
				return "COT 1 - " + SuriStrings.instrumentToName(Instrument.FullName);
			}
        }
        public override void OnCalculateMinMax() {
	        MinValue = 0;
	        MaxValue = 100;
        }
        
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        base.OnRender(chartControl, chartScale);
	        chartScale.Properties.AutoScaleMarginType = AutoScaleMarginType.Percent;
	        chartScale.Properties.AutoScaleMarginUpper = 30;
	        chartScale.Properties.AutoScaleMarginLower = 30;
        }
        
        #endregion
        
		protected override void OnBarUpdate() {
			if (CurrentBar < days) return;

			if (Math.Abs(cotData[0] - cotData[1]) > 0.0001) {
				double min = double.MaxValue;
				double max = double.MinValue;
				for (int barsAgo = 0; barsAgo < days; barsAgo++) {
					double v = cotData.Value[barsAgo];
					if (min > v) min = v;
					if (max < v) max = v;
				}
				Value[0] = 100.0 * (cotData.Value[0] - min) / (max - min);
			} else {
				Value[0] = Value[1];
			}

			if (!isCurrentlyASignal || Value[0] < 90 && Value[0] > 10 ) {
				isCurrentlyASignal = IsSignal();
			}
			if (isCurrentlyASignal) {
				if      (sma[0] > sma[1] && IsLong())  PlotBrushes[0][0] = Brushes.Green;
				else if (sma[0] < sma[1] && IsShort()) PlotBrushes[0][0] = Brushes.Red;
				else PlotBrushes[0][0] = Brushes.Yellow;
			}
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
						if (IsLong()) lastSignal = TradePosition.Long;
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
				    sma[0] > sma[1] && IsLong() ||
				    sma[0] < sma[1] && IsShort()
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
		private Suri.Cot1[] cacheCot1;
		public Suri.Cot1 Cot1(int days)
		{
			return Cot1(Input, days);
		}

		public Suri.Cot1 Cot1(ISeries<double> input, int days)
		{
			if (cacheCot1 != null)
				for (int idx = 0; idx < cacheCot1.Length; idx++)
					if (cacheCot1[idx] != null && cacheCot1[idx].days == days && cacheCot1[idx].EqualsInput(input))
						return cacheCot1[idx];
			return CacheIndicator<Suri.Cot1>(new Suri.Cot1(){ days = days }, input, ref cacheCot1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Cot1 Cot1(int days)
		{
			return indicator.Cot1(Input, days);
		}

		public Indicators.Suri.Cot1 Cot1(ISeries<double> input , int days)
		{
			return indicator.Cot1(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Cot1 Cot1(int days)
		{
			return indicator.Cot1(Input, days);
		}

		public Indicators.Suri.Cot1 Cot1(ISeries<double> input , int days)
		{
			return indicator.Cot1(input, days);
		}
	}
}

#endregion
