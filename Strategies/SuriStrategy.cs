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
			//else if (State == State.DataLoaded) PrepareMyTk();
		}
/*
		private void PrepareCot1() {
			SuriCot1 cot1 = SuriCot1(125, true);
			SuriCot2 cot2 = SuriCot2(true);
			SuriBarRange barRange = SuriBarRange(true, 125);
			SuriVolume volume = SuriVolume(125);
			DevTerminkurve terminkurve = DevTerminkurve(150);
			AddChartIndicator(cot1);
			AddChartIndicator(cot2);
			AddChartIndicator(terminkurve);
			AddChartIndicator(barRange);
			AddChartIndicator(volume);
			strategy = new Cot1Strategy(Bars, Instrument, cot1, cot2, terminkurve);
		}

		private void PrepareCot2() {
			SuriCot2 cot2 = SuriCot2(true);
			SuriBarRange barRange = SuriBarRange(true, 125);
			SuriVolume volume = SuriVolume(125);
			DevTerminkurve terminkurve = DevTerminkurve(150);
			AddChartIndicator(cot2);
			AddChartIndicator(terminkurve);
			AddChartIndicator(barRange);
			AddChartIndicator(volume);
			strategy = new Cot2Strategy(Bars, Instrument, cot2, barRange, volume, terminkurve);
		}

		private void PrepareTk() {
			SuriCot2 cot2 = SuriCot2(true);
			SuriBarRange barRange = SuriBarRange(true, 125);
			DevTerminkurve terminkurve = DevTerminkurve(125);
			AddChartIndicator(cot2);
			AddChartIndicator(terminkurve);
			AddChartIndicator(barRange);
			strategy = new TkStrategy(Bars, Instrument, terminkurve, cot2, barRange);
		}

		private void PrepareMyTk() {
			SuriBarRange barRange = SuriBarRange(true, 125);
			DevTerminkurve terminkurve = DevTerminkurve(150);
			AddChartIndicator(terminkurve);
			AddChartIndicator(barRange);
			strategy = new MyTkStrategy(Bars, Instrument, terminkurve, barRange);
		}
		
		private void PrepareMyVp() {
			SuriBarRange barRange = SuriBarRange(true, 125);
			//SuriVolumeProfileIntraday vpIntra = SuriVolumeProfileIntraday();
			AddChartIndicator(barRange);
			//AddChartIndicator(vpIntra);
			strategy = new VpIntraStrategy(Bars, Instrument, barRange);
		}
*/
	}
}
