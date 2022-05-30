using System;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public abstract class StrategyInterface {
        public readonly List<SuriSignal> signals = new List<SuriSignal>();
        protected readonly Bars bars;
        protected readonly Instrument instrument;
        
        protected StrategyInterface(Bars bars, Instrument instrument) { this.bars = bars; this.instrument = instrument; }
        
        public abstract void UpdateIndicators();
        protected static void Print(string s) { Code.Output.Process(s, PrintTo.OutputTab1); }
        protected double tickSize { get { return instrument.MasterInstrument.TickSize; } }
        protected abstract string name { get; }
        protected abstract int startBarIndex { get; }
        protected abstract bool IsEntry(int index);
        protected abstract SuriSignal PrepareSignal(int index);
        protected abstract bool SetAndCheckInitialStoploss(SuriSignal signal);
        protected abstract void SetExit(SuriSignal signal);
        
        public virtual void Analyze() {
            Print("Start " + name);
            for (int i = startBarIndex; i < bars.Count; i++) {
                try {
                    if (!IsEntry(i)) continue;
                    SuriSignal signal = PrepareSignal(i);
                    if (!SetAndCheckInitialStoploss(signal)) continue;
                    SetExit(signal);
                    signals.Add(signal);
                } catch (Exception e) {
                    Print("StrategyInterface Analyze Error @" + i + " " + bars.GetTime(i) + "\t\t" + e);
                }
            }
        }
        
    }
}
