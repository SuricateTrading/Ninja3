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
		[TypeConverter(typeof(FriendlyEnumConverter))]
		[PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")]
		[NinjaScriptProperty]
		[Display(Name = "COT Daten", Order=0, GroupName = "Parameter")]
		[XmlIgnore]
		public SuriCotReportField reportField { get; set; }
		
		protected override void OnStateChange() {
			base.OnStateChange();
			if (State == State.SetDefaults) {
				Description = @"CoT-Daten";
				Name = "CoT-Daten";
				reportField = SuriCotReportField.CommercialShort;
			}
		}
		public override string DisplayName { get { return CotReportMaper.ReportToString(reportField); } }
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


#region Boilerplate code for enum list
public abstract class CotReportMaper {
	public static String ReportToString(SuriCotReportField suriCotReportField) {
		switch (suriCotReportField) {
			case SuriCotReportField.OpenInterest: return "Open Interest";
			case SuriCotReportField.NoncommercialLong: return "Non Commercials Long";
			case SuriCotReportField.NoncommercialShort: return "Non Commercials Short";
			case SuriCotReportField.NoncommercialSpreads: return "Non Commercials Spreads";
			case SuriCotReportField.NoncommercialNet: return "Non Commercials Netto";
			case SuriCotReportField.CommercialLong: return "Commercials Long";
			case SuriCotReportField.CommercialShort: return "Commercials Short";
			case SuriCotReportField.CommercialNet: return "Commercials Netto";
			/*case SuriCotReportField.TotalLong: return "Total Long";
			case SuriCotReportField.TotalShort: return "Total Short";
			case SuriCotReportField.TotalNet: return "Total Net";*/
			case SuriCotReportField.NonreportablePositionsLong: return "Non Reportables Long";
			case SuriCotReportField.NonreportablePositionsShort: return "Non Reportables Short";
			case SuriCotReportField.NonreportablePositionsNet: return "Non Reportables Netto";
			/*case SuriCotReportField.TotalTraders: return "Total Traders";
			case SuriCotReportField.TradersInNoncommercialLong: return "Trader in Non Commercials Long";
			case SuriCotReportField.TradersInNoncommercialShort: return "Trader in Non Commercials Short";
			case SuriCotReportField.TradersInNoncommercialSpreads: return "Trader in Non Commercials Spreads";
			case SuriCotReportField.TradersInCommercialLong: return "Trader in Commercials Long";
			case SuriCotReportField.TradersInCommercialShort: return "Trader in Commercial Short";
			case SuriCotReportField.TradersInTotalLong: return "Trader in Total Long";
			case SuriCotReportField.TradersInTotalShort: return "Trader in Total Short";*/
			default: return "";
		}
	}
	public static SuriCotReportField StringToReport(String s) {
		switch (s) {
			case "Open Interest": return SuriCotReportField.OpenInterest;
			case "Non Commercials Long": return SuriCotReportField.NoncommercialLong;
			case "Non Commercials Short": return SuriCotReportField.NoncommercialShort;
			case "Non Commercials Spreads": return SuriCotReportField.NoncommercialSpreads;
			case "Non Commercials Netto": return SuriCotReportField.NoncommercialNet;
			case "Commercials Long": return SuriCotReportField.CommercialLong;
			case "Commercials Short": return SuriCotReportField.CommercialShort;
			case "Commercials Netto": return SuriCotReportField.CommercialNet;
			/*case "Total Long": return SuriCotReportField.TotalLong;
			case "Total Short": return SuriCotReportField.TotalShort;
			case "Total Net": return SuriCotReportField.TotalNet;*/
			case "Non Reportables Long": return SuriCotReportField.NonreportablePositionsLong;
			case "Non Reportables Short": return SuriCotReportField.NonreportablePositionsShort;
			case "Non Reportables Netto": return SuriCotReportField.NonreportablePositionsNet;
			/*case "Total Traders": return SuriCotReportField.TotalTraders;
			case "Trader in Non Commercials Long": return SuriCotReportField.TradersInNoncommercialLong;
			case "Trader in Non Commercials Short": return SuriCotReportField.TradersInNoncommercialShort;
			case "Trader in Non Commercials Spreads": return SuriCotReportField.TradersInNoncommercialSpreads;
			case "Trader in Commercials Long": return SuriCotReportField.TradersInCommercialLong;
			case "Trader in Commercial Short": return SuriCotReportField.TradersInCommercialShort;
			case "Trader in Total Long": return SuriCotReportField.TradersInTotalLong;
			case "Trader in Total Short": return SuriCotReportField.TradersInTotalShort;*/
			default: return SuriCotReportField.OpenInterest;
		}
	}
	public static CotReportField SuriToCotReport(SuriCotReportField reportField) {
		switch (reportField) {
			case SuriCotReportField.OpenInterest: return CotReportField.OpenInterest;
			case SuriCotReportField.NoncommercialLong: return CotReportField.NoncommercialLong;
			case SuriCotReportField.NoncommercialShort: return CotReportField.NoncommercialShort;
			case SuriCotReportField.NoncommercialSpreads: return CotReportField.NoncommercialSpreads;
			case SuriCotReportField.NoncommercialNet: return CotReportField.NoncommercialNet;
			case SuriCotReportField.CommercialLong: return CotReportField.CommercialLong;
			case SuriCotReportField.CommercialShort: return CotReportField.CommercialShort;
			case SuriCotReportField.CommercialNet: return CotReportField.CommercialNet;
			/*case SuriCotReportField.TotalLong: return CotReportField.TotalLong;
			case SuriCotReportField.TotalShort: return CotReportField.TotalShort;
			case SuriCotReportField.TotalNet: return CotReportField.TotalNet;*/
			case SuriCotReportField.NonreportablePositionsLong: return CotReportField.NonreportablePositionsLong;
			case SuriCotReportField.NonreportablePositionsShort: return CotReportField.NonreportablePositionsShort;
			case SuriCotReportField.NonreportablePositionsNet: return CotReportField.NonreportablePositionsNet;
			/*case SuriCotReportField.TotalTraders: return CotReportField.TotalTraders;
			case SuriCotReportField.TradersInNoncommercialLong: return CotReportField.TradersInNoncommercialLong;
			case SuriCotReportField.TradersInNoncommercialShort: return CotReportField.TradersInNoncommercialShort;
			case SuriCotReportField.TradersInNoncommercialSpreads: return CotReportField.TradersInNoncommercialSpreads;
			case SuriCotReportField.TradersInCommercialLong: return CotReportField.TradersInCommercialLong;
			case SuriCotReportField.TradersInCommercialShort: return CotReportField.TradersInCommercialShort;
			case SuriCotReportField.TradersInTotalLong: return CotReportField.TradersInTotalLong;
			case SuriCotReportField.TradersInTotalShort: return CotReportField.TradersInTotalShort;*/
			default: return CotReportField.OpenInterest;
		}
	}
}

public class FriendlyEnumConverter : TypeConverter {
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
        //List<string> values = new List<string>() { "Open Interest", "Non Commercials Long", "Non Commercials Short", "Non Commercials Spreads", "Non Commercials Netto", "Commercials Long", "Commercials Short", "Commercials Netto", "Total Long", "Total Short", "Total Net", "Non Reportables Long", "Non Reportables Short", "Non Reportables Netto", "Total Traders", "Trader in Non Commercials Long", "Trader in Non Commercials Short", "Trader in Non Commercials Spreads", "Trader in Commercials Long", "Trader in Commercial Short", "Trader in Total Long", "Trader in Total Short" };
        List<string> values = new List<string>() { "Open Interest", "Non Commercials Long", "Non Commercials Short", "Non Commercials Spreads", "Non Commercials Netto", "Commercials Long", "Commercials Short", "Commercials Netto", "Non Reportables Long", "Non Reportables Short", "Non Reportables Netto" };
        return new StandardValuesCollection(values);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
	    if(value==null) return SuriCotReportField.OpenInterest;
	    return CotReportMaper.StringToReport(value.ToString());
    }
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
		if (value is SuriCotReportField) return CotReportMaper.ReportToString((SuriCotReportField) value);
		return base.ConvertFrom(context, culture, value);
	}
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) { return true; }
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) { return true; }
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
}
    
