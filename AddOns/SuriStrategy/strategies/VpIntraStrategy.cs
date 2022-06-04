using System;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators.Suri;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.Custom.AddOns.SuriCommon.strategies {
    public class VpIntraStrategy : StrategyInterface {
	    private readonly SuriBarRange barRange;
	    private readonly SuriVpIntraData suriVpIntraData;
        
	    public VpIntraStrategy(Bars bars, Instrument instrument, SuriBarRange barRange) : base(bars, instrument) {
		    this.barRange = barRange;
		    suriVpIntraData = SuriIntraRepo.GetVpIntra(instrument, bars.GetTime(0).Date, bars.LastBarTime.Date);
	    }
	    public override void UpdateIndicators() {
		    barRange.Update();
	    }
	    protected override string name { get { return "MyVp"; } }
	    protected override int startBarIndex { get { return 0; } }

	    protected override bool IsEntry(int index) {
		    if (index >= suriVpIntraData.barData.Count) return false;
		    double deltaPercent = 100 * suriVpIntraData.barData[index].delta / suriVpIntraData.barData[index].totalVolume;
		    return deltaPercent > 10;
	    }

	    protected override SuriSignal PrepareSignal(int index) {
		    return new SuriSignal {
			    isLong = false,
			    suriRule = SuriRule.Tk,
			    signalIndex = index,
			    signalDate = bars.GetTime(index),
			    orderType = OrderType.Market,
			    entryIndex = index+1,
			    entryDate = bars.GetTime(index+1),
		    };
	    }

	    protected override bool SetAndCheckInitialStoploss(SuriSignal signal) {
		    //double t = SuriCommon.CurrencyToPrice(instrument, 2000);
		    //signal.AddStop(bars.GetClose(signal.entryIndex - 1) + (signal.isLong ? -t : t));
		    signal.AddStop(signal.isLong ? bars.GetLow(signal.signalIndex) - tickSize*10 : bars.GetHigh(signal.signalIndex) + tickSize*10);
		    return true;
	    }

	    protected override void SetExit(SuriSignal signal) {
		    /*for (int i = signal.entryIndex; i < bars.Count; i++) {
			    // trace stop
			    if (barRange.IsMegaBar(i) && signal.isLong == StrategyTasks.BarGoesUp(bars, i)) {
				    signal.AddStop(signal.isLong ? bars.GetLow(i) - tickSize : bars.GetHigh(i) + tickSize, i);
			    }
		    }*/
		    
		    for (int i = signal.entryIndex; i < bars.Count && i < signal.entryIndex + 10; i++) {
			    double stop = signal.isLong ? bars.GetLow(i) - tickSize : bars.GetHigh(i) + tickSize;
			    if (signal.isLong && stop > signal.stops.Last().Value || !signal.isLong && stop < signal.stops.Last().Value) {
				    signal.AddStop(stop, i);
			    }
		    }
		    
	    }

    }
}
