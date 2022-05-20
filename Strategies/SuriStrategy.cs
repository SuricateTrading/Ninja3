#region Using declarations
using NinjaTrader.Custom.AddOns.SuriCommon.strategies;
using NinjaTrader.NinjaScript.Indicators.Suri;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;
#endregion

namespace NinjaTrader.NinjaScript.Strategies {
	public sealed class SuriTest : GenericStrategyFramework {
		protected override void OnStateChange() {
			base.OnStateChange();
			if (State == State.SetDefaults) {
				Name = "SuriTest";
			} else if (State == State.DataLoaded) {
				SuriCot1 cot1 = SuriCot1();
				SuriCot2 cot2 = SuriCot2();
				SuriVolume volume = SuriVolume(125);
				SuriBarRange barRange = SuriBarRange(125);
				DevTerminkurve terminkurve = DevTerminkurve();
				//AddChartIndicator(strategy.indicator);
				strategies.Add(new Cot1Strategy(Bars, Instrument, cot1, cot2, terminkurve));
			}
		}
	}
}
