#region Using declarations
using System;
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

namespace NinjaTrader.NinjaScript.Indicators.Suri.Reports {
	public sealed class SuriNgas : Indicator {
		private NgasRepo ngasRepo;
		
		#region Indicator
		[XmlIgnore]
		[Display(Name = "Farbe", Order = 0, GroupName = "Farben")]
		public Brush lineBrush { get; set; }
		[Browsable(false)]
		public string lineBrushSerialize {
			get { return Serialize.BrushToString(lineBrush); }
			set { lineBrush = Serialize.StringToBrush(value); }
		}
		#endregion

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Erdgas Lagerbestand";
				Name										= "Erdgas";
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
				lineBrush									= Brushes.Green;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(lineBrush, 2), PlotStyle.Line, "Lagerbestand");
			} else if (State == State.DataLoaded) {
				if (Bars.Count > 0 && SuriStrings.GetComm(Instrument) == Commodity.NaturalGas) ngasRepo = new NgasRepo(Instrument, Bars);
			}
		}
		public override string DisplayName { get { return Name; } }
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        base.OnRender(chartControl, chartScale);
	        if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
        }

		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None || ngasRepo == null || ngasRepo.IsEmpty()) return;
			if (!(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Day && Bars.BarsPeriod.Value == 1 || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && Bars.BarsPeriod.Value == 1440)) {
				Draw.TextFixed(this, "Warning", "Erdgas ist nur für ein 1-Tages Chart oder 1440-Minuten Chart verfügbar.", TextPosition.Center);
				return;
			}

			try {
				NgasData ngasData = ngasRepo.Get(CurrentBar);
				if (ngasData == null) return;
				Values[0][0] = ngasData.total;
			} catch (Exception) {
				if (CurrentBar > 10) Value[0] = Value[1];
			}
		}
	}
}


























//
