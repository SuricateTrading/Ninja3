#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Custom.AddOns.SuriData;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.NinjaScript;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriCot : Indicator {
		private CotRepo cotRepo;
		private double min = double.MaxValue;
		private double max = double.MinValue;
		private int minIndex;
		private int maxIndex;
		private DateTime lastReportDate;
		
		#region Properties
		[TypeConverter(typeof(FriendlyEnumConverter))]
		[PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")]
		[NinjaScriptProperty]
		[Display(Name = "COT Daten", Order=0, GroupName = "Parameter")]
		[XmlIgnore]
		public SuriCotReportField reportField { get; set; }
		
		[Browsable(false)]
		public int cotSerialize {
			get { return (int) reportField; }
			set { reportField = (SuriCotReportField) value; }
		}
		
		[XmlIgnore]
		[Display(Name="Zeichne 20 und 80% Linien", Order=0, GroupName="Parameter")]
		public bool drawLines { get; set; }

		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Browsable(false)]
		[Display(Name="Tage der 20% und 80% Linien", Order=1, GroupName="Parameter")]
		public int days { get; set; }

		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der Hauptlinie", Order=2, GroupName="Parameter")]
		public int lineWidth
		{ get; set; }
		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der sekundären Linien", Order=3, GroupName="Parameter")]
		public int lineWidthSecondary
		{ get; set; }

		[XmlIgnore]
		[Display(Name = "Normale Linie", Order = 0, GroupName = "Farben")]
		public Brush regularLineBrush { get; set; }
		[Browsable(false)]
		public string regularLineBrushSerialize {
			get { return Serialize.BrushToString(regularLineBrush); }
			set { regularLineBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "20% Linie", Order = 1, GroupName = "Farben")]
		public Brush brush20 { get; set; }
		[Browsable(false)]
		public string brushSerialize20 {
			get { return Serialize.BrushToString(brush20); }
			set { brush20 = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "80% Linie", Order = 2, GroupName = "Farben")]
		public Brush brush80 { get; set; }
		[Browsable(false)]
		public string brushSerialize80 {
			get { return Serialize.BrushToString(brush80); }
			set { brush80 = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Keine neuen COT Daten", Order = 3, GroupName = "Farben", Description = "Wird benutzt, wenn die CFTC keinen aktuellen COT Report veröffentlicht hat.")]
		public Brush noNewCotBrush { get; set; }
		[Browsable(false)]
		public string noNewCotBrushSerialize {
			get { return Serialize.BrushToString(noNewCotBrush); }
			set { noNewCotBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Name										= "CoT-Daten";
				Description									= @"CoT-Daten";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				
				drawLines									= true;
				brush20										= Brushes.RoyalBlue;
				brush80										= Brushes.RoyalBlue;
				regularLineBrush							= Brushes.DarkGray;
				noNewCotBrush								= Brushes.Orange;
				lineWidth									= 2;
				lineWidthSecondary							= 1;
				reportField									= SuriCotReportField.CommercialLong;
				days										= 1000;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "CoT-Daten");
				if (drawLines) {
					AddPlot(new Stroke(brush20, lineWidthSecondary), PlotStyle.Line, "20%");
					AddPlot(new Stroke(brush80, lineWidthSecondary), PlotStyle.Line, "80%");
				}
			} else if (State == State.DataLoaded) {
				if (Bars.Count > 0) cotRepo = new CotRepo(Instrument, Bars);
			}
		}
		public override string DisplayName { get { return Name + " " + CotReportMaper.ReportToString(reportField); } }
		private double ValueOf(double percent) { return min + percent * (max - min); }
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}

		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None || cotRepo == null) return;
			
			try {
				DbCotData cotData = cotRepo.Get(CurrentBar);
				int? value = GetCotValue(reportField, cotData);
				if (value == null) return;
				Value[0] = value.Value;
			} catch (IndexOutOfRangeException) {
				if (CurrentBar > 10) Value[0] = Value[1];
			}
			
			if (drawLines) {
				SetMinMax();
				if (CurrentBar >= days) {
					Values[1][0] = ValueOf(0.2);
					Values[2][0] = ValueOf(0.8);
				}
			}
			
			if (CurrentBar > 0 && Math.Abs(Value[0] - Value[1]) > 0.00000000001)				lastReportDate = Time[0];
			if (lastReportDate != null && (Time[0].Date - lastReportDate.Date).TotalDays > 10)	PlotBrushes[0][0] = noNewCotBrush;
		}

		private int? GetCotValue(SuriCotReportField field, DbCotData cotData) {
			switch (field) {
				case SuriCotReportField.OpenInterest: return cotData.OpenInterest;
				case SuriCotReportField.NoncommercialLong: return cotData.NonCommercialsLong;
				case SuriCotReportField.NoncommercialShort: return cotData.NonCommercialsShort;
				case SuriCotReportField.NoncommercialNet: return cotData.NonCommercialsLong - cotData.NonCommercialsShort;
				
				//case SuriCotReportField.NoncommercialSpreads: return cotData.NoncommercialSpreads;
				
				case SuriCotReportField.CommercialLong: return cotData.CommercialsLong;
				case SuriCotReportField.CommercialShort: return cotData.CommercialsShort;
				case SuriCotReportField.CommercialNet: return cotData.CommercialsLong - cotData.CommercialsShort;
				//case SuriCotReportField.TotalLong: return cotData.TotalLong;
				//case SuriCotReportField.TotalShort: return cotData.TotalShort;
				//case SuriCotReportField.TotalNet: return cotData.TotalNet;
				case SuriCotReportField.NonreportablePositionsLong: return cotData.NonReportablesLong;
				case SuriCotReportField.NonreportablePositionsShort: return cotData.NonReportablesShort;
				case SuriCotReportField.NonreportablePositionsNet: return cotData.NonReportablesLong - cotData.NonReportablesShort;
				//case SuriCotReportField.TotalTraders: return cotData.TotalTraders;
				/*case SuriCotReportField.TradersInNoncommercialLong: return cotData.OpenInterest;
				case SuriCotReportField.TradersInNoncommercialShort: return cotData.OpenInterest;
				case SuriCotReportField.TradersInNoncommercialSpreads: return cotData.OpenInterest;
				case SuriCotReportField.TradersInCommercialLong: return cotData.OpenInterest;
				case SuriCotReportField.TradersInCommercialShort: return cotData.OpenInterest;
				case SuriCotReportField.TradersInTotalLong: return cotData.OpenInterest;
				case SuriCotReportField.TradersInTotalShort: return cotData.OpenInterest;*/
			}
			return null;
		}
		
		private void SetMinMax() {
			if (min > Value[0]) { min = Value[0]; minIndex = CurrentBar; }
			if (max < Value[0]) { max = Value[0]; maxIndex = CurrentBar; }
			
			if (CurrentBar < days) return;
			if (CurrentBar - maxIndex > days || CurrentBar - minIndex > days) {
				// the last max or min is too far away. Recalculate.
				min = double.MaxValue;
				max = double.MinValue;
				for (int i = 0; i < days; i++) {
					if (min > Value[i]) { min = Value[i]; minIndex = CurrentBar-i; }
					if (max < Value[i]) { max = Value[i]; maxIndex = CurrentBar-i; }
				}
			}
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
