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
		
		private bool isCurrentlyASignal;
		public readonly List<int> signalIndices = new List<int>();

		#region Indicator
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Wochen", Order=1, GroupName="Parameter")]
		public int weeks
		{ get; set; }
		
		[NinjaScriptProperty]
		[Browsable(false)]
		public bool isDelayed
		{ get; set; }
		
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der Hauptlinie", Order=2, GroupName="Parameter")]
		public int lineWidth
		{ get; set; }
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
		[XmlIgnore]
		[Display(Name = "SMA noch nicht bereit", Order = 5, GroupName = "Farben", Description = "Der SMA braucht eine gewisse Menge an Daten, bis er richtig berechnet ist. Normalerweise 125 Bars.")]
		public Brush smaNotReadyBrush { get; set; }
		[Browsable(false)]
		public string smaNotReadyBrushSerialize {
			get { return Serialize.BrushToString(smaNotReadyBrush); }
			set { smaNotReadyBrush = Serialize.StringToBrush(value); }
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
				smaNotReadyBrush							= Brushes.CornflowerBlue;
				lineWidth									= 4;
				lineWidthSecondary							= 2;
				weeks										= 26;
				isDelayed									= false;
			} else if (State == State.Configure) {
				suriSma = SuriSma(125 * weeks / 26);
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "COT1");
				AddPlot(new Stroke(shortBrush, lineWidthSecondary), PlotStyle.Line, "10%");
				AddPlot(new Stroke(brush50Percent, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(longBrush, lineWidthSecondary), PlotStyle.Line, "90%");
			} else if (State == State.DataLoaded) {
				if (Bars.Count > 0) cotRepo = new CotRepo(Instrument, Bars, isDelayed, Bars.GetTime(0).AddDays(- weeks * 7 - 14));
			}
		}
		public override string DisplayName { get { return isDelayed ? Name + " delayed" : Name; } }
        public override void OnCalculateMinMax() {
	        MinValue = 0;
	        MaxValue = 100;
        }
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        base.OnRender(chartControl, chartScale);
	        if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
        }

		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None || cotRepo == null || cotRepo.IsEmpty()) return;
			if (!(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Day && Bars.BarsPeriod.Value == 1 || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && Bars.BarsPeriod.Value == 1440)) {
				Draw.TextFixed(this, "Warning", "CoT 1 ist nur für ein 1-Tages Chart oder 1440-Minuten Chart verfügbar.", TextPosition.Center);
				return;
			}

			DbCotData cotData = null;
			try {
				cotData = cotRepo.Get(CurrentBar);
				if (cotData == null) return;
				int cotIndex = cotRepo.DataIndexOf(CurrentBar);
				Values[0][0] = DataTools.GetOsci(
					cotIndex,
					i => cotRepo.data[i].commercialsLong - cotRepo.data[i].commercialsShort,
					i => Math.Abs((cotRepo.data[i].date - cotRepo.data[cotIndex].date).Days) >= 7 * weeks
				);
			} catch (Exception) {
				if (CurrentBar > 10) Value[0] = Value[1];
			}
			if (cotData == null) return;

			Values[1][0] = 10;
			Values[2][0] = 50;
			Values[3][0] = 90;

			if ((Time[0].Date - cotData.date).TotalDays > 12) {
				PlotBrushes[0][0] = noNewCotBrush;
			} else if (SuriAddOn.license != License.Basic) {
				if (!isCurrentlyASignal || Value[0] < 90 && Value[0] > 10 ) {
					isCurrentlyASignal = StrategyTasks.ComesFromOtherSide(CurrentBar, i => Value.GetValueAt(i), 10, 90);
				}
				if (isCurrentlyASignal) {
					if (isDelayed) {
						// todo
						if (CurrentBar >= 15) {
							//int smaBarsAgo = 0;
							var reportDate = cotRepo.Get(CurrentBar).date;
							for (int j = 0; j < 15; j++) {
								if (Time[j].Date == reportDate.Date) {
									//smaBarsAgo = j;
									break;
								}
								if (j == 14) Print("Warning: Could not find reportDate for SMA.");
							}
						}
					}

					if (CurrentBar < 125 * weeks / 26) {
						PlotBrushes[0][0] = smaNotReadyBrush;
					} else {
						if (suriSma[0] > suriSma[1] && Value[0] >= 90) {
							PlotBrushes[0][0] = longBrush;
							signalIndices.Add(CurrentBar);
						} else if (suriSma[0] < suriSma[1] && Value[0] <= 10) {
							PlotBrushes[0][0] = shortBrush;
							signalIndices.Add(CurrentBar);
						} else {
							PlotBrushes[0][0] = noSignalBrush;
						}
					}
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
		private Suri.SuriCot1[] cacheSuriCot1;
		public Suri.SuriCot1 SuriCot1(int weeks, bool isDelayed)
		{
			return SuriCot1(Input, weeks, isDelayed);
		}

		public Suri.SuriCot1 SuriCot1(ISeries<double> input, int weeks, bool isDelayed)
		{
			if (cacheSuriCot1 != null)
				for (int idx = 0; idx < cacheSuriCot1.Length; idx++)
					if (cacheSuriCot1[idx] != null && cacheSuriCot1[idx].weeks == weeks && cacheSuriCot1[idx].isDelayed == isDelayed && cacheSuriCot1[idx].EqualsInput(input))
						return cacheSuriCot1[idx];
			return CacheIndicator<Suri.SuriCot1>(new Suri.SuriCot1(){ weeks = weeks, isDelayed = isDelayed }, input, ref cacheSuriCot1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCot1 SuriCot1(int weeks, bool isDelayed)
		{
			return indicator.SuriCot1(Input, weeks, isDelayed);
		}

		public Indicators.Suri.SuriCot1 SuriCot1(ISeries<double> input , int weeks, bool isDelayed)
		{
			return indicator.SuriCot1(input, weeks, isDelayed);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCot1 SuriCot1(int weeks, bool isDelayed)
		{
			return indicator.SuriCot1(Input, weeks, isDelayed);
		}

		public Indicators.Suri.SuriCot1 SuriCot1(ISeries<double> input , int weeks, bool isDelayed)
		{
			return indicator.SuriCot1(input, weeks, isDelayed);
		}
	}
}

#endregion
