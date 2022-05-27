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
	        foreach (int signalIndex in cot1.signalIndices) {
		        try {
			        if (!IsEntry(signalIndex)) continue;
			        SuriSignal signal = PrepareSignal(signalIndex);
					
			        // check at which index the entry will be filled
			        DateTime signalDate = bars.GetTime(signalIndex);
			        signal.entryIndex = StrategyTasks.GetIndexOfValueFill(signal.signalIndex, bars, signal.entry.Value,index => (bars.GetTime(index) - signalDate).TotalDays >= 42);
			        if (signal.entryIndex == null) {
				        Print("Skip COT1 " + bars.GetTime(signalIndex).ToShortDateString() + " @" + signalIndex + ". No entry " + signal.entry + " found after 6 weeks. Hint: This check is executed before valuating the size of the stop or counter signals!");
				        continue;
			        }
			        signal.entryDate = bars.GetTime(signal.entryIndex.Value);
					
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
		        Print("Skip COT1 " + bars.GetTime(index).ToShortDateString() + " @" + index + ". COT2 and TK contradict COT1 long trade.");
		        return false;
	        }
	        if (!isCot1Long && (isCot2Long || tkState.IsAnyBackwardation() == true)) {
		        // short: in contango und cot2 über 50
		        Print("Skip COT1 " + bars.GetTime(index).ToShortDateString() + " @" + index + ". COT2 or TK contradict COT1 short trade.");
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
			        ? StrategyTasks.GetWeekHigh(index, bars) + instrument.MasterInstrument.TickSize
			        : StrategyTasks.GetWeekLow (index, bars) - instrument.MasterInstrument.TickSize
	        };
	        signal.stopPrice = signal.entry.Value;
	        return signal;
        }

        protected override bool SetAndCheckInitialStoploss(SuriSignal signal) {
	        signal.AddStop(signal.isLong
		        ? StrategyTasks.GetLast10DaysLow (signal.entryIndex.Value, bars) - instrument.MasterInstrument.TickSize
		        : StrategyTasks.GetLast10DaysHigh(signal.entryIndex.Value, bars) + instrument.MasterInstrument.TickSize
	        );
	        double stoplossCurrency = SuriCommon.PriceToCurrency(instrument, Math.Abs(signal.currentStop - signal.entry.Value));
	        if (stoplossCurrency > 2000) {
		        Print("Skip COT1 " + bars.GetTime(signal.signalIndex).ToShortDateString() + " @" + signal.signalIndex + ". Stop " + stoplossCurrency + " $ too high.");
		        return false;
	        }
	        //signal.stoplossIndex = StrategyTasks.GetIndexOfValueFill(signal.entryIndex.Value, bars, signal.stoploss);
	        //if (signal.stoplossIndex != null) signal.stoplossDate = bars.GetTime(signal.stoplossIndex.Value);
	        return true;
        }

        protected override void SetExit(SuriSignal signal) {
	        string exitReason = null;
	        for (int i = signal.entryIndex.Value + 1; i < bars.Count; i++) {
		        // cot1
		        double value = cot1.Value.GetValueAt(i);
		        if (value >= 90 && !signal.isLong || value <= 10 && signal.isLong) {
			        exitReason = "COT 1 counter signal";
			        signal.exitIndex = StrategyTasks.GetNextWeekIndex(i, bars);
			        break;
		        }
		        // cot2
		        if (signal.isLong != cot2.IsInLongHalf(i)) {
			        exitReason = "COT 2 counter signal";
			        signal.exitIndex = StrategyTasks.GetNextWeekIndex(i, bars);
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
