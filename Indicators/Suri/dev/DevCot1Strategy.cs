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

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public sealed class DevCot1Strategy : Indicator {
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
				//suriCot1 = SuriCot1(days);
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

        public bool IsLong()  { return Value[0] >= 90; }
        public bool IsShort() { return Value[0] <= 10; }
        public bool IsInLongHalf()  { return Value[0] >= 50; }
        public bool IsInShortHalf() { return Value[0] <= 50; }
        public TradePosition GetTradePosition() {
	        if (Value[0] > 50) return TradePosition.Long;
	        if (Value[0] < 50) return TradePosition.Short;
	        return TradePosition.Middle;
        }
        
        public void Clean() {
	        entry = null;
	        signalDate = null;
	        stop = null;
	        if (suriTest.cot1SuriOrder != null) suriTest.cot1SuriOrder.Exit();
	        suriTest.cot1SuriOrder = null;
        }
        
		protected override void OnBarUpdate() {
			if (CurrentBar <= days) return;
			suriCot1.Update();
			
			// Check if signal has expired and clean it up.
			if (signalDate != null && suriTest.cot1SuriOrder != null && suriTest.cot1SuriOrder.order.OrderState != OrderState.Filled && (Time[0] - signalDate.Value.AddDays(42)).Days > 0) {
				suriTest.cot1SuriOrder.Cancel();
				Clean();
			}
			
			// check if report was released today
			if (SuriCotStrategy.releaseToReportDates.ContainsKey(Time[0].ToString("yyyy-MM-dd"))) {
				DateTime reportDate = DateTime.Parse(SuriCotStrategy.releaseToReportDates[Time[0].ToString("yyyy-MM-dd")]);

				bool found = false;
				for (int barsAgo = 0; barsAgo < 20; barsAgo++) {
					if (Time[barsAgo].Date == reportDate.Date) {
						found = true;
						Value[0] = suriCot1[barsAgo];
						if (Value[0] >= 90 && Value[1] < 90 || Value[0] <= 10 && Value[1] > 10) {
							PlotBrushes[0][0] = suriCot1.PlotBrushes[0][barsAgo];
							if (PlotBrushes[0][0] == Brushes.Yellow) {
								if (suriTest.cot1SuriOrder != null) suriTest.cot1SuriOrder.Cancel();
								Clean();
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
			
			// set stop if we reached entry
			if (suriTest.cot1SuriOrder != null && suriTest.cot1SuriOrder.order.OrderState == OrderState.Filled && suriTest.cot1SuriOrder.stopOrder == null) {
				SetStop();
			}
			
			Values[1][0] = 10;
			Values[2][0] = 50;
			Values[3][0] = 90;
		}
		

		/** Expects to be called on the bar of the cot release date. BarsAgo should be the bar of the cot report date. */
		private void SetEntry(int barsAgo) {
			if (suriTest.cot1SuriOrder != null) suriTest.cot1SuriOrder.Cancel();
			Clean();
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
				string signalName = "COT1 Long " + SuriCommon.random;
				suriTest.cot1SuriOrder = new SuriOrder(signalName, OrderAction.Buy, OrderType.StopMarket, entry.Value, entry.Value);
			}
			if (IsShort()) {
				entry = min - TickSize;
				string signalName = "COT1 Short " + SuriCommon.random;
				suriTest.cot1SuriOrder = new SuriOrder(signalName, OrderAction.Sell, OrderType.StopMarket, entry.Value, entry.Value);
			}
			
			Draw.Line(this, "Entry " + SuriCommon.random, false, 4, entry.Value, 0, entry.Value, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
			int yOffset = suriTest.cot1SuriOrder != null && suriTest.cot1SuriOrder.IsLong() ? 15 : -15;
			SuriCommon.DrawText(this, "Entry ", "Entry @" + entry.Value, 2, entry.Value, yOffset);
		}
		
		private void SetStop() {
			double max = double.MinValue;
			double min = double.MaxValue;
			for (int i = 0; i < 10; i++) {
				if (High[i] > max) max = High[i];
				if (Low[i]  < min) min =  Low[i];
			}
			
			if (suriTest.cot1SuriOrder.IsLong())  stop = min - TickSize;
			if (suriTest.cot1SuriOrder.IsShort()) stop = max + TickSize;
			
			if (SuriCommon.PriceToCurrency(Instrument, Math.Abs(entry.Value - stop.Value)) > 2000) {
				SuriCommon.DrawText(this, "Stop zu groß ", "X", 0, stop.Value, 0);
			} else {
				suriTest.cot1SuriOrder.SetStopLoss(stop.Value);
			}
			
			Draw.Line(this, "Stop " + SuriCommon.random, false, 9, stop.Value, 0, stop.Value, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
			int offset = suriTest.cot1SuriOrder.IsLong() ? -15 : 15;
			SuriCommon.DrawText(this, "Stop ", "Stop @" + stop.Value + " (" + SuriCommon.PriceToCurrency(Instrument, entry.Value - stop.Value) + " $)", 5, stop.Value, offset);
		}

    }
}

public enum StrategyState {
	None,
	Limit,
	Filled,
}

























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.DevCot1Strategy[] cacheDevCot1Strategy;
		public Suri.dev.DevCot1Strategy DevCot1Strategy(int days, SuriTest suriTest)
		{
			return DevCot1Strategy(Input, days, suriTest);
		}

		public Suri.dev.DevCot1Strategy DevCot1Strategy(ISeries<double> input, int days, SuriTest suriTest)
		{
			if (cacheDevCot1Strategy != null)
				for (int idx = 0; idx < cacheDevCot1Strategy.Length; idx++)
					if (cacheDevCot1Strategy[idx] != null && cacheDevCot1Strategy[idx].days == days && cacheDevCot1Strategy[idx].suriTest == suriTest && cacheDevCot1Strategy[idx].EqualsInput(input))
						return cacheDevCot1Strategy[idx];
			return CacheIndicator<Suri.dev.DevCot1Strategy>(new Suri.dev.DevCot1Strategy(){ days = days, suriTest = suriTest }, input, ref cacheDevCot1Strategy);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevCot1Strategy DevCot1Strategy(int days, SuriTest suriTest)
		{
			return indicator.DevCot1Strategy(Input, days, suriTest);
		}

		public Indicators.Suri.dev.DevCot1Strategy DevCot1Strategy(ISeries<double> input , int days, SuriTest suriTest)
		{
			return indicator.DevCot1Strategy(input, days, suriTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevCot1Strategy DevCot1Strategy(int days, SuriTest suriTest)
		{
			return indicator.DevCot1Strategy(Input, days, suriTest);
		}

		public Indicators.Suri.dev.DevCot1Strategy DevCot1Strategy(ISeries<double> input , int days, SuriTest suriTest)
		{
			return indicator.DevCot1Strategy(input, days, suriTest);
		}
	}
}

#endregion
