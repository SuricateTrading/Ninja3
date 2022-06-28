#region Using declarations
using System;
using System.ComponentModel;
using NinjaTrader.Custom.AddOns.SuriCommon;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriCot : Suri2080Indicator {
		[Browsable(true)]
		public override SuriCotReportField reportField { get; set; }

		private String initialName;
		protected override void OnStateChange() {
			base.OnStateChange();
			if (State == State.SetDefaults) {
				Description = @"CoT-Daten";
				Name = "CoT-Daten";
				initialName = Name;
				reportField = SuriCotReportField.CommercialShort;
			}
		}
		public override string DisplayName { get { return Name.Equals(initialName) ? CotReportMaper.ReportToString(reportField) : Name; } }
		protected override string plotName { get { return CotReportMaper.ReportToString(reportField); } }
		protected override double GetMainValue(DbCotData cotData) {
			return cotData.GetByReportField(reportField);
		}
	}
}







//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriCot[] cacheSuriCot;
		public Suri.SuriCot SuriCot()
		{
			return SuriCot(Input);
		}

		public Suri.SuriCot SuriCot(ISeries<double> input)
		{
			if (cacheSuriCot != null)
				for (int idx = 0; idx < cacheSuriCot.Length; idx++)
					if (cacheSuriCot[idx] != null &&  cacheSuriCot[idx].EqualsInput(input))
						return cacheSuriCot[idx];
			return CacheIndicator<Suri.SuriCot>(new Suri.SuriCot(), input, ref cacheSuriCot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCot SuriCot()
		{
			return indicator.SuriCot(Input);
		}

		public Indicators.Suri.SuriCot SuriCot(ISeries<double> input )
		{
			return indicator.SuriCot(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCot SuriCot()
		{
			return indicator.SuriCot(Input);
		}

		public Indicators.Suri.SuriCot SuriCot(ISeries<double> input )
		{
			return indicator.SuriCot(input);
		}
	}
}

#endregion
