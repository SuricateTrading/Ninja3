#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.Indicators.Suri;
#endregion

namespace NinjaTrader.NinjaScript.Strategies {
	public sealed class SuriTest : Strategy {
		private bool prepared;
		private SuriCot1 cot1;
		private SuriCot2 cot2;
		private SuriVolume volume;
		private SuriBarRange barRange;
		//private SuriBarRange barRange;
		//private SuriVolume volume;
		
		private readonly List<Signal> signals = new List<Signal>();
		
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
				AddChartIndicator(cot1);
				AddChartIndicator(cot2);
				AddChartIndicator(volume);
				AddChartIndicator(barRange);
				cot1.Update();
				cot2.Update();
				volume.Update();
				barRange.Update();
			}
		}

		private void AnalyseSignals() {
			Print("Start " + Instrument.MasterInstrument.Name);
			//AnalyzeCot1();
			AnalyzeCot2();
			Print(JsonConvert.SerializeObject(signals));
		}

		private void AnalyzeCot1() {
			Print("Start COT 1");
			foreach (int signalIndex in cot1.signalIndices) {
				try {
					// check if valid entry
					bool isCot1Long = cot1.Value.GetValueAt(signalIndex) > 89.9;
					bool isCot2Long = cot2.seriesMain.GetValueAt(signalIndex) < cot2.series50.GetValueAt(signalIndex);
					//bool isTkLong = true; // todo
					if (isCot1Long != isCot2Long /*&& (isCot1Long && !isCot2Long && isTkLong) == false*/) {
						Print("Skip " + Bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". COT2 or TK contradict.");
						continue;
					}
					// todo:
					// TK prüfen: muss z.B. in Richtung BW (long) / CT (short) sein
					// Richtung BW wäre dann, wenn es danach aussieht, dass demnächst der letzte Kontrakt unter den ersten springt.
					
					
					// calculate entry
					Signal signal = new Signal {
						rule = Rule.Cot1,
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
						Print("Skip " + Bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". No entry " + signal.entry + " found after 6 weeks.");
						continue;
					}
					signal.entryDate = Bars.GetTime(signal.entryIndex.Value);
					
					
					// calculate stoploss
					signal.stoploss = signal.isLong
						? StrategyTasks.GetLast10DaysLow (signal.entryIndex.Value, Bars) - Instrument.MasterInstrument.TickSize
						: StrategyTasks.GetLast10DaysHigh(signal.entryIndex.Value, Bars) + Instrument.MasterInstrument.TickSize
					;
					signal.stoplossCurrency = SuriCommon.PriceToCurrency(Instrument, Math.Abs(signal.stoploss - signal.entry.Value));
					if (signal.stoplossCurrency > 2000) {
						Print("Skip " + Bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". Stop " + signal.stoplossCurrency + " $ too high.");
						continue;
					}
					//signal.stoplossIndex = StrategyTasks.GetIndexOfValueFill(signal.entryIndex.Value, Bars, signal.stoploss);
					//if (signal.stoplossIndex != null) signal.stoplossDate = Bars.GetTime(signal.stoplossIndex.Value);
					
					
					// exit
					string exitReason = null;
					for (int i = signal.entryIndex.Value + 1; i < Bars.Count; i++) {
						// cot1
						double value = cot1.Value.GetValueAt(i);
						if (value >= 90 && !signal.isLong || value <= 10 &&  signal.isLong) {
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
						// tk ...
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
					if (cot2.seriesMain[i] < cot2.series25[i]) cot2Position = SuriPosition.Long;
					if (cot2.seriesMain[i] > cot2.series75[i]) cot2Position = SuriPosition.Short;
					if (cot2Position == SuriPosition.None) continue;

					// calculate entry
					/*StrikingCalculator strikingCalculator = new StrikingCalculator(ChartPanel.Scales[0]);
					StrikingSpotData strikingSpotData = cot2Position == SuriPosition.Long
						? strikingCalculator.FindStrikingHigh()*/

					//signals.Add(signal);
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
						signal.rule + " " + (signal.isLong ? "Long" : "Short") + " @" + signal.entryIndex + ". ID" + SuriCommon.random
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
						signal.rule + " " + (signal.isLong ? "Long" : "Short") + " exit " + signal.exitReason + " @" + signal.exitIndex + ". ID" + SuriCommon.random
					);
					signal.orderState = OrderState.Done;
				}
				// check stoploss
				if (signal.orderState == OrderState.Filled && StrategyTasks.IsFilledTomorrow(CurrentBar, signal.stoploss, Bars)) {
					SubmitOrderUnmanaged(
						0,
						signal.isLong ? OrderAction.SellShort : OrderAction.BuyToCover,
						OrderType.Limit,
						1,
						signal.stoploss,
						0,
						null,
						signal.rule + " " + (signal.isLong ? "Long" : "Short") + " stoploss @" + (CurrentBar + 1) + ". ID" + SuriCommon.random
					);
					signal.exitReason = "Stoploss";
					signal.orderState = OrderState.Done;
				}
			}
		}
		
	}

	public class Signal {
		public Rule rule;
		public bool isLong;
		public OrderState orderState = OrderState.New;
		
		public OrderType orderType;
		public double limitPrice;
		public double stopPrice;
		
		public int signalIndex;
		public DateTime signalDate;
		
		public int? entryIndex;
		public DateTime? entryDate;
		public double? entry;
		
		public double stoploss;
		public double stoplossCurrency;
		
		public int? exitIndex;
		public DateTime? exitDate;
		public string exitReason;
	}

	public enum Rule {
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
	
}
