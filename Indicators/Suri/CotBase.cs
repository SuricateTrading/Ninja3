#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Core;
using NinjaTrader.Custom.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class CotBase : Indicator {
		private CotReport sCot;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Name										= "CoT-Daten";
				Description									= @"CoT-Daten";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				lineWidth									= 2;
				lineBrush									= Brushes.DarkGray;
				straightenLines								= false;
				reportField									= SuriCotReportField.CommercialLong;
			} else if (State == State.Configure) {
				sCot = new CotReport { ReportType = CotReportType.Futures, Field = CotReportMaper.SuriToCotReport(reportField) };
				AddPlot(new Stroke(straightenLines ? Brushes.Transparent : lineBrush, lineWidth), PlotStyle.Line, "CoT-Daten In");
				AddPlot(new Stroke(!straightenLines ? Brushes.Transparent : lineBrush, lineWidth), PlotStyle.Line, "CoT-Daten");
			}
		}
		public override string DisplayName { get { return SuriStrings.DisplayName(Name, Instrument); } }
		
		protected override void OnBarUpdate() {
			if (CotData.GetCotReportNames(Instrument.MasterInstrument.Name).Count == 0) {
				Draw.TextFixed(this, "Error", Custom.Resource.CotDataError, TextPosition.BottomRight);
				return;
			}
			if (!Globals.MarketDataOptions.DownloadCotData) {
				Draw.TextFixed(this, "Warning", Custom.Resource.CotDataWarning, TextPosition.BottomRight);
			}
			if (CotData.IsDownloadingData) {
				Draw.TextFixed(this, "Warning", Custom.Resource.CotDataStillDownloading, TextPosition.BottomRight);
				return;
			}
			
			double value = sCot.Calculate(Instrument.MasterInstrument.Name, Time[0]);
			if (!double.IsNaN(value)) {
				Value[0] = value;
				if (!straightenLines) return;
				try {
					int startIndex = -1;
					if (Math.Abs(Value[0] - Value[1]) > 0.0000001) {
						for (int i = 1; i < 100; i++) {
							if (Math.Abs(Value[i] - Value[i+1]) > 0.0000001) {
								startIndex = i;
								break;
							}
						}
					}
					if (startIndex != -1) {
						Vector2 v1 = new Vector2(startIndex, (float) Value[startIndex]);
						Vector2 v2 = new Vector2(0, (float) Value[0]);
						for (int i = startIndex; i >= 0; i--) {
							Values[1][i] = GetY(v1, v2, i);
						}
					}
				} catch (Exception) {
					// ignored
				}
			}
		}
		
		public static float GetY(Vector2 point1, Vector2 point2, float x) {
			var dx = point2.X - point1.X;
			if (dx == 0) return float.NaN;
			var m = (point2.Y - point1.Y) / dx;
			var b = point1.Y - (m * point1.X);
			return m*x + b;
		}

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
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der Linie", Order=1, GroupName="Parameter")]
		public int lineWidth
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name = "Farbe", Order = 2, GroupName = "Parameter")]
		public Brush lineBrush { get; set; }
		[Browsable(false)]
		public string lineBrushSerialize {
			get { return Serialize.BrushToString(lineBrush); }
			set { lineBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Begradige Linien", Order=3, GroupName="Parameter")]
		public bool straightenLines
		{ get; set; }
		#endregion
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
			case SuriCotReportField.TotalLong: return "Total Long";
			case SuriCotReportField.TotalShort: return "Total Short";
			case SuriCotReportField.TotalNet: return "Total Net";
			case SuriCotReportField.NonreportablePositionsLong: return "Non Reportables Long";
			case SuriCotReportField.NonreportablePositionsShort: return "Non Reportables Short";
			case SuriCotReportField.NonreportablePositionsNet: return "Non Reportables Netto";
			case SuriCotReportField.TotalTraders: return "Total Traders";
			case SuriCotReportField.TradersInNoncommercialLong: return "Trader in Non Commercials Long";
			case SuriCotReportField.TradersInNoncommercialShort: return "Trader in Non Commercials Short";
			case SuriCotReportField.TradersInNoncommercialSpreads: return "Trader in Non Commercials Spreads";
			case SuriCotReportField.TradersInCommercialLong: return "Trader in Commercials Long";
			case SuriCotReportField.TradersInCommercialShort: return "Trader in Commercial Short";
			case SuriCotReportField.TradersInTotalLong: return "Trader in Total Long";
			case SuriCotReportField.TradersInTotalShort: return "Trader in Total Short";
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
			case "Total Long": return SuriCotReportField.TotalLong;
			case "Total Short": return SuriCotReportField.TotalShort;
			case "Total Net": return SuriCotReportField.TotalNet;
			case "Non Reportables Long": return SuriCotReportField.NonreportablePositionsLong;
			case "Non Reportables Short": return SuriCotReportField.NonreportablePositionsShort;
			case "Non Reportables Netto": return SuriCotReportField.NonreportablePositionsNet;
			case "Total Traders": return SuriCotReportField.TotalTraders;
			case "Trader in Non Commercials Long": return SuriCotReportField.TradersInNoncommercialLong;
			case "Trader in Non Commercials Short": return SuriCotReportField.TradersInNoncommercialShort;
			case "Trader in Non Commercials Spreads": return SuriCotReportField.TradersInNoncommercialSpreads;
			case "Trader in Commercials Long": return SuriCotReportField.TradersInCommercialLong;
			case "Trader in Commercial Short": return SuriCotReportField.TradersInCommercialShort;
			case "Trader in Total Long": return SuriCotReportField.TradersInTotalLong;
			case "Trader in Total Short": return SuriCotReportField.TradersInTotalShort;
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
			case SuriCotReportField.TotalLong: return CotReportField.TotalLong;
			case SuriCotReportField.TotalShort: return CotReportField.TotalShort;
			case SuriCotReportField.TotalNet: return CotReportField.TotalNet;
			case SuriCotReportField.NonreportablePositionsLong: return CotReportField.NonreportablePositionsLong;
			case SuriCotReportField.NonreportablePositionsShort: return CotReportField.NonreportablePositionsShort;
			case SuriCotReportField.NonreportablePositionsNet: return CotReportField.NonreportablePositionsNet;
			case SuriCotReportField.TotalTraders: return CotReportField.TotalTraders;
			case SuriCotReportField.TradersInNoncommercialLong: return CotReportField.TradersInNoncommercialLong;
			case SuriCotReportField.TradersInNoncommercialShort: return CotReportField.TradersInNoncommercialShort;
			case SuriCotReportField.TradersInNoncommercialSpreads: return CotReportField.TradersInNoncommercialSpreads;
			case SuriCotReportField.TradersInCommercialLong: return CotReportField.TradersInCommercialLong;
			case SuriCotReportField.TradersInCommercialShort: return CotReportField.TradersInCommercialShort;
			case SuriCotReportField.TradersInTotalLong: return CotReportField.TradersInTotalLong;
			case SuriCotReportField.TradersInTotalShort: return CotReportField.TradersInTotalShort;
			default: return CotReportField.OpenInterest;
		}
	}
}

