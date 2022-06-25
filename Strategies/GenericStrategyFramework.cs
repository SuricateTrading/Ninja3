#region Using declarations
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
#endregion

/*
 * Limitationen:
 * - Nachkaufen, bzw. 2 gleichzeitig aktive short oder long orders funktionieren schlecht, weil es sein kann, dass eine Order beendet wird aufgrund des exits der anderen Order.
 */
namespace NinjaTrader.NinjaScript.Strategies {
    public abstract class GenericStrategyFramework : Strategy {
	    protected StrategyInterface strategy;
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
				IsUnmanaged									= false;
				// IncludeCommission = true; // https://ninjatrader.com/support/helpGuides/nt8/NT%20HelpGuide%20English.html?includecommission.htm
				// IncludeTradeHistoryInBacktest = true; // uncomment for performance boost ... https://ninjatrader.com/support/helpGuides/nt8/NT%20HelpGuide%20English.html?includetradehistoryinbacktest.htm
			}
		}
        
		protected override void OnBarUpdate() {
			if (BarsInProgress != 0) return;
			if (!prepared) {
				prepared = true;
				Bars.CurrentBar = Bars.Count - 1;
				strategy.UpdateIndicators();
				Bars.CurrentBar = 0;
				strategy.Analyze();
				
				string signals = JsonConvert.SerializeObject(strategy.signals);
				//Print(signals);
				Thread thread = new Thread(() => Clipboard.SetText(signals));
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start(); 
				thread.Join(); // Wait for the thread to end
			}
			
			foreach (var signal in strategy.signals) {
				/*if (signal.entryIndex == 961 && CurrentBar == 960) {
					Print("");
				}*/
				if (signal.orderState == OrderState.Done) continue;

				string signalName = signal.signalName;
				// entry
				if (signal.orderState == OrderState.New && CurrentBar+1 == signal.entryIndex) {
					switch (signal.orderType) {
						case OrderType.Limit:
							if (signal.isLong) 
								EnterLongLimit (0, true, signal.quantity, signal.limitPrice, signalName); else
								EnterShortLimit(0, true, signal.quantity, signal.limitPrice, signalName);
							break;
						case OrderType.Market:
							if (signal.isLong)
								EnterLong (signal.quantity, signalName); else
								EnterShort(signal.quantity, signalName);
							break;
						case OrderType.StopLimit:
							if (signal.isLong) 
								EnterLongStopLimit (0, true, signal.quantity, signal.limitPrice, signal.stopPrice, signalName); else 
								EnterShortStopLimit(0, true, signal.quantity, signal.limitPrice, signal.stopPrice, signalName); 
							break;
						case OrderType.StopMarket:
							if (signal.isLong) 
								EnterLongStopMarket (0, true, signal.quantity, signal.stopPrice, signalName); else 
								EnterShortStopMarket(0, true, signal.quantity, signal.stopPrice, signalName); 
							break;
						default: throw new ArgumentOutOfRangeException();
					}
					signal.orderState = OrderState.Filled;
				}
				
				// exit
				if (signal.orderState == OrderState.Filled && CurrentBar + 1 == signal.exitIndex) {
					SubmitOrderUnmanaged(
						0,
						signal.isLong ? OrderAction.SellShort : OrderAction.BuyToCover,
						OrderType.Market,
						1,
						0,
						0,
						null,
						signal.suriRule + " " + (signal.isLong ? "Long" : "Short") + " exit " + signal.exitReason
					);
					signal.orderState = OrderState.Done;
					continue;
				}
				
				// check if stoploss has to be traced
				if (signal.TryTraceStopAt(CurrentBar)) {
					double stoplossCurrency = SuriCommon.PriceToCurrency(Instrument, Math.Abs(signal.currentStop - Bars.GetClose(CurrentBar)));
					signal.notes += "Traced stoploss @" + CurrentBar + " to " + signal.currentStop + ". Reduced risk to " + stoplossCurrency + " $. ";
				}
				// check if stoploss will be filled tomorrow
				if (signal.orderState == OrderState.Filled && StrategyTasks.IsFilledAt(!signal.isLong, OrderType.StopMarket, CurrentBar + 1, signal.currentStop, Bars)) {
					SubmitOrderUnmanaged(
						0,
						signal.isLong ? OrderAction.SellShort : OrderAction.BuyToCover,
						OrderType.StopMarket,
						1,
						0,
						signal.currentStop,
						null,
						signal.suriRule + " " + (signal.isLong ? "Long" : "Short") + " stoploss"
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
	    public string notes = "";
	    public int quantity = 1;

	    public string signalName { get { return suriRule + " " + (isLong ? "Long" : "Short") + " @" + signalIndex; } }
		
	    public OrderType orderType;
	    /** Optional. Used for entry only. */
	    public double limitPrice;
	    /** Optional. Used for entry only. */
	    public double stopPrice;
		
	    /** The bar at which the signal was detected. */
	    public int signalIndex;
	    public DateTime signalDate;
		
	    /** The bar index where the entry occurs. */
	    public int entryIndex;
	    public DateTime entryDate;
	    /** The price at which we entered the market. May be null if we never enter, e.g. the limit order is not filled. This is just an information and not used by the strategy. */
	    public double? entry;
		
	    /** Stops may be traced, which is why we need multiple stops -> a dictionary with a bar index (int-key) and a stop (double-value).
	     * The index is the bar index where we adjust the stop. The new stop starts working from the next bar.
		 * You can change the current stop by updating currentStopBarIndex.
	     * The first (initial) stop has index -1. All other stop indices should be higher-equal than the entry index or else it is ignored.
		 */
	    public readonly SortedList<int, double> stops = new SortedList<int, double>();
	    private int currentStopBarIndex = -1;
	    public double currentStop { get { return stops[currentStopBarIndex]; } }
	    /** Set barIndex to null or omit it iff this is an initial stop. */
	    public void AddStop(double stop, int barIndex = -1) {
		    stops.Add(barIndex, stop);
	    }
	    /** Tries to trace the current stop to a new stop. Returns true iff this signal has a tracing stop at the given barIndex, else returns false. */
	    public bool TryTraceStopAt(int barIndex) {
		    if (!stops.ContainsKey(barIndex)) return false;
		    currentStopBarIndex = barIndex;
		    return true;
	    }
		
	    /** The bar index where we exit market. The order is actually sent one bar before so the exit can happen at this index. */
	    public int? exitIndex;
	    public DateTime? exitDate;
	    public string exitReason = "";

	    public string Serialize() { return JsonConvert.SerializeObject(this); }
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
