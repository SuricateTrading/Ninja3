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
        
        private StrategyInterface() {}
        protected StrategyInterface(Bars bars, Instrument instrument) {
            this.bars = bars;
            this.instrument = instrument;
        }
        public abstract void UpdateIndicators();
        protected static void Print(string s) { Code.Output.Process(s, PrintTo.OutputTab1); }
        protected double TickSize { get { return instrument.MasterInstrument.TickSize; } }

        public abstract void Analyze();
        protected abstract bool IsEntry(int index);
        protected abstract SuriSignal PrepareSignal(int index);
        protected abstract bool SetAndCheckInitialStoploss(SuriSignal signal, int index);
        protected abstract void SetExit(SuriSignal signal);
    }
}
