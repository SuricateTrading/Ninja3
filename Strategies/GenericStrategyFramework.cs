﻿#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
#endregion

namespace NinjaTrader.NinjaScript.Strategies {
    public abstract class GenericStrategyFramework : Strategy {
	    protected readonly List<StrategyInterface> strategies = new List<StrategyInterface>();
	    private bool prepared;
        
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Generic Strategy Framework";
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
			}
		}
        
		protected override void OnBarUpdate() {
			if (BarsInProgress != 0) return;
			if (!prepared) {
				prepared = true;
				Bars.CurrentBar = Bars.Count - 1;
				foreach (var strategy in strategies) strategy.UpdateIndicators();
				Bars.CurrentBar = 0;
				foreach (var strategy in strategies) strategy.Analyze();
			}
			
			foreach (var strategy in strategies) {
				foreach (var signal in strategy.signals) {
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