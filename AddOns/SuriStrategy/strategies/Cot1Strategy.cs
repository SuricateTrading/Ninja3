using System;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators.Suri;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.Custom.AddOns.SuriCommon.strategies {
    public class Cot1Strategy : StrategyInterface {
	    private readonly SuriCot1 cot1;
	    private readonly SuriCot2 cot2;
        private readonly DevTerminkurve terminkurve;

        public Cot1Strategy(Bars bars, Instrument instrument, SuriCot1 cot1, SuriCot2 cot2, DevTerminkurve terminkurve) : base(bars, instrument) {
	        this.cot1 = cot1;
	        this.cot2 = cot2;
            this.terminkurve = terminkurve;
        }
        public override void UpdateIndicators() {
	        cot1.Update();
	        cot2.Update();
	        terminkurve.Update();
        }
        protected override string name { get { return "COT1"; } }
        protected override int startBarIndex { get { return 0; } }

        public override void Analyze() {
	        Print("Start COT1");
	        // hint: signalIndex is the release date of the cot report, which usually is the last trading day of the week.
	        foreach (int signalIndex in cot1.signalIndices) {
		        try {
			        if (!IsEntry(signalIndex)) continue;
			        SuriSignal signal = PrepareSignal(signalIndex);
					
			        // check at which index the entry will be filled
			        int? entryIndex = StrategyTasks.GetIndexOfValueFill(signal.signalIndex, bars, signal.entry.Value,index => (bars.GetTime(index) - signal.signalDate).TotalDays >= 42);
			        if (entryIndex == null) {
				        Print("Skip COT1 " + bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". No entry " + signal.entry + " found after 6 weeks. Hint: This check is executed before valuating the size of the stop or counter signals!");
				        continue;
			        }
			        signal.entryIndex = entryIndex.Value;
			        signal.entryDate = bars.GetTime(signal.entryIndex);
			        
			        // iff cot2 or tk contradict at entry date, then skip this signal.
			        bool isCot2Long = cot2.IsInLongHalf(signal.entryIndex - 1);
			        TkState tkState = terminkurve.GetTkState(signal.entryIndex - 1);
			        if (signal.isLong && !isCot2Long && tkState.IsAnyBackwardation() != true) {
				        Print("Skip COT1 " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". COT2 and TK contradict COT1 long trade at entry.");
				        continue;
			        }
			        if (!signal.isLong && (isCot2Long || tkState.IsAnyBackwardation() == true)) {
				        Print("Skip COT1 " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". COT2 or TK contradict COT1 short trade at entry.");
				        continue;
			        }
					
			        if (!SetAndCheckInitialStoploss(signal)) continue;
			        SetExit(signal);
			        signals.Add(signal);
		        } catch (Exception e) {
			        Print(e.ToString());
		        }
	        }
        }
        
        protected override bool IsEntry(int index) {
	        bool isCot1Long = cot1.Value.GetValueAt(index) > 89.9;
	        bool isCot2Long = cot2.IsInLongHalf(index);
	        TkState tkState = terminkurve.GetTkState(index);
	        if (isCot1Long && !isCot2Long && tkState.IsAnyBackwardation() != true) {
		        // long: wenn cot2 unter 50, dann tk egal. wenn cot2 über 50, dann tk muss in backwardation
		        Print("Skip COT1 " + bars.GetTime(index).ToShortDateString() + " @" + index + ". COT2 and TK contradict COT1 long trade at signal.");
		        return false;
	        }
	        if (!isCot1Long && (isCot2Long || tkState.IsAnyBackwardation() == true)) {
		        // short: in contango und cot2 über 50
		        string reason = "";
		        if (isCot2Long && tkState.IsAnyBackwardation() == true) reason = "COT2 and TK";
		        else if (isCot2Long) reason = "COT2";
		        else if (tkState.IsAnyBackwardation() == true) reason = "TK";
		        Print("Skip COT1 " + bars.GetTime(index).ToShortDateString() + " @" + index + ". " + reason + " contradict COT1 short trade at signal.");
		        return false;
	        }
	        return true;
        }

        protected override SuriSignal PrepareSignal(int index) {
	        bool isCot1Long = cot1.Value.GetValueAt(index) > 89.9;
	        SuriSignal signal = new SuriSignal {
		        suriRule = SuriRule.Cot1,
		        isLong = isCot1Long,
		        signalIndex = index,
		        signalDate = bars.GetTime(index),
		        orderType = OrderType.StopMarket,
		        entry = isCot1Long
			        ? StrategyTasks.GetWeekHigh(index - 2, bars) + instrument.MasterInstrument.TickSize
			        : StrategyTasks.GetWeekLow (index - 2, bars) - instrument.MasterInstrument.TickSize
			        // note: I reduced the index by 2 because it may happen that friday is a holiday, thus the cot report is released on monday. The strategy would then calculate the wrong week.
	        };
	        signal.stopPrice = signal.entry.Value;
	        return signal;
        }

        protected override bool SetAndCheckInitialStoploss(SuriSignal signal) {
	        double stop = signal.isLong
		        ? StrategyTasks.GetLast10DaysLow(signal.entryIndex, bars) - instrument.MasterInstrument.TickSize
		        : StrategyTasks.GetLast10DaysHigh(signal.entryIndex, bars) + instrument.MasterInstrument.TickSize;
	        signal.AddStop(stop);
	        double stoplossCurrency = SuriCommon.PriceToCurrency(instrument, Math.Abs(stop - signal.entry.Value));
	        if (stoplossCurrency >= 2100) {
		        Print("Skip COT1 " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". Stop " + stoplossCurrency + " $ too high. " + signal.Serialize());
		        // todo: eigentlich ist das gemogelt. hier wird die entry-bar mit berücksichtigt, ob man den trade macht oder nicht.
		        return false;
	        }
	        //signal.stoplossIndex = StrategyTasks.GetIndexOfValueFill(signal.entryIndex.Value, bars, signal.stoploss);
	        //if (signal.stoplossIndex != null) signal.stoplossDate = bars.GetTime(signal.stoplossIndex.Value);
	        return true;
        }

        protected override void SetExit(SuriSignal signal) {
	        string exitReason = null;
	        for (int i = signal.entryIndex + 1; i < bars.Count; i++) {
		        // cot1
		        double value = cot1.Value.GetValueAt(i);
		        if (value >= 90 && !signal.isLong || value <= 10 && signal.isLong) {
			        exitReason = "COT 1 counter signal";
			        signal.exitIndex = i + 1;
			        break;
		        }
		        // cot2
		        // todo: bei einem cot1 long trade darf cot2 short sein, falls eine backwardation da ist. In diesem Fall würde aber gleich am nächsten tag der trade beendet werden durch die folgenden Zeilen.
		        if (signal.isLong != cot2.IsInLongHalf(i)) {
			        exitReason = "COT 2 counter signal";
			        signal.exitIndex = i + 1;
			        break;
		        }
		        // tk
		        TkState tkState = terminkurve.GetTkState(i);
		        TkState prevTkState = terminkurve.GetTkState(i - 1);
		        if (signal.isLong && tkState.IsBackwardationToContango(prevTkState) || !signal.isLong && tkState.IsContangoToBackwardation(prevTkState)) {
			        exitReason = "TK counter signal";
			        signal.exitIndex = i + 1;
			        break;
		        }
	        }
	        if (exitReason != null && signal.exitIndex != null) {
		        signal.exitDate = bars.GetTime(signal.exitIndex.Value);
		        signal.exitReason = exitReason;
	        }
        }

    }
}
