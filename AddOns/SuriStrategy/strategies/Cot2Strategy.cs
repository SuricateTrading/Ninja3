using System;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators.Suri;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.Custom.AddOns.SuriCommon.strategies {
    public class Cot2Strategy : StrategyInterface {
	    private readonly SuriCot2 cot2;
	    private readonly SuriBarRange barRange;
	    private readonly SuriVolume volume;
        private readonly DevTerminkurve terminkurve;
        
        public Cot2Strategy(Bars bars, Instrument instrument, SuriCot2 cot2, SuriBarRange barRange, SuriVolume volume, DevTerminkurve terminkurve) : base(bars, instrument) {
	        this.cot2 = cot2;
	        this.barRange = barRange;
	        this.volume = volume;
            this.terminkurve = terminkurve;
        }
        public override void UpdateIndicators() {
	        cot2.Update();
	        barRange.Update();
	        volume.Update();
	        terminkurve.Update();
        }
        protected override string name { get { return "COT2"; } }
        protected override int startBarIndex { get { return 125; } }

        protected override bool IsEntry(int index) {
	        bool isMegaBar = barRange.IsMegaRange(index);
	        bool isMegaVolume = volume.IsMegaVolume(index);
	        if (!isMegaBar && !isMegaVolume) return false;
					
	        SuriPosition cot2Position = SuriPosition.None;
	        if (cot2.seriesMain.GetValueAt(index) <= cot2.series25.GetValueAt(index)) cot2Position = SuriPosition.Long;
	        if (cot2.seriesMain.GetValueAt(index) >= cot2.series75.GetValueAt(index)) cot2Position = SuriPosition.Short;
	        if (cot2Position == SuriPosition.None) return false;
					
	        // tk
	        TkState tkState = terminkurve.GetTkState(index);
	        if (cot2Position == SuriPosition.Short && tkState.IsAnyBackwardation() == true) {
		        Print("Skip COT2 " + bars.GetTime(index).ToShortDateString() + " @" + index + ". COT2 short but TK was in backwardation.");
		        return false;
	        }

	        return true;
        }

        protected override SuriSignal PrepareSignal(int index) {
	        SuriSignal signal = new SuriSignal {
		        suriRule = SuriRule.Cot2,
		        isLong = cot2.GetSuriPosition(index) == SuriPosition.Long,
		        signalIndex = index,
		        signalDate = bars.GetTime(index),
		        orderType = OrderType.Market,
		        entryIndex = index+1
	        };
	        if (index + 1 < bars.Count) {
		        signal.entry     = bars.GetOpen(index + 1);
		        signal.entryDate = bars.GetTime(index + 1);
	        }
	        return signal;
        }

        protected override bool SetAndCheckInitialStoploss(SuriSignal signal) {
			// check iff valid cot 2 versions and calculate initial stoploss
			bool isEndOfTrend = true; // todo: check if end of trend
			SuriBarType barType = StrategyTasks.GetBarType(bars, signal.signalIndex, tickSize);
			if (signal.isLong && barType == SuriBarType.MegabarDown || !signal.isLong && barType == SuriBarType.MegabarUp) {
				if (!isEndOfTrend) {
					Print("Skip COT2 " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". V1 not end of trend.");
					return false;
				}
				signal.notes += "V1. ";
				StrikingSpotData strikingSpotData = StrikingCalculator.FindStrikingSpot(!signal.isLong, bars, signal.signalIndex);
				signal.AddStop(strikingSpotData.p2Value + (signal.isLong ? -tickSize : tickSize));
			} else if (signal.isLong && barType == SuriBarType.MegabarUp || !signal.isLong && barType == SuriBarType.MegabarDown) {
				signal.notes += "V2. ";
				if (isEndOfTrend) {
					signal.AddStop(signal.isLong ? bars.GetLow(signal.signalIndex) - tickSize : bars.GetHigh(signal.signalIndex) + tickSize);
				} else {
					StrikingSpotData strikingSpotData = StrikingCalculator.FindStrikingSpot(!signal.isLong, bars, signal.signalIndex);
					signal.AddStop(strikingSpotData.p2Value);
				}
			} else if (signal.isLong && (barType == SuriBarType.ReversalBarTop    || barType == SuriBarType.ReversalBarMiddleTop) ||
				      !signal.isLong && (barType == SuriBarType.ReversalBarBottom || barType == SuriBarType.ReversalBarMiddleBottom)) {
				signal.notes += "V3. ";
				if (!isEndOfTrend) {
					Print("Skip COT2 " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". V3 not end of trend.");
					return false;
				}
				signal.AddStop(signal.isLong ? bars.GetLow(signal.signalIndex) - tickSize : bars.GetHigh(signal.signalIndex) + tickSize);
			} else if (signal.isLong && (barType == SuriBarType.ReversalBarBottom || barType == SuriBarType.ReversalBarMiddleBottom) ||
			           !signal.isLong && (barType == SuriBarType.ReversalBarTop    || barType == SuriBarType.ReversalBarMiddleTop)) {
				// v4
				Print("Skip COT2 " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". Bad reversal bar (v4).");
				return false;
			} else {
				Print("Skip COT2 " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". No valid COT2 version.");
				return false;
			}

			double stoplossCurrency = SuriCommon.PriceToCurrency(instrument, Math.Abs(signal.currentStop - bars.GetClose(signal.signalIndex)));
			if (stoplossCurrency > 2000) {
				Print("Skip COT2 " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". Stop " + stoplossCurrency + " $ too high.");
				return false;
			}
			return true;
        }

        protected override void SetExit(SuriSignal signal) {
	        for (int j = signal.entryIndex.Value; j < bars.Count; j++) {
		        // cot2
		        if (signal.isLong && cot2.seriesMain.GetValueAt(j) >= cot2.series75.GetValueAt(j) || !signal.isLong && cot2.seriesMain.GetValueAt(j) <= cot2.series25.GetValueAt(j)) {
			        signal.exitIndex = j + 1;
			        signal.exitDate = bars.GetTime(j + 1);
			        signal.exitReason = "COT 2 counter signal";
			        break;
		        }
		        // tk
		        TkState tkState = terminkurve.GetTkState(j);
		        TkState prevTkState = terminkurve.GetTkState(j - 1);
		        if (signal.isLong && tkState.IsBackwardationToContango(prevTkState) || !signal.isLong && tkState.IsContangoToBackwardation(prevTkState)) {
			        signal.exitReason = "TK counter signal";
			        signal.exitIndex = j + 1;
			        break;
		        }
		        // trace stop
		        if (signal.isLong != cot2.IsInLongHalf(j) && barRange.IsMegaRange(j) && signal.isLong == StrategyTasks.BarGoesUp(bars, j)) {
			        signal.AddStop(signal.isLong ? bars.GetLow(j) - tickSize : bars.GetHigh(j) + tickSize, j + 1);
		        }
	        }
        }

    }
}
