#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Core;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public sealed class DevCot : Indicator {
		private CotReport sCot;
		private double min = double.MaxValue;
		private double max = double.MinValue;
		private int minIndex;
		private int maxIndex;
		
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
				Name										= "Dev CoT-Daten";
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
				
				drawLines									= true;
				brush20										= Brushes.RoyalBlue;
				brush80										= Brushes.RoyalBlue;
				regularLineBrush							= Brushes.DarkGray;
				noNewCotBrush								= Brushes.Orange;
				lineWidth									= 2;
				lineWidthSecondary							= 1;
				days										= 1000;
				reportField									= SuriCotReportField.CommercialLong;
			} else if (State == State.Configure) {
				sCot = new CotReport { ReportType = CotReportType.Futures, Field = CotReportMaper.SuriToCotReport(reportField) };
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "CoT-Daten");
				
				if (drawLines) {
					AddPlot(new Stroke(brush20, lineWidthSecondary), PlotStyle.Line, "20%");
					AddPlot(new Stroke(brush80, lineWidthSecondary), PlotStyle.Line, "80%");
				}
			}
		}
		public override string DisplayName { get { return Name; } }
		private double ValueOf(double percent) { return min + percent * (max - min); }

		/// Returns a list of CoT values starting with the given *start* timestamp. The Count of the list is equal to the given *bars* + 1.
		public List<double> GetRange(DateTime start, int bars, NinjaScriptBase ninjaScriptBase) {
			List<double> value = new List<double>();
			DateTime current = start;
			value.Add(sCot.Calculate(ninjaScriptBase.Instrument.MasterInstrument.Name, current));
			for (int i = 0; i < bars; i++) {
				// find previous trading date
				bool isHoliday;
				do {
					isHoliday = false;
					current = current.AddDays(-1);
					foreach (var pair in TradingHours.Holidays) {
						Print(pair.Key.Date);
						if (pair.Key.Date == current.Date) {
							isHoliday = true;
							break;
						}
					}
				} while (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday || isHoliday);
				value.Add(sCot.Calculate(ninjaScriptBase.Instrument.MasterInstrument.Name, current));
				//Print(current + " " + value.Last());
			}
			return value;
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}

		private int noNewCotSince;
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None) return;
			
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

			Value[0] = sCot.Calculate(Instrument.MasterInstrument.Name, Time[0]);
			if (drawLines) {
				SetMinMax();
				if (CurrentBar >= days) {
					Values[1][0] = ValueOf(0.2);
					Values[2][0] = ValueOf(0.8);
				}
			}
			
			if (CurrentBar > 0 && Math.Abs(Value[0] - Value[1]) < 0.00000000001) {
				noNewCotSince++;
			} else {
				noNewCotSince = 0;
			}
			if (noNewCotSince > 12) {
				PlotBrushes[0][0] = noNewCotBrush;
			}
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

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.DevCot[] cacheDevCot;
		public Suri.dev.DevCot DevCot(SuriCotReportField reportField)
		{
			return DevCot(Input, reportField);
		}

		public Suri.dev.DevCot DevCot(ISeries<double> input, SuriCotReportField reportField)
		{
			if (cacheDevCot != null)
				for (int idx = 0; idx < cacheDevCot.Length; idx++)
					if (cacheDevCot[idx] != null && cacheDevCot[idx].reportField == reportField && cacheDevCot[idx].EqualsInput(input))
						return cacheDevCot[idx];
			return CacheIndicator<Suri.dev.DevCot>(new Suri.dev.DevCot(){ reportField = reportField }, input, ref cacheDevCot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevCot DevCot(SuriCotReportField reportField)
		{
			return indicator.DevCot(Input, reportField);
		}

		public Indicators.Suri.dev.DevCot DevCot(ISeries<double> input , SuriCotReportField reportField)
		{
			return indicator.DevCot(input, reportField);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevCot DevCot(SuriCotReportField reportField)
		{
			return indicator.DevCot(Input, reportField);
		}

		public Indicators.Suri.dev.DevCot DevCot(ISeries<double> input , SuriCotReportField reportField)
		{
			return indicator.DevCot(input, reportField);
		}
	}
}

#endregion
