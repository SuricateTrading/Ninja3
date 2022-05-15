#region Using declarations
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.NinjaScript.Indicators.Suri;
#endregion

namespace NinjaTrader.NinjaScript.Strategies {
	public sealed class SuriTest : Strategy {
		private bool prepared;
		private SuriCot1 cot1;
		private SuriCot2 cot2;
		//private SuriBarRange barRange;
		//private SuriVolume volume;
		
		private readonly Dictionary<int, Signal> cot1Signals = new Dictionary<int, Signal>();
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "SuriTest";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
				OrderFillResolution							= OrderFillResolution.Standard;
				//OrderFillResolutionType					= BarsPeriodType.Tick;
				//OrderFillResolutionValue					= 1;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 0;
				IsInstantiatedOnEachOptimizationIteration	= true;
				IsExitOnSessionCloseStrategy				= false;
				IsUnmanaged									= true;
				//IncludeCommission = true; // https://ninjatrader.com/support/helpGuides/nt8/NT%20HelpGuide%20English.html?includecommission.htm
				//IncludeTradeHistoryInBacktest = true; // uncomment for performance boost ... https://ninjatrader.com/support/helpGuides/nt8/NT%20HelpGuide%20English.html?includetradehistoryinbacktest.htm
			} else if (State == State.DataLoaded) {
				cot1 = SuriCot1();
				cot2 = SuriCot2();
				AddChartIndicator(cot1);
				AddChartIndicator(cot2);
				cot1.Update();
				cot2.Update();
			}
		}

		private void AnalyseSignals() {
			AnalyzeCot1();
			Print(JsonConvert.SerializeObject(cot1Signals));
		}

		private void AnalyzeCot1() {
			// COT 1
			foreach (int signalIndex in cot1.signalIndices) {
				try {
					// check if valid entry
					bool isCot1Long = cot1.Value.GetValueAt(signalIndex) > 89.9;
					bool isCot2Long = cot2.Values[3].GetValueAt(signalIndex) < cot2.Values[1].GetValueAt(signalIndex);
					bool isTkLong = true; // todo
					if (isCot1Long != isCot2Long && (isCot1Long && !isCot2Long && isTkLong) == false) {
						Print("Skip " + Bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". COT2 or TK contradict.");
						continue;
					}
					// todo:
					// TK prüfen: muss z.B. in Richtung BW (long) / CT (short) sein
					// Richtung BW wäre dann, wenn es danach aussieht, dass demnächst der letzte Kontrakt unter den ersten springt.
				
					// calculate entry
					Signal signal = new Signal {
						isLong = isCot1Long,
						signalIndex = signalIndex,
						signalDate = Bars.GetTime(signalIndex),
						entry = isCot1Long
							? StrategyTasks.GetWeekHigh(signalIndex, Bars) + Instrument.MasterInstrument.TickSize
							: StrategyTasks.GetWeekLow (signalIndex, Bars) - Instrument.MasterInstrument.TickSize
					};
				
					// check at which index the entry will be filled
					DateTime signalDate = Bars.GetTime(signalIndex);
					signal.entryIndex = StrategyTasks.GetIndexOfValueFill(signal.signalIndex, Bars, signal.entry,index => (Bars.GetTime(index) - signalDate).TotalDays >= 42);
					if (signal.entryIndex == null) {
						Print("Skip " + Bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". No entry " + signal.entry + " found after 6 weeks.");
						continue;
					}
				
					// calculate stop
					signal.stoploss = signal.isLong
							? StrategyTasks.GetLast10DaysLow (signalIndex, Bars) - Instrument.MasterInstrument.TickSize
							: StrategyTasks.GetLast10DaysHigh(signalIndex, Bars) + Instrument.MasterInstrument.TickSize
						;
					signal.stoplossCurrency = SuriCommon.PriceToCurrency(Instrument, Math.Abs(signal.stoploss - signal.entry));
					if (signal.stoplossCurrency > 2000) {
						Print("Skip " + Bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". Stop " + signal.stoplossCurrency + " $ too high.");
						continue;
					}
					signal.stoplossIndex = StrategyTasks.GetIndexOfValueFill(signal.entryIndex.Value, Bars, signal.stoploss);
				
					cot1Signals.Add(signalIndex, signal);
				} catch (Exception e) {
					Print(e.ToString());
				}
			}
		}

		protected override void OnBarUpdate() {
			if (BarsInProgress != 0) return;
			if (!prepared) {
				prepared = true;
				Bars.CurrentBar = Bars.Count - 1;
				cot1.Update();
				cot2.Update();
				Bars.CurrentBar = 0;
				AnalyseSignals();
			}
		}

	}

	public class Signal {
		public bool isLong;
		public int signalIndex;
		public DateTime signalDate;
		public int? entryIndex;
		public double entry;
		public double stoploss;
		public int? stoplossIndex;
		public double stoplossCurrency;
		public int exitIndex;
	}
	
}

