#region Using declarations
using NinjaTrader.Custom.AddOns.SuriCommon.strategies;
using NinjaTrader.NinjaScript.Indicators.Suri;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;
#endregion

namespace NinjaTrader.NinjaScript.Strategies {
	public sealed class SuriTest : GenericStrategyFramework {
		protected override void OnStateChange() {
			base.OnStateChange();
			if (State == State.SetDefaults) Name = "SuriTest";
			else if (State == State.DataLoaded) {
				SuriCot1 cot1 = SuriCot1();
				SuriCot2 cot2 = SuriCot2();
				SuriBarRange barRange = SuriBarRange(125);
				SuriVolume volume = SuriVolume(125);
				DevTerminkurve terminkurve = DevTerminkurve();
				AddChartIndicator(cot1);
				AddChartIndicator(cot2);
				AddChartIndicator(terminkurve);
				AddChartIndicator(barRange);
				AddChartIndicator(volume);
				strategies.Add(new Cot1Strategy(Bars, Instrument, cot1, cot2, terminkurve));
				strategies.Add(new Cot2Strategy(Bars, Instrument, cot2, barRange, volume, terminkurve));
				strategies.Add(new TkStrategy(Bars, Instrument, terminkurve, cot2, barRange));
			}
		}
	}
}
