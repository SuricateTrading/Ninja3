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
		[Display(Name="Jahre", Order=1, GroupName="Parameter")]
		public int years { get; set; }

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
		[Display(Name = "50% Linie", Order = 2, GroupName = "Farben")]
		public Brush brush50 { get; set; }
		[Browsable(false)]
		public string brushSerialize50 {
			get { return Serialize.BrushToString(brush50); }
			set { brush50 = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "80% Linie", Order = 3, GroupName = "Farben")]
		public Brush brush80 { get; set; }
		[Browsable(false)]
		public string brushSerialize80 {
			get { return Serialize.BrushToString(brush80); }
			set { brush80 = Serialize.StringToBrush(value); }
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
				
				brush20										= Brushes.RoyalBlue;
				brush80										= Brushes.RoyalBlue;
				brush50										= Brushes.DimGray;
				regularLineBrush							= Brushes.DarkGray;
				noNewCotBrush								= Brushes.Orange;
				lineWidth									= 2;
				lineWidthSecondary							= 1;
				years										= 4;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, plotName);
				AddPlot(new Stroke(brush20, lineWidthSecondary), PlotStyle.Line, "20%");
				AddPlot(new Stroke(brush50, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(brush80, lineWidthSecondary), PlotStyle.Line, "80%");
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
				if (CurrentBar > 10) Value[0] = Value[1];
			}
			if (cotData == null) return;
			
			SetMinMax();
			Values[1][0] = ValueOf(0.2);
			Values[2][0] = ValueOf(0.5);
			Values[3][0] = ValueOf(0.8);
			
			if ((Time[0].Date - cotData.date).TotalDays > 12) PlotBrushes[0][0] = noNewCotBrush;
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

	}
}


