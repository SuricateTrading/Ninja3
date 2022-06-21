#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript;
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
			switch (reportField) {
				case SuriCotReportField.OpenInterest: return cotData.openInterest;
				case SuriCotReportField.NoncommercialLong: return cotData.nonCommercialsLong;
				case SuriCotReportField.NoncommercialShort: return cotData.nonCommercialsShort;
				case SuriCotReportField.NoncommercialNet: return cotData.nonCommercialsLong - cotData.nonCommercialsShort;
				
				//case SuriCotReportField.NoncommercialSpreads: return cotData.NoncommercialSpreads;
				
				case SuriCotReportField.CommercialLong: return cotData.commercialsLong;
				case SuriCotReportField.CommercialShort: return cotData.commercialsShort;
				case SuriCotReportField.CommercialNet: return cotData.commercialsLong - cotData.commercialsShort;
				//case SuriCotReportField.TotalLong: return cotData.TotalLong;
				//case SuriCotReportField.TotalShort: return cotData.TotalShort;
				//case SuriCotReportField.TotalNet: return cotData.TotalNet;
				case SuriCotReportField.NonreportablePositionsLong: return cotData.nonReportablesLong;
				case SuriCotReportField.NonreportablePositionsShort: return cotData.nonReportablesShort;
				case SuriCotReportField.NonreportablePositionsNet: return cotData.nonReportablesLong - cotData.nonReportablesShort;
				//case SuriCotReportField.TotalTraders: return cotData.TotalTraders;
				/*case SuriCotReportField.TradersInNoncommercialLong: return cotData.OpenInterest;
				case SuriCotReportField.TradersInNoncommercialShort: return cotData.OpenInterest;
				case SuriCotReportField.TradersInNoncommercialSpreads: return cotData.OpenInterest;
				case SuriCotReportField.TradersInCommercialLong: return cotData.OpenInterest;
				case SuriCotReportField.TradersInCommercialShort: return cotData.OpenInterest;
				case SuriCotReportField.TradersInTotalLong: return cotData.OpenInterest;
				case SuriCotReportField.TradersInTotalShort: return cotData.OpenInterest;*/
			}
			return 0;
		}
	}
}

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
