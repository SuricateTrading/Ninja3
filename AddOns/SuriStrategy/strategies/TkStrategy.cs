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
	    
	    
	    protected override bool IsEntry(int index) {
		    TkState tkState = terminkurve.GetTkState(index);
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

		    if (!isLong && cot2.IsInLongHalf(index)) {
			    Print("Skip TK " + bars.GetTime(index).ToShortDateString() + " @" + index + ". TK Short signal, but COT2 was long.");
			    return false;
		    }
		    return true;
	    }

	    protected override SuriSignal PrepareSignal(int index) {
		    TkState tkState = terminkurve.GetTkState(index);
		    SuriSignal signal = new SuriSignal();
		    if      (tkState.IsAnyBackwardation() == true) signal.isLong = true;
		    else if (tkState.IsAnyContango()      == true) signal.isLong = false;
		    else throw new Exception("Unexpected TK State.");

		    signal.suriRule = SuriRule.Tk;
		    signal.signalIndex = index;
		    signal.signalDate = bars.GetTime(index);
		    signal.orderType = OrderType.Market;
		    signal.entryIndex = index+1;
		    signal.entryDate = bars.GetTime(index+1);
		    return signal;
	    }

	    protected override bool SetAndCheckInitialStoploss(SuriSignal signal) {
		    StrikingSpotData strikingSpotData = StrikingCalculator.FindStrikingSpot(!signal.isLong, bars, signal.signalIndex);
		    signal.AddStop(strikingSpotData.p2Value + (signal.isLong ? -tickSize : tickSize));
		    double stoplossCurrency = SuriCommon.PriceToCurrency(instrument, Math.Abs(signal.currentStop - bars.GetClose(signal.signalIndex)));
		    if (stoplossCurrency > 2000) {
			    Print("Skip TK " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". Stop " + stoplossCurrency + " $ too high.");
			    return false;
		    }
		    return true;
	    }

	    protected override void SetExit(SuriSignal signal) {
		    for (int j = signal.entryIndex.Value; j < bars.Count; j++) {
			    TkState tkState = terminkurve.GetTkState(j);
			    TkState prevTkState = terminkurve.GetTkState(j - 1);
			    if (signal.isLong && tkState.IsBackwardationToContango(prevTkState) || !signal.isLong && tkState.IsContangoToBackwardation(prevTkState)) {
				    signal.exitReason = "TK counter signal";
				    signal.exitIndex = j + 1;
				    break;
			    }
			    // trace stop
			    if ((signal.isLong || !signal.isLong && cot2.IsInLongHalf(j)) && barRange.IsMegaRange(j) && signal.isLong == StrategyTasks.BarGoesUp(bars, j)) {
				    signal.AddStop(signal.isLong ? bars.GetLow(j) - tickSize : bars.GetHigh(j) + tickSize, j + 1);
			    }
		    }
	    }

    }
}
