#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies;
using Brush = System.Windows.Media.Brush;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriCot1Strategy : Indicator {
		#region Indicator
		[NinjaScriptProperty] [Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int days { get; set; }
		[NinjaScriptProperty] [Display(Name="SuriTest", Order=2, GroupName="Parameter")]
		public SuriTest suriTest { get; set; }
		private int lineWidth { get; set; }
		private int lineWidthSecondary { get; set; }
		private Brush longBrush { get; set; }
		private Brush shortBrush { get; set; }
		private Brush regularLineBrush { get; set; }
		private Brush brush50Percent { get; set; }
		private Brush noSignalBrush { get; set; }

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"CoT 1 Commercials Netto Oszillator 125 Tage";
				Name										= "CoT 1 Strategy";
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
			} else if (State == State.Configure) {
				suriCot1 = SuriCot1(days);
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "COT1");
				AddPlot(new Stroke(shortBrush, lineWidthSecondary), PlotStyle.Line, "10%");
				AddPlot(new Stroke(brush50Percent, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(longBrush, lineWidthSecondary), PlotStyle.Line, "90%");
				SuriServer.GetSuri(Cbi.License.MachineId);
			}
		}
        public override string DisplayName { get { return SuriStrings.DisplayName(Name, Instrument); } }
        public override void OnCalculateMinMax() { MinValue = 0; MaxValue = 100; }
        #endregion
        
        public SuriCot1 suriCot1;
        /** After there was a trading signal, we try to enter at this price. Null iff there was no signal the last 6 weeks. */
        public double? entry;
        /** The Date when the last signal occured. Null iff there was no signal the last 6 weeks. */
        public DateTime? signalDate;
        /** The stop of the last signal. Null iff there was no signal the last 6 weeks. */
        public double? stop;
        public TradePosition? signalTradePosition;
        /** Iff true, immediately exit the trade. Is set to true by the indicator and set to false by the strategy after the exit has been done. */
        public bool mustExit;
        /** Iff true, COT1 wants to enter. No other rules have been taken into consideration. Is set to true by the indicator and set to false by the strategy after the order has been executed. */
        public bool waitingToEnter;
        /** Set to true by the order. */
        public bool orderHasBeenFilled;
        public Order order;

        /**  */
        public void Reset() {
	        entry = null;
	        signalDate = null;
	        stop = null;
	        signalTradePosition = null;
	        mustExit = true;
	        waitingToEnter = false;
	        orderHasBeenFilled = false;
        }
        
		protected override void OnBarUpdate() {
			if (CurrentBar <= days) return;
			suriCot1.Update();
			
			// Check if signal has expired and clean it up.
			if (signalDate != null && !orderHasBeenFilled && (Time[0] - signalDate.Value.AddDays(42)).Days > 0) {
				Reset();
			}
			
			// set stop if we reached entry
			// todo: man setzt ja eine limit order. d.h. man muss schon am gleichen tag rein...
			if (stop == null && (signalTradePosition == TradePosition.Long && High[0] > entry.Value || signalTradePosition == TradePosition.Short && Low[0] < entry.Value)) {
				SetStop();
			}
			
			if (SuriCotStrategy.releaseToReportDates.ContainsKey(Time[0].ToString("yyyy-MM-dd"))) {
				// cot report was released today
				DateTime reportDate = DateTime.Parse(SuriCotStrategy.releaseToReportDates[Time[0].ToString("yyyy-MM-dd")]);

				bool found = false;
				for (int barsAgo = 0; barsAgo < 20; barsAgo++) {
					if (Time[barsAgo].Date == reportDate.Date) {
						found = true;
						Value[0] = suriCot1[barsAgo];
						if (Value[0] >= 90 && Value[1] < 90 || Value[0] <= 10 && Value[1] > 10) {
							PlotBrushes[0][0] = suriCot1.PlotBrushes[0][barsAgo];
							if (PlotBrushes[0][0] == Brushes.Yellow) {
							} else if (PlotBrushes[0][0] == Brushes.Green) {
								SetEntry(barsAgo);
							} else if (PlotBrushes[0][0] == Brushes.Red) {
								SetEntry(barsAgo);
							} else if (PlotBrushes[0][0] == Brushes.DarkGray || PlotBrushes[0][0] == null) {
								// nothing to do
							} else {
								Print("Error: Der COT 1 Zustand konnte nicht ermittelt werden. " + Time[0] + " " + PlotBrushes[0][0]);
							}
						}
						break;
					}
				}
				if (!found) Print("Error: Es wurde kein Handelstag für das COT-Report-Datum gefunden " + reportDate);
			} else {
				Values[0][0] = Values[0][1];
			}
			
			if (entry != null && (signalTradePosition == TradePosition.Long && Value[0] <= 10 || signalTradePosition == TradePosition.Short && Value[0] >= 90)) {
				mustExit = true;
			}
			
			Values[1][0] = 10;
			Values[2][0] = 50;
			Values[3][0] = 90;
		}
		
		public bool IsLong()  { return Value[0] >= 90; }
		public bool IsShort() { return Value[0] <= 10; }
		public bool IsInLongHalf()  { return Value[0] >= 50; }
		public bool IsInShortHalf() { return Value[0] <= 50; }
		public TradePosition GetTradePosition() {
			if (Value[0] > 50) return TradePosition.Long;
			if (Value[0] < 50) return TradePosition.Short;
			return TradePosition.Middle;
		}
		
		
		/** Expects to be called on the bar of the cot release date. BarsAgo should be the bar of the cot report date. */
		private void SetEntry(int barsAgo) {
			waitingToEnter = true;
			signalDate = Time[barsAgo];
			
			double max = double.MinValue;
			double min = double.MaxValue;
			
			int i = barsAgo;
			// check before barsago
			while (Time[i].DayOfWeek < Time[i-1].DayOfWeek) {
				if (High[i] > max) max = High[i];
				if (Low[i]  < min) min =  Low[i];
				i++;
			}
			i = barsAgo;
			// check after barsago
			while (i >= 0 && Time[i].DayOfWeek > Time[i+1].DayOfWeek) {
				if (High[i] > max) max = High[i];
				if (Low[i]  < min) min =  Low[i];
				i--;
			}
			
			if (IsLong()) {
				entry = max + TickSize;
				signalTradePosition = TradePosition.Long;
			}
			if (IsShort()) {
				entry = min - TickSize;
				signalTradePosition = TradePosition.Short;
			}
			
			Draw.Line(this, "Entry " + SuriCommon.random, false, 4, entry.Value, 0, entry.Value, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
			int yOffset = signalTradePosition == TradePosition.Long ? 15 : -15;
			SuriCommon.DrawText(this, "Entry ", "Entry @" + entry.Value, 2, entry.Value, yOffset);
		}
		
		private void SetStop() {
			double max = double.MinValue;
			double min = double.MaxValue;
			for (int i = 0; i < 10; i++) {
				if (High[i] > max) max = High[i];
				if (Low[i]  < min) min =  Low[i];
			}
			
			if (signalTradePosition == TradePosition.Long)  stop = min - TickSize;
			if (signalTradePosition == TradePosition.Short) stop = max + TickSize;
			
			Draw.Line(this, "Stop " + SuriCommon.random, false, 9, stop.Value, 0, stop.Value, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
			int offset = signalTradePosition == TradePosition.Long ? -15 : 15;
			SuriCommon.DrawText(this, "Stop ", "Stop @" + stop.Value + " (" + SuriCommon.PriceToCurrency(Instrument, entry.Value - stop.Value) + " $)", 5, stop.Value, offset);
		}

    }
}


























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriCot1Strategy[] cacheSuriCot1Strategy;
		public Suri.SuriCot1Strategy SuriCot1Strategy(int days, SuriTest suriTest)
		{
			return SuriCot1Strategy(Input, days, suriTest);
		}

		public Suri.SuriCot1Strategy SuriCot1Strategy(ISeries<double> input, int days, SuriTest suriTest)
		{
			if (cacheSuriCot1Strategy != null)
				for (int idx = 0; idx < cacheSuriCot1Strategy.Length; idx++)
					if (cacheSuriCot1Strategy[idx] != null && cacheSuriCot1Strategy[idx].days == days && cacheSuriCot1Strategy[idx].suriTest == suriTest && cacheSuriCot1Strategy[idx].EqualsInput(input))
						return cacheSuriCot1Strategy[idx];
			return CacheIndicator<Suri.SuriCot1Strategy>(new Suri.SuriCot1Strategy(){ days = days, suriTest = suriTest }, input, ref cacheSuriCot1Strategy);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCot1Strategy SuriCot1Strategy(int days, SuriTest suriTest)
		{
			return indicator.SuriCot1Strategy(Input, days, suriTest);
		}

		public Indicators.Suri.SuriCot1Strategy SuriCot1Strategy(ISeries<double> input , int days, SuriTest suriTest)
		{
			return indicator.SuriCot1Strategy(input, days, suriTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCot1Strategy SuriCot1Strategy(int days, SuriTest suriTest)
		{
			return indicator.SuriCot1Strategy(Input, days, suriTest);
		}

		public Indicators.Suri.SuriCot1Strategy SuriCot1Strategy(ISeries<double> input , int days, SuriTest suriTest)
		{
			return indicator.SuriCot1Strategy(input, days, suriTest);
		}
	}
}

#endregion