/*


		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string comment) {
			base.OnOrderUpdate(order, limitPrice, stopPrice, quantity, filled, averageFillPrice, orderState, time, error, comment);
			// todo:
			// Assign entryOrder in OnOrderUpdate() to ensure the assignment occurs when expected.
			// This is more reliable than assigning Order objects in OnBarUpdate, as the assignment is not guaranteed to be complete if it is referenced immediately after submitting
		}
		
		/// OnExecution is called for each order being filled. If for example a Stop Loss Order is never touched, then this method is not called for this order.
		/// Hence we use this method for *ONLY 2 THINGS* : to know when we went market in and out.
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time) {
			base.OnExecutionUpdate(execution, executionId, price, quantity, marketPosition, orderId, time);

			String orderName = execution.Order.FromEntrySignal.IsNullOrEmpty() ? execution.Order.Name : execution.Order.FromEntrySignal + " " + execution.Order.Name;
			//Print(Time[0] + " " + orderName);

			if (orderName.Contains("Cot 1")) {
				if (execution.IsEntryStrategy && suriCot1.LastEntryValue() != null) {
					// set stop loss order and Draw 10-Bars Rectangle To draw entry and stop loss value
					double? stop = suriCot1.GetStopValue();
					if (stop != null && execution.Order == cot1Order) {
						//SDraw.RectangleS(this, "Marketbox " + random, 8, cot1EntryValue.Value, -1, stop.Value, Brushes.Blue, true);
						//Draw.Text(this, "Entry " + random, "Entry", 3, cot1EntryValue.Value, ChartControl.Properties.ChartText);

						double stopLossDollar = PriceToCurrency(Math.Abs(stop.Value - suriCot1.LastEntryValue().Value));
						Draw.Line(this, "Stop " + random, false, 8, stop.Value, -1, stop.Value, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
						int offset = suriCot1.IsInLongHalf() ? -15 : 15;
						DrawText("Stop ", "Stop @" + stop.Value + " (" + stopLossDollar + " $)", 4, stop.Value, offset);
						
						SetStopLoss(suriCot1.GetEntryName(), CalculationMode.Price, stop.Value, true);
					}
				}
				if (execution.IsExitStrategy) {
					// Just draws the win/loss when an exit occured.
					Trade lastTrade = SystemPerformance.AllTrades[SystemPerformance.AllTrades.TradesCount-1];
					String text = lastTrade.ProfitCurrency < 0 ? "Verlust" : "Profit";
					double s = Bars.GetLow(CurrentBar + 1);
					Draw.Line(this, "Stop " + random, false, 0, s, -2, s, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
					DrawText("Exit ", text + ":\n"+ lastTrade.ProfitCurrency.ToString("F0") + "$", -1, s, -22);
					cot1Order = null;
				}
			}

			if (orderName.Contains("Cot 2")) {

			}

		}

*/
