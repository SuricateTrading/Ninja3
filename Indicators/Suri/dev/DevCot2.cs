#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;

#endregion
/*
namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public sealed class DevCot2 : Indicator {
		private DevCot suriCotData;
		private double min = double.MaxValue;
		private double max = double.MinValue;
		private int minIndex;
		private int maxIndex;

		#region Properties
		[NinjaScriptProperty]
		[Browsable(false)]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
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
		[Display(Name = "Farbe wenn noch nicht genügend Daten", Order = 4, GroupName = "Farben", Description = "CoT 2 braucht normalerweise ungefähr 4 Jahre, bis es korrekt angezeigt werden kann. Wenn noch nicht 4 Jahre geladen sind, wird diese Farbe benutzt.")]
		public Brush notReadyBrush { get; set; }
		[Browsable(false)]
		public string notReadyBrushSerialize {
			get { return Serialize.BrushToString(notReadyBrush); }
			set { notReadyBrush = Serialize.StringToBrush(value); }
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
				Name										= "Dev CoT 2";
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
				longBrush									= Brushes.Green;
				shortBrush									= Brushes.Red;
				brush50Percent								= Brushes.DimGray;
				regularLineBrush							= Brushes.DarkGray;
				notReadyBrush								= Brushes.Transparent;
				noNewCotBrush								= Brushes.Orange;
				lineWidth									= 4;
				lineWidthSecondary							= 2;
				days										= 500;
				suriCotData = DevCot(SuriCotReportField.CommercialShort);
			} else if (State == State.Configure) {
				AddPlot(new Stroke(shortBrush, lineWidthSecondary), PlotStyle.Line, "75%");
				AddPlot(new Stroke(brush50Percent, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(longBrush, lineWidthSecondary), PlotStyle.Line, "25%");
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "Com Short");
			}
		}
		public override string DisplayName { get { return Name; } }
		private double ValueOf(double percent) { return min + percent * (max - min); }
		
		private int noNewCotSince;
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None) return;

			Values[3][0] = suriCotData[0];
			SetMinMax();
			Values[0][0] = ValueOf(0.75);
			Values[1][0] = ValueOf(0.5);
			Values[2][0] = ValueOf(0.25);
			MoveLines();
			Analyze();

			if (CurrentBar > 0 && Math.Abs(Values[3][0] - Values[3][1]) < 0.00000000001) {
				noNewCotSince++;
			} else {
				noNewCotSince = 0;
			}
			if (noNewCotSince > 12) {
				PlotBrushes[3][0] = noNewCotBrush;
			}
		}

		private void SetMinMax() {
			if (min > suriCotData.Value[0]) { min = suriCotData.Value[0]; minIndex = CurrentBar; }
			if (max < suriCotData.Value[0]) { max = suriCotData.Value[0]; maxIndex = CurrentBar; }
			
			if (CurrentBar - maxIndex > days || CurrentBar - minIndex > days) {
				// the last max or min is too far away. Recalculate.
				min = double.MaxValue;
				max = double.MinValue;
				for (int i = 0; i < days; i++) {
					if (min > suriCotData.Value[i]) { min = suriCotData.Value[i]; minIndex = CurrentBar-i; }
					if (max < suriCotData.Value[i]) { max = suriCotData.Value[i]; maxIndex = CurrentBar-i; }
				}
			}
		}

		private void MoveLines() {
			if (CurrentBar <= days) return;
			
			double line25 = ValueOf(0.25);
			double line75 = ValueOf(0.75);
			double? localHigh = null;
			double lowestHigh = double.MaxValue;
			double? localLow = null;
			double highestLow = double.MinValue;
			int countHigh = 0;
			int countLow = 0;

			for (int i = days; i >= 0; i--) {
				if (Values[3][i] > line75 && (Values[3][i+1] < line75 || Values[3][i] > localHigh)) localHigh = Values[3][i];
				if (Values[3][i] < line25 && (Values[3][i+1] > line25 || Values[3][i] < localLow))  localLow  = Values[3][i];
				
				if (localHigh!=null && Values[3][i] < line75 && Values[3][i+1] > line75) {
					if (localHigh < lowestHigh) lowestHigh = localHigh.Value;
					countHigh++;
				}
				if (localLow!=null && Values[3][i] > line25 && Values[3][i+1] < line25) {
					if (highestLow < localLow) highestLow = localLow.Value;
					countLow++;
				}
			}
			
			Values[0][0] = countHigh > 1 ? lowestHigh : line75;
			Values[2][0] = countLow  > 1 ? highestLow : line25;
		}
		
		private void Analyze() {
			if (SuriAddOn.license == License.Basic) return;
			
			if (CurrentBar <= days) {
				PlotBrushes[0][0] = notReadyBrush;
				PlotBrushes[1][0] = notReadyBrush;
				PlotBrushes[2][0] = notReadyBrush;
				return;
			}
			if (Values[3][0] > Values[0][0]) {
				PlotBrushes[3][0] = shortBrush;
			}
			if (Values[3][0] < Values[2][0]) {
				PlotBrushes[3][0] = longBrush;
			}
		}
		
	}
}
*/
