using System;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators.Suri;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.Custom.AddOns.SuriCommon.strategies {
    public class TkStrategy : StrategyInterface{
	    private readonly SuriCot2 cot2;
	    private readonly SuriBarRange barRange;
	    private readonly DevTerminkurve terminkurve;
	    private bool comesFromContango;
	    private bool comesFromBackwardation;
        
	    public TkStrategy(Bars bars, Instrument instrument, DevTerminkurve terminkurve, SuriCot2 cot2, SuriBarRange barRange) : base(bars, instrument) {
		    this.cot2 = cot2;
		    this.barRange = barRange;
		    this.terminkurve = terminkurve;
	    }
	    public override void UpdateIndicators() {
		    cot2.Update();
		    barRange.Update();
		    terminkurve.Update();
	    }
	    protected override string name { get { return "TK"; } }
	    protected override int startBarIndex { get { return 1; } }
	    
	    
	    protected override bool IsEntry(int signalIndex) {
		    TkState tkState = terminkurve.GetTkState(signalIndex);
		    if (!comesFromContango && !comesFromBackwardation) {
			    if (tkState == TkState.Backwardation) { comesFromContango = false; comesFromBackwardation = true ; }
			    if (tkState == TkState.Contango)      { comesFromContango = true ; comesFromBackwardation = false; }
			    return false;
		    }

		    bool isLong;
		    if      (comesFromContango && tkState.IsAnyBackwardation() == true) isLong = true;
		    else if (comesFromBackwardation && tkState.IsAnyContango() == true) isLong = false;
		    else return false;
		    comesFromContango = false;
		    comesFromBackwardation = false;

		    if (!isLong && cot2.IsInLongHalf(signalIndex)) {
			    Print("Skip TK " + bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". TK Short signal, but COT2 was long.");
			    return false;
		    }
		    return true;
	    }

	    protected override SuriSignal PrepareSignal(int signalIndex) {
		    TkState tkState = terminkurve.GetTkState(signalIndex);
		    SuriSignal signal = new SuriSignal();
		    if      (tkState.IsAnyBackwardation() == true) signal.isLong = true;
		    else if (tkState.IsAnyContango()      == true) signal.isLong = false;
		    else throw new Exception("Unexpected TK State.");

		    signal.suriRule = SuriRule.Tk;
		    signal.signalIndex = signalIndex;
		    signal.signalDate = bars.GetTime(signalIndex);
		    signal.orderType = OrderType.Market;
		    signal.entryIndex = signalIndex+1;
		    signal.entryDate = bars.GetTime(signalIndex+1);
		    return signal;
	    }

	    protected override bool SetAndCheckInitialStoploss(SuriSignal signal) {
		    /*StrikingSpotData strikingSpotData = StrikingCalculator.FindStrikingSpot(!signal.isLong, bars, signal.signalIndex);
		    signal.AddStop(strikingSpotData.p2Value + (signal.isLong ? -tickSize : tickSize));
		    double stoplossCurrency = SuriCommon.PriceToCurrency(instrument, Math.Abs(signal.currentStop - bars.GetClose(signal.signalIndex)));
		    if (stoplossCurrency > 2000) {
			    Print("Skip TK " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". Stop " + stoplossCurrency + " $ too high.");
			    return false;
		    }*/
		    double t = SuriCommon.CurrencyToPrice(instrument, 2000);
		    signal.AddStop(bars.GetClose(signal.entryIndex - 1) + (signal.isLong ? -t : t));
		    return true;
	    }

	    protected override void SetExit(SuriSignal signal) {
		    for (int i = signal.entryIndex; i < bars.Count; i++) {
			    TkState tkState = terminkurve.GetTkState(i);
			    if ( signal.isLong && (tkState == TkState.FirstHighestAndLastLowest || tkState == TkState.Contango || tkState == TkState.FirstThreeContango) ||
			        !signal.isLong && (tkState == TkState.FirstLowestAndLastHighest || tkState == TkState.Backwardation)
			    ) {
				    signal.exitReason = "TK counter signal";
				    signal.exitIndex = i + 1;
				    break;
			    }
			    // trace stop
			    if (barRange.IsMegaBar(i) && (signal.isLong || !signal.isLong && cot2.IsInLongHalf(i)) && signal.isLong == StrategyTasks.BarGoesUp(bars, i)) {
				    signal.AddStop(signal.isLong ? bars.GetLow(i) - tickSize : bars.GetHigh(i) + tickSize, i + 1);
			    }
		    }
	    }

    }
}
