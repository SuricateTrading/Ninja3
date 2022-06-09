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
		#region Properties
		
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
			} else if (State == State.Configure) {
				AddPlot(new Stroke(shortBrush, lineWidthSecondary), PlotStyle.Line, "75%");
				AddPlot(new Stroke(brush50Percent, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(longBrush, lineWidthSecondary), PlotStyle.Line, "25%");
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "Com Short");
			} else if (State == State.DataLoaded) {
				if (Bars.Count > 0) cotRepo = new CotRepo(Instrument, Bars, isDelayed);
			}
		}

		public Series<double> seriesMain { get { return Values[3]; } }
		public Series<double> series25   { get { return Values[2]; } }
		public Series<double> series50   { get { return Values[1]; } }
		public Series<double> series75   { get { return Values[0]; } }
		public bool IsInLongHalf(int barIndex) { return Values[3].GetValueAt(barIndex) < Values[1].GetValueAt(barIndex); }
		public bool IsInShortHalf(int barIndex) { return !IsInLongHalf(barIndex); }

		/** Returns a position iff over 75% or under 25%. */
		public SuriPosition GetSuriPosition(int i) {
			if (seriesMain.GetValueAt(i) <= series25.GetValueAt(i)) return SuriPosition.Long;
			if (seriesMain.GetValueAt(i) >= series75.GetValueAt(i)) return SuriPosition.Short;
			return SuriPosition.None;
		}

		public override string DisplayName { get { return isDelayed ? Name + " delayed" : Name; } }

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
				Values[0][0] = cotData.Cot2Max.Value;
				Values[1][0] = cotData.Cot2Mid.Value;
				Values[2][0] = cotData.Cot2Min.Value;
				Values[3][0] = cotData.commercialsShort;
			} catch (Exception) {
				if (CurrentBar > 10) {
					Values[0][0] = Values[0][1];
					Values[1][0] = Values[1][1];
					Values[2][0] = Values[2][1];
					Values[3][0] = Values[3][1];
				}
			}
			if (cotData == null) return;
			
			if ((Time[0].Date - cotData.date).TotalDays > 12) {
				PlotBrushes[0][0] = noNewCotBrush;
				PlotBrushes[1][0] = noNewCotBrush;
				PlotBrushes[2][0] = noNewCotBrush;
				PlotBrushes[3][0] = noNewCotBrush;
			} else {
				Analyze();
			}
		}
		
		private void Analyze() {
			if (SuriAddOn.license == License.Basic) return;
			if (Values[3][0] > Values[0][0]) {
				PlotBrushes[3][0] = shortBrush;
			}
			if (Values[3][0] < Values[2][0]) {
				PlotBrushes[3][0] = longBrush;
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
