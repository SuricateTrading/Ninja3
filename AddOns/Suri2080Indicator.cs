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
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public abstract class Suri2080Indicator : Indicator {
		private CotRepo cotRepo;
		private double min = double.MaxValue;
		private double max = double.MinValue;
		private DateTime? lastMinDate;
		private DateTime? lastMaxDate;
		
		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Display(Name="Jahre", Order=0, GroupName="Parameter")]
		public int years { get; set; }
		
		[NinjaScriptProperty]
		[Browsable(false)]
		public bool isDelayed
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Verschiebe Linien", Order=1, GroupName="Parameter")]
		public bool moveLines
		{ get; set; }
		
		[Range(0, 100)]
		[Display(Name="Obere Linie in %", Order=1, GroupName="Parameter")]
		public virtual int topLinePercent
		{ get; set; }
		
		[Range(0, 100)]
		[Display(Name="Untere Linie in %", Order=1, GroupName="Parameter")]
		public virtual int bottomLinePercent
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

		[XmlIgnore]
		[Display(Name = "Normale Linie", Order = 0, GroupName = "Farben")]
		public Brush regularLineBrush { get; set; }
		[Browsable(false)]
		public string regularLineBrushSerialize {
			get { return Serialize.BrushToString(regularLineBrush); }
			set { regularLineBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Untere Linie", Order = 1, GroupName = "Farben")]
		public virtual Brush bottomBrush { get; set; }
		[Browsable(false)]
		public string bottomBrushSerialize {
			get { return Serialize.BrushToString(bottomBrush); }
			set { bottomBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "50% Linie", Order = 2, GroupName = "Farben")]
		public Brush brush50 { get; set; }
		[Browsable(false)]
		public string brushSerialize50 {
			get { return Serialize.BrushToString(brush50); }
			set { brush50 = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Obere Linie", Order = 3, GroupName = "Farben")]
		public virtual Brush topBrush { get; set; }
		[Browsable(false)]
		public string topBrushSerialize {
			get { return Serialize.BrushToString(topBrush); }
			set { topBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Keine neuen COT Daten", Order = 4, GroupName = "Farben", Description = "Wird benutzt, wenn die CFTC keinen aktuellen COT Report veröffentlicht hat.")]
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

				topLinePercent								= 80;
				bottomLinePercent							= 20;
				bottomBrush									= Brushes.RoyalBlue;
				topBrush									= Brushes.RoyalBlue;
				brush50										= Brushes.DimGray;
				regularLineBrush							= Brushes.DarkGray;
				noNewCotBrush								= Brushes.Orange;
				lineWidth									= 2;
				lineWidthSecondary							= 1;
				years										= 4;
				isDelayed									= false;
				moveLines									= false;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, plotName);
				AddPlot(new Stroke(bottomBrush, lineWidthSecondary), PlotStyle.Line, bottomLinePercent + "%");
				AddPlot(new Stroke(brush50, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(topBrush, lineWidthSecondary), PlotStyle.Line, topLinePercent + "%");
				lastMinDate = null;
				lastMaxDate = null;
			} else if (State == State.DataLoaded) {
				if (Bars.Count > 0) cotRepo = new CotRepo(Instrument, Bars, false, Bars.GetTime(0).AddYears(-years).AddDays(-14));
			}
		}
		public override string DisplayName { get { return Name; } }
		private double ValueOf(double percent) { return min + percent * (max - min); }
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}
		protected abstract string plotName { get; }

		protected abstract double GetMainValue(DbCotData cotData);

		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None || cotRepo == null || cotRepo.IsEmpty()) return;
			
			DbCotData cotData = null;
			try {
				cotData = cotRepo.Get(CurrentBar);
				if (cotData == null) return;
				Value[0] = GetMainValue(cotData);
			} catch (Exception) {
				if (CurrentBar > 10) {
					Values[0][0] = Values[0][1];
					Values[1][0] = Values[1][1];
					Values[2][0] = Values[2][1];
					Values[3][0] = Values[3][1];
				}
			}
			if (cotData == null) return;
			
			SetMinMax();
			Values[1][0] = ValueOf(bottomLinePercent / 100.0);
			Values[2][0] = ValueOf(0.5);
			Values[3][0] = ValueOf(topLinePercent / 100.0);
			if (moveLines) MoveLines();
			
			if ((Time[0].Date - cotData.date).TotalDays > 12) {
				PlotBrushes[0][0] = noNewCotBrush;
				PlotBrushes[1][0] = noNewCotBrush;
				PlotBrushes[2][0] = noNewCotBrush;
				PlotBrushes[3][0] = noNewCotBrush;
			} else {
				if (SuriAddOn.license == License.Premium || SuriAddOn.license == License.Dev) {
					if (Values[0][0] > Values[3][0]) { PlotBrushes[0][0] = topBrush; }
					if (Values[0][0] < Values[1][0]) { PlotBrushes[0][0] = bottomBrush; }
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
					double cotValue = GetMainValue(cotRepo.data[i]);
					DateTime date = cotRepo.data[i].date;
					if (min > cotValue) { min = cotValue; lastMinDate = date; }
					if (max < cotValue) { max = cotValue; lastMaxDate = date; }
					if (Math.Abs((date - currentReportDate).Days / 365.0) >= years) break;
				}
			} else {
				if (min > Value[0]) { min = Value[0]; lastMinDate = currentReportDate; }
				if (max < Value[0]) { max = Value[0]; lastMaxDate = currentReportDate; }
			}
		}

		private void MoveLines() {
			double bottomLine	= ValueOf(bottomLinePercent / 100.0);
			double topLine		= ValueOf(topLinePercent    / 100.0);
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
				if (current > topLine && (prev < topLine || current > localHigh)) localHigh = current;
				if (current < bottomLine && (prev > bottomLine || current < localLow )) localLow  = current;

				if (localHigh != int.MinValue && current < topLine && prev > topLine) {
					if (lowestHigh > localHigh) lowestHigh = localHigh;
					countHigh++;
				}
				if (localLow != int.MaxValue && current > bottomLine && prev < bottomLine) {
					if (highestLow < localLow) highestLow = localLow;
					countLow++;
				}

				if (Math.Abs((cotRepo.data[i].date - cot.date).Days / 365.0) >= years) break;
			}
			
			Values[3][0] = countHigh > 1 ? lowestHigh : topLine;
			Values[1][0] = countLow  > 1 ? highestLow : bottomLine;
		}

	}
}
