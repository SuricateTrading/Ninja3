using System;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators.Suri;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.Custom.AddOns.SuriCommon.strategies {
    public class MyTkStrategy : StrategyInterface {
	    private readonly SuriBarRange barRange;
	    private readonly DevTerminkurve terminkurve;
        
	    public MyTkStrategy(Bars bars, Instrument instrument, DevTerminkurve terminkurve, SuriBarRange barRange) : base(bars, instrument) {
		    this.barRange = barRange;
		    this.terminkurve = terminkurve;
	    }
	    public override void UpdateIndicators() {
		    barRange.Update();
		    terminkurve.Update();
	    }
	    protected override string name { get { return "MyTK"; } }
	    protected override int startBarIndex { get { return 1; } }

	    // context
	    private bool comesFrom100;
	    private bool isUntrending;
	    private MyTkSignalType? myTkSignalType;
	    
	    protected override bool IsEntry(int signalIndex) {
		    TkState tkState = terminkurve.GetTkState(signalIndex);
		    double delta = terminkurve.Values[2].GetValueAt(signalIndex);
		    double prevDelta = terminkurve.Values[2].GetValueAt(signalIndex - 1);
		    if (double.IsNaN(delta) || double.IsNaN(prevDelta)) return false;
		    
		    // tk2
		    if (delta >= 99.99) comesFrom100 = true;
		    if (comesFrom100 && delta < 50) {
			    comesFrom100 = false;
			    //isUntrending = true;
			    myTkSignalType = MyTkSignalType.Tk2;
			    return true;
		    }
		    //isUntrending = false;

		    // tk1
		    /*if (!(delta > 70 && prevDelta < 70)) return false;
		    for (int i = index - 1; i >= 0; i--) {
			    double d = terminkurve.Values[2].GetValueAt(i);
			    if (d > 70) return false;
			    if (d < 30) {
				    return true;
			    }
		    }*/
		    return false;
	    }

	    protected override SuriSignal PrepareSignal(int signalIndex) {
		    var tkState = terminkurve.GetTkState(signalIndex);
		    bool isLong;
		    if (myTkSignalType == null) throw new Exception("Unexpected signal type.");
		    MyTkSignalType type = myTkSignalType.Value;
		    if      (tkState.IsAnyBackwardation() == true) isLong = type != MyTkSignalType.Tk2;
		    else if (tkState.IsAnyContango()      == true) isLong = type == MyTkSignalType.Tk2;
		    else throw new Exception("Unexpected TK State.");
		    return new SuriSignal {
			    isLong = isLong,
			    suriRule = SuriRule.Tk,
			    signalIndex = signalIndex,
			    signalDate = bars.GetTime(signalIndex),
			    orderType = OrderType.Market,
			    entryIndex = signalIndex+1,
			    entryDate = bars.GetTime(signalIndex+1),
		    };
	    }

	    protected override bool SetAndCheckInitialStoploss(SuriSignal signal) {
		    double t = SuriCommon.CurrencyToPrice(instrument, 2000);
		    signal.AddStop(bars.GetClose(signal.entryIndex - 1) + (signal.isLong ? -t : t));
		    return true;
	    }

	    protected override void SetExit(SuriSignal signal) {
		    for (int i = signal.entryIndex; i < bars.Count; i++) {
			    /*TkState tkState = terminkurve.GetTkState(i);
			    double delta = terminkurve.Values[2].GetValueAt(i);
			    if (delta < 30 || signal.isLong && tkState.IsAnyContango() == true || !signal.isLong && tkState.IsAnyBackwardation() == true ) {
				    signal.exitReason = "TK counter";
				    signal.exitIndex = i + 1;
				    signal.exitDate = bars.GetTime(i + 1);
				    break;
			    }*/
			    // trace stop
			    if (barRange.IsMegaBar(i) && signal.isLong == StrategyTasks.BarGoesUp(bars, i)) {
				    signal.AddStop(signal.isLong ? bars.GetLow(i) - tickSize : bars.GetHigh(i) + tickSize, i);
			    }
		    }
	    }

    }
    
    internal enum MyTkSignalType {
	    Tk1,
	    Tk2,
    }
}
