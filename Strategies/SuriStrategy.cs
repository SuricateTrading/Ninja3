#region Using declarations
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.NinjaScript.Indicators.Suri;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;

#endregion

namespace NinjaTrader.NinjaScript.Strategies {
	public sealed class SuriTest : Strategy {
		private bool prepared;
		private SuriCot1 cot1;
		private SuriCot2 cot2;
		private SuriVolume volume;
		private SuriBarRange barRange;
		private DevTerminkurve devTerminkurve;
		
		private readonly List<SuriSignal> signals = new List<SuriSignal>();
		
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
				volume = SuriVolume(125);
				barRange = SuriBarRange(125);
				devTerminkurve = DevTerminkurve();
				AddChartIndicator(cot1);
				AddChartIndicator(cot2);
				AddChartIndicator(volume);
				AddChartIndicator(barRange);
				AddChartIndicator(devTerminkurve);
				cot1.Update();
				cot2.Update();
				volume.Update();
				barRange.Update();
				devTerminkurve.Update();
			}
		}

		private void AnalyseSignals() {
			Print("Start " + Instrument.MasterInstrument.Name);
			//AnalyzeCot1();
			//AnalyzeCot2();
			//AnalyzeTk();
			Print(JsonConvert.SerializeObject(signals));
		}

		private void AnalyzeCot1() {
			Print("Start COT 1");
			foreach (int signalIndex in cot1.signalIndices) {
				try {
					// check if valid entry
					bool isCot1Long = cot1.Value.GetValueAt(signalIndex) > 89.9;
					bool isCot2Long = cot2.seriesMain.GetValueAt(signalIndex) < cot2.series50.GetValueAt(signalIndex);
					TkState tkState = devTerminkurve.GetTkState(signalIndex);
					if (isCot1Long && !isCot2Long && tkState.IsAnyBackwardation() != true) {
						// long: wenn cot2 unter 50, dann tk egal. wenn cot2 über 50, dann tk muss in backwardation
						Print("Skip " + Bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". COT2 and TK contradict COT1 long trade.");
						continue;
					}
					if (!isCot1Long && (isCot2Long || tkState.IsAnyBackwardation() == true)) {
						// short: in contango und cot2 über 50
						Print("Skip " + Bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". COT2 or TK contradict COT1 short trade.");
						continue;
					}
					
					// calculate entry
					SuriSignal signal = new SuriSignal {
						suriRule = SuriRule.Cot1,
						isLong = isCot1Long,
						signalIndex = signalIndex,
						signalDate = Bars.GetTime(signalIndex),
						orderType = OrderType.StopMarket,
						entry = isCot1Long
							? StrategyTasks.GetWeekHigh(signalIndex, Bars) + Instrument.MasterInstrument.TickSize
							: StrategyTasks.GetWeekLow (signalIndex, Bars) - Instrument.MasterInstrument.TickSize
					};
					signal.stopPrice = signal.entry.Value;
					
					
					// check at which index the entry will be filled
					DateTime signalDate = Bars.GetTime(signalIndex);
					signal.entryIndex = StrategyTasks.GetIndexOfValueFill(signal.signalIndex, Bars, signal.entry.Value,index => (Bars.GetTime(index) - signalDate).TotalDays >= 42);
					if (signal.entryIndex == null) {
						Print("Skip " + Bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". No entry " + signal.entry + " found after 6 weeks. Hint: This check is executed before valuating the size of the stop or counter signals!");
						continue;
					}
					signal.entryDate = Bars.GetTime(signal.entryIndex.Value);
					
					
					// calculate stoploss
					signal.AddStop(signal.isLong
						? StrategyTasks.GetLast10DaysLow (signal.entryIndex.Value, Bars) - Instrument.MasterInstrument.TickSize
						: StrategyTasks.GetLast10DaysHigh(signal.entryIndex.Value, Bars) + Instrument.MasterInstrument.TickSize
					);
					double stoplossCurrency = SuriCommon.PriceToCurrency(Instrument, Math.Abs(signal.currentStop - signal.entry.Value));
					if (stoplossCurrency > 2000) {
						Print("Skip " + Bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". Stop " + stoplossCurrency + " $ too high.");
						continue;
					}
					//signal.stoplossIndex = StrategyTasks.GetIndexOfValueFill(signal.entryIndex.Value, Bars, signal.stoploss);
					//if (signal.stoplossIndex != null) signal.stoplossDate = Bars.GetTime(signal.stoplossIndex.Value);
					
					
					// exit
					string exitReason = null;
					for (int i = signal.entryIndex.Value + 1; i < Bars.Count; i++) {
						// cot1
						double value = cot1.Value.GetValueAt(i);
						if (value >= 90 && !signal.isLong || value <= 10 && signal.isLong) {
							exitReason = "COT 1 counter signal";
							signal.exitIndex = StrategyTasks.GetNextWeekIndex(i, Bars);
							break;
						}
						// cot2
						if (signal.isLong && cot2.seriesMain[i] > cot2.series50[i] || !signal.isLong && cot2.seriesMain[i] < cot2.series50[i]) {
							exitReason = "COT 2 counter signal";
							signal.exitIndex = StrategyTasks.GetNextWeekIndex(i, Bars);
							break;
						}
						// tk
						tkState = devTerminkurve.GetTkState(i);
						TkState prevTkState = devTerminkurve.GetTkState(i - 1);
						if (signal.isLong && tkState.IsBackwardationToContango(prevTkState) || !signal.isLong && tkState.IsContangoToBackwardation(prevTkState)) {
							exitReason = "TK counter signal";
							signal.exitIndex = i + 1;
							break;
						}
					}
					if (exitReason != null && signal.exitIndex != null) {
						signal.exitDate = Bars.GetTime(signal.exitIndex.Value);
						signal.exitReason = exitReason;
					}
					
					signals.Add(signal);
				} catch (Exception e) {
					Print(e.ToString());
				}
			}
		}

		private void AnalyzeCot2() {
			Print("Start COT 2");
			for (int i = 125; i < Bars.Count; i++) {
				try {
					// check if valid entry
					bool isMegaBar = barRange.IsMegaRange(i);
					bool isMegaVolume = volume.IsMegaVolume(i);
					if (!isMegaBar && !isMegaVolume) continue;
					
					SuriPosition cot2Position = SuriPosition.None;
					if (cot2.seriesMain.GetValueAt(i) <= cot2.series25.GetValueAt(i)) cot2Position = SuriPosition.Long;
					if (cot2.seriesMain.GetValueAt(i) >= cot2.series75.GetValueAt(i)) cot2Position = SuriPosition.Short;
					if (cot2Position == SuriPosition.None) continue;
					
					// tk
					TkState tkState = devTerminkurve.GetTkState(i);
					if (cot2Position == SuriPosition.Short && tkState.IsAnyBackwardation() == true) {
						Print("Skip " + Bars.GetTime(i).ToShortDateString() + " @" + i + ". COT2 short but TK was in backwardation.");
						continue;
					}


					SuriSignal signal = new SuriSignal {
						suriRule = SuriRule.Cot2,
						isLong = cot2Position == SuriPosition.Long,
						signalIndex = i,
						signalDate = Bars.GetTime(i),
						orderType = OrderType.Market,
						entryIndex = i+1
					};
					if (i + 1 < Bars.Count) {
						signal.entry     = Bars.GetOpen(i + 1);
						signal.entryDate = Bars.GetTime(i + 1);
					}


					// check iff valid cot 2 versions and calculate initial stoploss
					bool isEndOfTrend = true; // todo: check if end of trend
					SuriBarType barType = StrategyTasks.GetBarType(Bars, i, TickSize);
					if (signal.isLong && barType == SuriBarType.MegabarDown || !signal.isLong && barType == SuriBarType.MegabarUp) {
						if (!isEndOfTrend) {
							Print("Skip " + Bars.GetTime(i).ToShortDateString() + " @" + i + ". V1 not end of trend.");
							continue;
						}
						signal.notes += "V1. ";
						StrikingSpotData strikingSpotData = StrikingCalculator.FindStrikingSpot(cot2Position == SuriPosition.Short, Bars, i);
						signal.AddStop(strikingSpotData.p2Value + (signal.isLong ? -TickSize : TickSize));
					} else if (signal.isLong && barType == SuriBarType.MegabarUp || !signal.isLong && barType == SuriBarType.MegabarDown) {
						signal.notes += "V2. ";
						if (isEndOfTrend) {
							signal.AddStop(signal.isLong ? Bars.GetLow(i) - TickSize : Bars.GetHigh(i) + TickSize);
						} else {
							StrikingSpotData strikingSpotData = StrikingCalculator.FindStrikingSpot(cot2Position == SuriPosition.Short, Bars, i);
							signal.AddStop(strikingSpotData.p2Value);
						}
					} else if (signal.isLong && (barType == SuriBarType.ReversalBarTop    || barType == SuriBarType.ReversalBarMiddleTop) ||
						      !signal.isLong && (barType == SuriBarType.ReversalBarBottom || barType == SuriBarType.ReversalBarMiddleBottom)) {
						signal.notes += "V3. ";
						if (!isEndOfTrend) {
							Print("Skip " + Bars.GetTime(i).ToShortDateString() + " @" + i + ". V3 not end of trend.");
							continue;
						}
						signal.AddStop(signal.isLong ? Bars.GetLow(i) - TickSize : Bars.GetHigh(i) + TickSize);
					} else if (signal.isLong && (barType == SuriBarType.ReversalBarBottom || barType == SuriBarType.ReversalBarMiddleBottom) ||
					           !signal.isLong && (barType == SuriBarType.ReversalBarTop    || barType == SuriBarType.ReversalBarMiddleTop)) {
						// v4
						Print("Skip " + Bars.GetTime(i).ToShortDateString() + " @" + i + ". Bad reversal bar (v4).");
						continue;
					} else {
						continue;
					}

					double stoplossCurrency = SuriCommon.PriceToCurrency(Instrument, Math.Abs(signal.currentStop - Bars.GetClose(i)));
					if (stoplossCurrency > 2000) {
						Print("Skip " + Bars.GetTime(i).ToShortDateString() + " @" + i + ". Stop " + stoplossCurrency + " $ too high.");
						continue;
					}
					
					
					// exit and trace stops
					for (int j = signal.entryIndex.Value; j < Bars.Count; j++) {
						// cot2
						if (signal.isLong && cot2.seriesMain.GetValueAt(j) >= cot2.series75.GetValueAt(j) || !signal.isLong && cot2.seriesMain.GetValueAt(j) <= cot2.series25.GetValueAt(j)) {
							signal.exitIndex = j + 1;
							signal.exitDate = Bars.GetTime(j + 1);
							signal.exitReason = "COT 2 counter signal";
							break;
						}
						// tk
						tkState = devTerminkurve.GetTkState(j);
						TkState prevTkState = devTerminkurve.GetTkState(j - 1);
						if (signal.isLong && tkState.IsBackwardationToContango(prevTkState) || !signal.isLong && tkState.IsContangoToBackwardation(prevTkState)) {
							signal.exitReason = "TK counter signal";
							signal.exitIndex = j + 1;
							break;
						}
						// trace stop
						if ((signal.isLong && cot2.seriesMain.GetValueAt(j) >= cot2.series75.GetValueAt(j) ||
						    !signal.isLong && cot2.seriesMain.GetValueAt(j) <= cot2.series25.GetValueAt(j)) &&
						    barRange.IsMegaRange(j) && signal.isLong == StrategyTasks.BarGoesUp(Bars, j)
						) {
							signal.AddStop(signal.isLong ? Bars.GetLow(j + i) - TickSize : Bars.GetHigh(j + i) + TickSize, j + 1);
						}
					}

					signals.Add(signal);
				} catch (Exception e) {
					Print(e.ToString());
				}
			}
		}

		private void AnalyzeTk() {
			
		}

		protected override void OnBarUpdate() {
			if (BarsInProgress != 0) return;
			if (!prepared) {
				prepared = true;
				Bars.CurrentBar = Bars.Count - 1;
				cot1.Update();
				cot2.Update();
				volume.Update();
				barRange.Update();
				devTerminkurve.Update();
				Bars.CurrentBar = 0;
				AnalyseSignals();
			}

			foreach (var signal in signals) {
				// check entry
				if (CurrentBar+1 == signal.entryIndex) {
					SubmitOrderUnmanaged(
						0,
						signal.isLong ? OrderAction.Buy : OrderAction.Sell,
						signal.orderType,
						1,
						signal.limitPrice,
						signal.stopPrice,
						null,
						signal.suriRule + " " + (signal.isLong ? "Long" : "Short") + " @" + signal.entryIndex + ". ID" + SuriCommon.random
					);
					signal.orderState = OrderState.Filled;
				}
				
				// check exit
				if (CurrentBar + 1 == signal.exitIndex) {
					SubmitOrderUnmanaged(
						0,
						signal.isLong ? OrderAction.SellShort : OrderAction.BuyToCover,
						OrderType.Market,
						1,
						0,
						0,
						null,
						signal.suriRule + " " + (signal.isLong ? "Long" : "Short") + " exit " + signal.exitReason + " @" + signal.exitIndex + ". ID" + SuriCommon.random
					);
					signal.orderState = OrderState.Done;
				}
				
				// check if stoploss has to be traced
				if (signal.TryTraceStopAt(CurrentBar)) {
					double stoplossCurrency = SuriCommon.PriceToCurrency(Instrument, Math.Abs(signal.currentStop - Bars.GetClose(CurrentBar)));
					signal.notes += "Traced stoploss @" + CurrentBar + " to " + signal.currentStop + ". Reduced risk to " + stoplossCurrency + " $. ";
				}
				// check if stoploss will be filled
				if (signal.orderState == OrderState.Filled && StrategyTasks.IsFilledAt(CurrentBar + 1, signal.currentStop, Bars)) {
					SubmitOrderUnmanaged(
						0,
						signal.isLong ? OrderAction.SellShort : OrderAction.BuyToCover,
						OrderType.Limit,
						1,
						signal.currentStop,
						0,
						null,
						signal.suriRule + " " + (signal.isLong ? "Long" : "Short") + " stoploss @" + (CurrentBar + 1) + ". ID" + SuriCommon.random
					);
					signal.exitReason = "Stoploss";
					signal.orderState = OrderState.Done;
				}
			}
		}
		
	}

	public class SuriSignal {
		public SuriRule suriRule;
		public bool isLong;
		public OrderState orderState = OrderState.New;
		public string notes;
		
		public OrderType orderType;
		public double limitPrice;
		public double stopPrice;
		
		public int signalIndex;
		public DateTime signalDate;
		
		public int? entryIndex;
		public DateTime? entryDate;
		public double? entry;
		
		/** Stops may be traced, which is why we need multiple stops -> a dictionary with a bar index (int-key) and a stop (double-value).
		 * Change current stop by updating the field currentStopBarIndex.
		 */
		private readonly SortedList<int, double> stops = new SortedList<int, double>();
		private int currentStopBarIndex;
		public double currentStop { get { return stops[currentStopBarIndex]; } }
		/** Set barIndex to null or omit it iff this is an initial stop. */
		public void AddStop(double stop, int? barIndex = null) {
			stops.Add(barIndex ?? -1, stop);
			currentStopBarIndex = barIndex ?? -1;
		}
		/** Tries to trace the current stop to a new stop. Returns true iff this signal has a tracing stop at the given barIndex, else returns false. */
		public bool TryTraceStopAt(int barIndex) {
			if (!stops.ContainsKey(barIndex)) return false;
			currentStopBarIndex = barIndex;
			return true;
		}
		
		public int? exitIndex;
		public DateTime? exitDate;
		public string exitReason;
	}

	public enum SuriRule {
		Cot1,
		Cot2,
		Tk,
	}

	public enum OrderState {
		New,
		Filled,
		Done,
	}

	public enum SuriPosition {
		Long,
		Short,
		None,
	}

	public enum SuriBarType {
		MegabarUp,
		MegabarDown,
		ReversalBarTop,
		ReversalBarBottom,
		ReversalBarMiddleTop,
		ReversalBarMiddleBottom,
	}
	
}
