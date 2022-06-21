#region Using declarations
using NinjaTrader.Custom.AddOns.SuriCommon;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class ComShortOpenInterest : Suri2080Indicator {
		protected override void OnStateChange() {
			base.OnStateChange();
			if (State == State.SetDefaults) {
				Description = @"Commercials Short geteilt durch Open Interest in Prozent";
				Name = "ComShort / OI";
			}
		}
		protected override string plotName { get { return "Com Short / OI in %"; } }
		protected override double GetMainValue(DbCotData cotData) { return 100 * cotData.commercialsShort / (double) cotData.openInterest; }
	}
}
