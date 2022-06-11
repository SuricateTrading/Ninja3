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

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriFight : Indicator {
		private SuriBarRange range;
		
		#region Properties
		[NinjaScriptProperty]
		[Browsable(false)]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=0, GroupName="Parameter")]
		public int days { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Bar Farbe", Order = 1, GroupName = "Parameter")]
		public Brush barBrush { get; set; }
		[Browsable(false)]
		public string barBrushSerialize {
			get { return Serialize.BrushToString(barBrush); }
			set { barBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Volumen / Bar Range";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				barBrush									= Brushes.RoyalBlue;
				days										= 125;
			} else if (State == State.Configure) {
				range = SuriBarRange(false, days);
				AddPlot(new Stroke(barBrush, 2), PlotStyle.Bar, "Volumen / Bar Range");
			}
		}
		public override string DisplayName { get { return Name; } }
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}

		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None) return;
			Value[0] = Volume[0] / range[0];
		}
		
	}
}











































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriFight[] cacheSuriFight;
		public Suri.SuriFight SuriFight(int days)
		{
			return SuriFight(Input, days);
		}

		public Suri.SuriFight SuriFight(ISeries<double> input, int days)
		{
			if (cacheSuriFight != null)
				for (int idx = 0; idx < cacheSuriFight.Length; idx++)
					if (cacheSuriFight[idx] != null && cacheSuriFight[idx].days == days && cacheSuriFight[idx].EqualsInput(input))
						return cacheSuriFight[idx];
			return CacheIndicator<Suri.SuriFight>(new Suri.SuriFight(){ days = days }, input, ref cacheSuriFight);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriFight SuriFight(int days)
		{
			return indicator.SuriFight(Input, days);
		}

		public Indicators.Suri.SuriFight SuriFight(ISeries<double> input , int days)
		{
			return indicator.SuriFight(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriFight SuriFight(int days)
		{
			return indicator.SuriFight(Input, days);
		}

		public Indicators.Suri.SuriFight SuriFight(ISeries<double> input , int days)
		{
			return indicator.SuriFight(input, days);
		}
	}
}

#endregion
