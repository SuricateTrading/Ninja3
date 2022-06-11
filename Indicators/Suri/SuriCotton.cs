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

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriCotton : Indicator {
		private CottonRepo cottonRepo;
		
		#region Indicator
		[Display(Name = "Zeige Käufe", Order = 0, GroupName = "Parameter")]
		public bool showPurchases { get; set; }
		[Display(Name = "Zeige Verkäufe", Order = 1, GroupName = "Parameter")]
		public bool showSales { get; set; }
		[Display(Name = "Zeige Open Interest", Order = 2, GroupName = "Parameter")]
		public bool showOi { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Farbe Käufe", Order = 0, GroupName = "Farben")]
		public Brush purchaseLineBrush { get; set; }
		[Browsable(false)]
		public string purchaseLineBrushSerialize {
			get { return Serialize.BrushToString(purchaseLineBrush); }
			set { purchaseLineBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Farbe Verkäufe", Order = 1, GroupName = "Farben")]
		public Brush saleLineBrush { get; set; }
		[Browsable(false)]
		public string saleLineBrushSerialize {
			get { return Serialize.BrushToString(saleLineBrush); }
			set { saleLineBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Farbe Open Interest auf der ICE", Order = 2, GroupName = "Farben")]
		public Brush oiLineBrush { get; set; }
		[Browsable(false)]
		public string oiLineBrushSerialize {
			get { return Serialize.BrushToString(oiLineBrush); }
			set { oiLineBrush = Serialize.StringToBrush(value); }
		}
		#endregion

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Cotton On Call";
				Name										= "Baumwolle";
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

				showSales									= true;
				showPurchases								= true;
				showOi										= false;
				purchaseLineBrush							= Brushes.Green;
				saleLineBrush								= Brushes.Red;
				oiLineBrush									= Brushes.Orange;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(showPurchases ? purchaseLineBrush : Brushes.Transparent, 2), PlotStyle.Line, "Käufe");
				AddPlot(new Stroke(showSales ? saleLineBrush : Brushes.Transparent, 2), PlotStyle.Line, "Verkäufe");
				AddPlot(new Stroke(showOi ? oiLineBrush : Brushes.Transparent, 2), PlotStyle.Line, "Open Interest");
			} else if (State == State.DataLoaded) {
				if (Bars.Count > 0 && SuriStrings.GetComm(Instrument) == Commodity.Cotton) cottonRepo = new CottonRepo(Instrument, Bars);
			}
		}
		public override string DisplayName { get { return Name + (showPurchases ? " Käufe" : "") + (showSales ? " Verkäufe" : "") + (showOi ? " Open Interest" : ""); } }
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        base.OnRender(chartControl, chartScale);
	        if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
        }

		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None || cottonRepo == null || cottonRepo.IsEmpty()) return;
			if (!(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Day && Bars.BarsPeriod.Value == 1 || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && Bars.BarsPeriod.Value == 1440)) {
				Draw.TextFixed(this, "Warning", "Coton ist nur für ein 1-Tages Chart oder 1440-Minuten Chart verfügbar.", TextPosition.Center);
				return;
			}

			try {
				CottonData cottonData = cottonRepo.Get(CurrentBar);
				if (cottonData == null) return;
				Values[0][0] = cottonData.purchases;
				Values[1][0] = cottonData.sales;
				Values[2][0] = cottonData.openInterest;
			} catch (Exception) {
				if (CurrentBar > 10) Value[0] = Value[1];
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
		private Suri.SuriCotton[] cacheSuriCotton;
		public Suri.SuriCotton SuriCotton()
		{
			return SuriCotton(Input);
		}

		public Suri.SuriCotton SuriCotton(ISeries<double> input)
		{
			if (cacheSuriCotton != null)
				for (int idx = 0; idx < cacheSuriCotton.Length; idx++)
					if (cacheSuriCotton[idx] != null &&  cacheSuriCotton[idx].EqualsInput(input))
						return cacheSuriCotton[idx];
			return CacheIndicator<Suri.SuriCotton>(new Suri.SuriCotton(), input, ref cacheSuriCotton);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCotton SuriCotton()
		{
			return indicator.SuriCotton(Input);
		}

		public Indicators.Suri.SuriCotton SuriCotton(ISeries<double> input )
		{
			return indicator.SuriCotton(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCotton SuriCotton()
		{
			return indicator.SuriCotton(Input);
		}

		public Indicators.Suri.SuriCotton SuriCotton(ISeries<double> input )
		{
			return indicator.SuriCotton(input);
		}
	}
}

#endregion