public class FriendlyEnumConverter : TypeConverter {
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
        List<string> values = new List<string>() { "Open Interest", "Non Commercials Long", "Non Commercials Short", "Non Commercials Spreads", "Non Commercials Netto", "Commercials Long", "Commercials Short", "Commercials Netto", "Total Long", "Total Short", "Total Net", "Non Reportables Long", "Non Reportables Short", "Non Reportables Netto", "Total Traders", "Trader in Non Commercials Long", "Trader in Non Commercials Short", "Trader in Non Commercials Spreads", "Trader in Commercials Long", "Trader in Commercial Short", "Trader in Total Long", "Trader in Total Short" };
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

	TotalLong,
	TotalShort,
	TotalNet,

	NonreportablePositionsLong,
	NonreportablePositionsShort,
	NonreportablePositionsNet,

	TotalTraders,
	TradersInNoncommercialLong,
	TradersInNoncommercialShort,
	TradersInNoncommercialSpreads,
	TradersInCommercialLong,
	TradersInCommercialShort,
	TradersInTotalLong,
	TradersInTotalShort,
}

#endregion


























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.CotBase[] cacheCotBase;
		public Suri.CotBase CotBase(SuriCotReportField reportField)
		{
			return CotBase(Input, reportField);
		}

		public Suri.CotBase CotBase(ISeries<double> input, SuriCotReportField reportField)
		{
			if (cacheCotBase != null)
				for (int idx = 0; idx < cacheCotBase.Length; idx++)
					if (cacheCotBase[idx] != null && cacheCotBase[idx].reportField == reportField && cacheCotBase[idx].EqualsInput(input))
						return cacheCotBase[idx];
			return CacheIndicator<Suri.CotBase>(new Suri.CotBase(){ reportField = reportField }, input, ref cacheCotBase);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.CotBase CotBase(SuriCotReportField reportField)
		{
			return indicator.CotBase(Input, reportField);
		}

		public Indicators.Suri.CotBase CotBase(ISeries<double> input , SuriCotReportField reportField)
		{
			return indicator.CotBase(input, reportField);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.CotBase CotBase(SuriCotReportField reportField)
		{
			return indicator.CotBase(Input, reportField);
		}

		public Indicators.Suri.CotBase CotBase(ISeries<double> input , SuriCotReportField reportField)
		{
			return indicator.CotBase(input, reportField);
		}
	}
}

#endregion
