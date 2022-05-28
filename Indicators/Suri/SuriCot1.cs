#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Custom.AddOns.SuriData;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using Brush = System.Windows.Media.Brush;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriCot1 : Indicator {
		private CotRepo cotRepo;
		private SuriSma suriSma;
		private SessionIterator sessionIterator;
		private DateTime lastReportDate;
		
		private bool isCurrentlyASignal;
		public readonly List<int> signalIndices = new List<int>();

		#region Indicator
		//[NinjaScriptProperty]
		[Browsable(false)]
		//[Display(Name="Benutze Wochen (oder Tage)", Order=1, GroupName="Parameter")]
		public bool useWeeks
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
		[Display(Name = "SMA passt nicht", Order = 4, GroupName = "Farben")]
		public Brush noSignalBrush { get; set; }
		[Browsable(false)]
		public string noSignalBrushSerialize {
			get { return Serialize.BrushToString(noSignalBrush); }
			set { noSignalBrush = Serialize.StringToBrush(value); }
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
				Description									= @"CoT 1 Commercials Netto Oszillator 125 Tage";
				Name										= "CoT 1";
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
				noSignalBrush								= Brushes.Yellow;
				regularLineBrush							= Brushes.DarkGray;
				noNewCotBrush								= Brushes.Orange;
				lineWidth									= 4;
				lineWidthSecondary							= 2;
				//useWeeks									= true;
			} else if (State == State.Configure) {
				suriSma = SuriSma(125);
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "COT1");
				AddPlot(new Stroke(shortBrush, lineWidthSecondary), PlotStyle.Line, "10%");
				AddPlot(new Stroke(brush50Percent, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(longBrush, lineWidthSecondary), PlotStyle.Line, "90%");
			} else if (State == State.DataLoaded) {
				if (Bars.Count > 0) cotRepo = new CotRepo(Instrument, Bars);
				sessionIterator = new SessionIterator(Bars);
			}
		}
		public override string DisplayName { get { return Name; } }
        public override void OnCalculateMinMax() {
	        MinValue = 0;
	        MaxValue = 100;
        }
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        base.OnRender(chartControl, chartScale);
	        if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
        }

		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None || cotRepo == null) return;
			if (!(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Day && Bars.BarsPeriod.Value == 1 || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && Bars.BarsPeriod.Value == 1440)) {
				Draw.TextFixed(this, "Warning", "CoT 1 ist nur für ein 1-Tages Chart oder 1440-Minuten Chart verfügbar.", TextPosition.Center);
				return;
			}
			
			try {
				DbCotData cotData = cotRepo.Get(CurrentBar);
				lastReportDate = Time[0];
				if (cotData.Cot1 == null) return;
				Values[0][0] = cotData.Cot1.Value;
			} catch (IndexOutOfRangeException) {
				if (CurrentBar > 10) Value[0] = Value[1];
			}

			Values[1][0] = 10;
			Values[2][0] = 50;
			Values[3][0] = 90;

			if (lastReportDate != null && (Time[0].Date - lastReportDate.Date).TotalDays > 10) {
				PlotBrushes[0][0] = noNewCotBrush;
			} else if (SuriAddOn.license != License.Basic) {
				if (!isCurrentlyASignal || Value[0] < 90 && Value[0] > 10 ) {
					isCurrentlyASignal = IsSignal();
				}
				if (isCurrentlyASignal) {
					if      (suriSma[0] > suriSma[1] && Value[0] >= 90) PlotBrushes[0][0] = longBrush;
					else if (suriSma[0] < suriSma[1] && Value[0] <= 10) PlotBrushes[0][0] = shortBrush;
					else PlotBrushes[0][0] = noSignalBrush;
				}
			}
		}
		
		private bool IsSignal() {
			if (CurrentBar == 0) return false;
			if (Value[0] < 90 && Value[0] > 10) return false;
			if ((Value[1] > 10 && Value[0] <= 10 || Value[1] < 90 && Value[0] >= 90) == false) return false;
			
			// check if we come from the other side
			for (int i = 2; i <= CurrentBar-1; i++) {
				if (Value[i] <= 10 && Value[i - 1] > 10 || Value[i] >= 90 && Value[i - 1] < 90) {
					bool isSignal = IsValidDataPoint(i) && Math.Abs(Value[i] - Value[0]) >= 80;
					if (isSignal && (suriSma[0] > suriSma[1] && Value[0] >= 90 || suriSma[0] < suriSma[1] && Value[0] <= 10)) {
						signalIndices.Add(CurrentBar);
					}
					return isSignal;
				}
			}
			return false;
		}


	}
}


























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriCot1[] cacheSuriCot1;
		public Suri.SuriCot1 SuriCot1()
		{
			return SuriCot1(Input);
		}

		public Suri.SuriCot1 SuriCot1(ISeries<double> input)
		{
			if (cacheSuriCot1 != null)
				for (int idx = 0; idx < cacheSuriCot1.Length; idx++)
					if (cacheSuriCot1[idx] != null &&  cacheSuriCot1[idx].EqualsInput(input))
						return cacheSuriCot1[idx];
			return CacheIndicator<Suri.SuriCot1>(new Suri.SuriCot1(), input, ref cacheSuriCot1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCot1 SuriCot1()
		{
			return indicator.SuriCot1(Input);
		}

		public Indicators.Suri.SuriCot1 SuriCot1(ISeries<double> input )
		{
			return indicator.SuriCot1(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCot1 SuriCot1()
		{
			return indicator.SuriCot1(Input);
		}

		public Indicators.Suri.SuriCot1 SuriCot1(ISeries<double> input )
		{
			return indicator.SuriCot1(input);
		}
	}
}

#endregion