public enum SuriCotReportField {
	OpenInterest,

	NoncommercialLong,
	NoncommercialShort,
	NoncommercialSpreads,
	NoncommercialNet,

	CommercialLong,
	CommercialShort,
	CommercialNet,

	/*TotalLong,
	TotalShort,
	TotalNet,*/

	NonreportablePositionsLong,
	NonreportablePositionsShort,
	NonreportablePositionsNet,

	/*TotalTraders,
	TradersInNoncommercialLong,
	TradersInNoncommercialShort,
	TradersInNoncommercialSpreads,
	TradersInCommercialLong,
	TradersInCommercialShort,
	TradersInTotalLong,
	TradersInTotalShort,*/
}

#endregion


//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriCot[] cacheSuriCot;
		public Suri.SuriCot SuriCot(SuriCotReportField reportField)
		{
			return SuriCot(Input, reportField);
		}

		public Suri.SuriCot SuriCot(ISeries<double> input, SuriCotReportField reportField)
		{
			if (cacheSuriCot != null)
				for (int idx = 0; idx < cacheSuriCot.Length; idx++)
					if (cacheSuriCot[idx] != null && cacheSuriCot[idx].reportField == reportField && cacheSuriCot[idx].EqualsInput(input))
						return cacheSuriCot[idx];
			return CacheIndicator<Suri.SuriCot>(new Suri.SuriCot(){ reportField = reportField }, input, ref cacheSuriCot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCot SuriCot(SuriCotReportField reportField)
		{
			return indicator.SuriCot(Input, reportField);
		}

		public Indicators.Suri.SuriCot SuriCot(ISeries<double> input , SuriCotReportField reportField)
		{
			return indicator.SuriCot(input, reportField);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCot SuriCot(SuriCotReportField reportField)
		{
			return indicator.SuriCot(Input, reportField);
		}

		public Indicators.Suri.SuriCot SuriCot(ISeries<double> input , SuriCotReportField reportField)
		{
			return indicator.SuriCot(input, reportField);
		}
	}
}

#endregion
