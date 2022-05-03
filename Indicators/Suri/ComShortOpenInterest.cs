#region Using declarations
using System;
using System.ComponentModel;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui.NinjaScript;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class ComShortOpenInterest : Indicator {
		private SuriCotHelper suriCotHelper;
		private double min = double.MaxValue;
		private double max = double.MinValue;
		private int minIndex;
		private int maxIndex;
		private DateTime lastReportDate;
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Zeichne 20 und 80% Linien", Order=0, GroupName="Parameter")]
		public bool drawLines { get; set; }

		[NinjaScriptProperty]
		[Browsable(false)]
		[Range(1, int.MaxValue)]
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
				Description									= @"Commercials Short geteilt durch Open Interest in Prozent";
				Name										= "ComShort / OI";
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
				days										= 1000;
			} else if (State == State.Configure) {
				if (Bars.Count > 0) {
					suriCotHelper = new SuriCotHelper(Instrument, Bars.GetTime(0), Bars.LastBarTime.Date);
				}
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "Com Short / OI in %");
				if (drawLines) {
					AddPlot(new Stroke(brush20, lineWidthSecondary), PlotStyle.Line, "20%");
					AddPlot(new Stroke(brush80, lineWidthSecondary), PlotStyle.Line, "80%");
				}
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}

		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None || suriCotHelper == null) return;
			int? index = suriCotHelper.Update(Time[0]);
			if (index == null) {
				if (CurrentBar > 10) {
					Value[0] = Value[1];
				}
			} else {
				lastReportDate = Time[0];
				Values[0][0] = 100 * suriCotHelper.dbCotData[index.Value].CommercialsShort / (double) suriCotHelper.dbCotData[index.Value].OpenInterest;
			}

			if (drawLines) {
				SetMinMax();
				if (CurrentBar > days) {
					Values[1][0] = ValueOf(0.2);
					Values[2][0] = ValueOf(0.8);
				}
			}
			
			if (lastReportDate != null && (Time[0].Date - lastReportDate.Date).TotalDays > 10) {
				PlotBrushes[0][0] = noNewCotBrush;
			}
		}
		private double ValueOf(double percent) { return min + percent * (max - min); }
		
		
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


































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.ComShortOpenInterest[] cacheComShortOpenInterest;
		public Suri.ComShortOpenInterest ComShortOpenInterest(bool drawLines, int days)
		{
			return ComShortOpenInterest(Input, drawLines, days);
		}

		public Suri.ComShortOpenInterest ComShortOpenInterest(ISeries<double> input, bool drawLines, int days)
		{
			if (cacheComShortOpenInterest != null)
				for (int idx = 0; idx < cacheComShortOpenInterest.Length; idx++)
					if (cacheComShortOpenInterest[idx] != null && cacheComShortOpenInterest[idx].drawLines == drawLines && cacheComShortOpenInterest[idx].days == days && cacheComShortOpenInterest[idx].EqualsInput(input))
						return cacheComShortOpenInterest[idx];
			return CacheIndicator<Suri.ComShortOpenInterest>(new Suri.ComShortOpenInterest(){ drawLines = drawLines, days = days }, input, ref cacheComShortOpenInterest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest(bool drawLines, int days)
		{
			return indicator.ComShortOpenInterest(Input, drawLines, days);
		}

		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest(ISeries<double> input , bool drawLines, int days)
		{
			return indicator.ComShortOpenInterest(input, drawLines, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest(bool drawLines, int days)
		{
			return indicator.ComShortOpenInterest(Input, drawLines, days);
		}

		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest(ISeries<double> input , bool drawLines, int days)
		{
			return indicator.ComShortOpenInterest(input, drawLines, days);
		}
	}
}

#endregion
