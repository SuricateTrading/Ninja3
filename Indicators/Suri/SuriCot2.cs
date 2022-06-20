#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Custom.AddOns.SuriData;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriCot2 : Indicator {
		private CotRepo cotRepo;
		private double min = double.MaxValue;
		private double max = double.MinValue;
		private DateTime? lastMinDate;
		private DateTime? lastMaxDate;
		
		#region Properties
		//[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Jahre", Order=1, GroupName="Parameter")]
		public int years { get; set; }
		
		[NinjaScriptProperty]
		[Browsable(false)]
		public bool isDelayed
		{ get; set; }
		
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
		
		#region Colors
		[XmlIgnore]
		[Display(Name = "Long", Order = 0, GroupName = "Farben")]
		public Brush longBrush { get; set; }
		[Browsable(false)]
		public string longBrushSerialize {
			get { return Serialize.BrushToString(longBrush); }
			set { longBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Short", Order = 1, GroupName = "Farben")]
		public Brush shortBrush { get; set; }
		[Browsable(false)]
		public string shortBrushSerialize {
			get { return Serialize.BrushToString(shortBrush); }
			set { shortBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Normale Linie", Order = 2, GroupName = "Farben")]
		public Brush regularLineBrush { get; set; }
		[Browsable(false)]
		public string regularLineBrushSerialize {
			get { return Serialize.BrushToString(regularLineBrush); }
			set { regularLineBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "50% Linie", Order = 3, GroupName = "Farben")]
		public Brush brush50Percent { get; set; }
		[Browsable(false)]
		public string brush50PercentSerialize {
			get { return Serialize.BrushToString(brush50Percent); }
			set { brush50Percent = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Keine neuen COT Daten", Order = 5, GroupName = "Farben", Description = "Wird benutzt, wenn die CFTC keinen aktuellen COT Report veröffentlicht hat.")]
		public Brush noNewCotBrush { get; set; }
		[Browsable(false)]
		public string noNewCotBrushSerialize {
			get { return Serialize.BrushToString(noNewCotBrush); }
			set { noNewCotBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		#endregion

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"CoT 2 Commercials Short";
				Name										= "CoT 2";
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
				longBrush									= Brushes.Green;
				shortBrush									= Brushes.Red;
				brush50Percent								= Brushes.DimGray;
				regularLineBrush							= Brushes.DarkGray;
				noNewCotBrush								= Brushes.Orange;
				lineWidth									= 4;
				lineWidthSecondary							= 2;
				isDelayed									= false;
				years										= 4;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(shortBrush, lineWidthSecondary), PlotStyle.Line, "75%");
				AddPlot(new Stroke(brush50Percent, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(longBrush, lineWidthSecondary), PlotStyle.Line, "25%");
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "Com Short");
				lastMinDate = null;
				lastMaxDate = null;
			} else if (State == State.DataLoaded) {
				if (Bars.Count > 0) cotRepo = new CotRepo(Instrument, Bars, isDelayed, Bars.GetTime(0).AddYears(-years).AddDays(-14));
			}
		}

		[Browsable(false)] public Series<double> seriesMain { get { return Values[3]; } }
		[Browsable(false)] public Series<double> series25   { get { return Values[2]; } }
		[Browsable(false)] public Series<double> series50   { get { return Values[1]; } }
		[Browsable(false)] public Series<double> series75   { get { return Values[0]; } }
		public bool IsInLongHalf(int barIndex) { return Values[3].GetValueAt(barIndex) < Values[1].GetValueAt(barIndex); }
		public bool IsInShortHalf(int barIndex) { return !IsInLongHalf(barIndex); }
		private double ValueOf(double percent) { return min + percent * (max - min); }
		public override string DisplayName { get { return isDelayed ? Name + " delayed" : Name; } }

		/** Returns a position iff over 75% or under 25%. */
		public SuriPosition GetSuriPosition(int i) {
			if (seriesMain.GetValueAt(i) <= series25.GetValueAt(i)) return SuriPosition.Long;
			if (seriesMain.GetValueAt(i) >= series75.GetValueAt(i)) return SuriPosition.Short;
			return SuriPosition.None;
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}
		
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None || cotRepo == null || cotRepo.IsEmpty()) return;
			
			DbCotData cotData = null;
			try {
				cotData = cotRepo.Get(CurrentBar);
				if (cotData == null || cotData.Cot2Max == null) return;
				seriesMain[0] = cotData.commercialsShort;
				
				SetMinMax();
				series75[0] = ValueOf(0.75);
				series50[0] = ValueOf(0.5);
				series25[0] = ValueOf(0.25);
				MoveLines();
			} catch (Exception) {
				if (CurrentBar > 10) {
					series75[0] = series75[1];
					series50[0] = series50[1];
					series25[0] = series25[1];
					seriesMain[0] = seriesMain[1];
				}
			}
			if (cotData == null) return;
			
			if ((Time[0].Date - cotData.date).TotalDays > 12) {
				PlotBrushes[0][0] = noNewCotBrush;
				PlotBrushes[1][0] = noNewCotBrush;
				PlotBrushes[2][0] = noNewCotBrush;
				PlotBrushes[3][0] = noNewCotBrush;
			} else {
				if (SuriAddOn.license == License.Premium || SuriAddOn.license == License.Dev) {
					if (seriesMain[0] > series75[0]) { PlotBrushes[3][0] = shortBrush; }
					if (seriesMain[0] < series25[0]) { PlotBrushes[3][0] = longBrush; }
				}
			}
		}
		
		private void SetMinMax() {
			int currentCotIndex = cotRepo.CotIndexOf(CurrentBar);
			DateTime currentReportDate = cotRepo.data[currentCotIndex].date;
			
			if (lastMinDate == null || lastMaxDate == null ||
			    Math.Abs((lastMinDate.Value - currentReportDate).Days / 365.0) >= years ||
			    Math.Abs((lastMaxDate.Value - currentReportDate).Days / 365.0) >= years
			) {
				// the last max or min is too far away. Recalculate.
				min = double.MaxValue;
				max = double.MinValue;
				for (int i = currentCotIndex; i >= 0; i--) {
					int cotValue = cotRepo.data[i].commercialsShort;
					DateTime date = cotRepo.data[i].date;
					if (min > cotValue) { min = cotValue; lastMinDate = date; }
					if (max < cotValue) { max = cotValue; lastMaxDate = date; }
					if (Math.Abs((date - currentReportDate).Days / 365.0) >= years) break;
				}
			} else {
				if (min > seriesMain[0]) { min = seriesMain[0]; lastMinDate = currentReportDate; }
				if (max < seriesMain[0]) { max = seriesMain[0]; lastMaxDate = currentReportDate; }
			}
		}

		private void MoveLines() {
			double line25 = ValueOf(0.25);
			double line75 = ValueOf(0.75);
			int localHigh  = int.MaxValue;
			int localLow   = int.MaxValue;
			int highestLow = int.MinValue;
			int lowestHigh = int.MaxValue;
			int countHigh = 0;
			int countLow = 0;

			int currentCotIndex = cotRepo.CotIndexOf(CurrentBar);
			var cot = cotRepo.Get(CurrentBar);
			for (int i = currentCotIndex - 1; i >= 0; i--) {
				var current = cotRepo.data[i].commercialsShort;
				var prev = cotRepo.data[i + 1].commercialsShort;
				if (current > line75 && (prev < line75 || current > localHigh)) localHigh = current;
				if (current < line25 && (prev > line25 || current < localLow )) localLow  = current;

				if (localHigh != int.MinValue && current < line75 && prev > line75) {
					if (lowestHigh > localHigh) lowestHigh = localHigh;
					countHigh++;
				}
				if (localLow != int.MaxValue && current > line25 && prev < line25) {
					if (highestLow < localLow) highestLow = localLow;
					countLow++;
				}

				if (Math.Abs((cotRepo.data[i].date - cot.date).Days / 365.0) >= years) break;
			}
				
			series75[0] = countHigh > 1 ? lowestHigh : line75;
			series25[0] = countLow  > 1 ? highestLow : line25;
		}
		
	}
}






























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriCot2[] cacheSuriCot2;
		public Suri.SuriCot2 SuriCot2(bool isDelayed)
		{
			return SuriCot2(Input, isDelayed);
		}

		public Suri.SuriCot2 SuriCot2(ISeries<double> input, bool isDelayed)
		{
			if (cacheSuriCot2 != null)
				for (int idx = 0; idx < cacheSuriCot2.Length; idx++)
					if (cacheSuriCot2[idx] != null && cacheSuriCot2[idx].isDelayed == isDelayed && cacheSuriCot2[idx].EqualsInput(input))
						return cacheSuriCot2[idx];
			return CacheIndicator<Suri.SuriCot2>(new Suri.SuriCot2(){ isDelayed = isDelayed }, input, ref cacheSuriCot2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCot2 SuriCot2(bool isDelayed)
		{
			return indicator.SuriCot2(Input, isDelayed);
		}

		public Indicators.Suri.SuriCot2 SuriCot2(ISeries<double> input , bool isDelayed)
		{
			return indicator.SuriCot2(input, isDelayed);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCot2 SuriCot2(bool isDelayed)
		{
			return indicator.SuriCot2(Input, isDelayed);
		}

		public Indicators.Suri.SuriCot2 SuriCot2(ISeries<double> input , bool isDelayed)
		{
			return indicator.SuriCot2(input, isDelayed);
		}
	}
}

#endregion
