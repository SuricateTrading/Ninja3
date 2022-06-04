using System;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators.Suri;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.Custom.AddOns.SuriCommon.strategies {
    public class MyTkStrategy : StrategyInterface {
	    private readonly SuriCot2 cot2;
	    private readonly SuriBarRange barRange;
	    private readonly DevTerminkurve terminkurve;
        
	    public MyTkStrategy(Bars bars, Instrument instrument, DevTerminkurve terminkurve, SuriCot2 cot2, SuriBarRange barRange) : base(bars, instrument) {
		    this.cot2 = cot2;
		    this.barRange = barRange;
		    this.terminkurve = terminkurve;
	    }
	    public override void UpdateIndicators() {
		    cot2.Update();
		    barRange.Update();
		    terminkurve.Update();
	    }
	    protected override string name { get { return "MyTK"; } }
	    protected override int startBarIndex { get { return 1; } }

	    // context
	    private bool comesFrom100;
	    private bool isUntrending;
	    private MyTkSignalType? myTkSignalType;
	    
	    protected override bool IsEntry(int index) {
		    TkState tkState = terminkurve.GetTkState(index);
		    double delta = terminkurve.Values[2].GetValueAt(index);
		    double prevDelta = terminkurve.Values[2].GetValueAt(index - 1);
		    if (double.IsNaN(delta) || double.IsNaN(prevDelta)) return false;
		    
		    // tk2
		    /*if (delta >= 99.99) comesFrom100 = true;
		    if (comesFrom100 && delta < 50) {
			    comesFrom100 = false;
			    isUntrending = true;
			    return true;
		    }
		    isUntrending = false;*/
		    
		    // tk1
		    if (!(delta > 70 && prevDelta < 70)) return false;
		    for (int i = index - 1; i >= 0; i--) {
			    double d = terminkurve.Values[2].GetValueAt(i);
			    if (d > 70) return false;
			    if (d < 30) {
				    return true;
			    }
		    }
		    return false;
	    }

	    protected override SuriSignal PrepareSignal(int index) {
		    var tkState = terminkurve.GetTkState(index);
		    bool isLong;
		    if      (tkState.IsAnyBackwardation() == true) isLong = true;
		    else if (tkState.IsAnyContango()      == true) isLong = false;
		    else throw new Exception("Unexpected TK State.");
		    return new MyTkSignal {
			    isLong = isLong,
			    suriRule = SuriRule.Tk,
			    signalIndex = index,
			    signalDate = bars.GetTime(index),
			    orderType = OrderType.Market,
			    entryIndex = index+1,
			    entryDate = bars.GetTime(index+1),
		    };
	    }

	    protected override bool SetAndCheckInitialStoploss(SuriSignal signal) {
		    double t = SuriCommon.CurrencyToPrice(instrument, 2000);
		    signal.AddStop(bars.GetClose(signal.entryIndex - 1) + (signal.isLong ? -t : t));
		    return true;
	    }

	    protected override void SetExit(SuriSignal signal) {
		    signal = signal as MyTkSignal;
		    if (signal == null) throw new Exception("Unexpected Signal");
		    for (int i = signal.entryIndex; i < bars.Count; i++) {
			    TkState tkState = terminkurve.GetTkState(i);
			    double delta = terminkurve.Values[2].GetValueAt(i);
			    if (delta < 30 || signal.isLong && tkState.IsAnyContango() == true || !signal.isLong && tkState.IsAnyBackwardation() == true ) {
				    signal.exitReason = "TK counter";
				    signal.exitIndex = i + 1;
				    signal.exitDate = bars.GetTime(i + 1);
				    break;
			    }
			    // trace stop
			    if (barRange.IsMegaBar(i) && signal.isLong == StrategyTasks.BarGoesUp(bars, i)) {
				    signal.AddStop(signal.isLong ? bars.GetLow(i) - tickSize : bars.GetHigh(i) + tickSize, i);
			    }
		    }
	    }

    }
    
    internal class MyTkSignal : SuriSignal {
	    public MyTkSignalType myTkSignalType;
    }

    internal enum MyTkSignalType {
	    Tk1,
	    Tk2,
    }
}
