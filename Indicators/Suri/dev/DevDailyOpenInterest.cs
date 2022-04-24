#region Using declarations

using System.Collections.Generic;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public sealed class DevDailyOpenInterest : Indicator {
		private List<TkData> tkData;
		private int nextIndex;
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"DevDailyOpenInterest";
				Name										= "DevDailyOpenInterest";
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
			} else if (State == State.Configure) {
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Line, "Volumen");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "Open Interest");
				//AddPlot(new Stroke(Brushes.DarkGray, 2), PlotStyle.Line, "Delta");
				/*AddLine(new Stroke(Brushes.DimGray, 1), 10, "0");
				AddLine(new Stroke(Brushes.DimGray, 1), 50, "50");
				AddLine(new Stroke(Brushes.DimGray, 1), 90, "100");*/
			} else if (State == State.DataLoaded) {
				int? id = SuriStrings.GetId(Instrument);
				if (id != null) {
					string oldDate = Bars.GetTime(0).Date.ToString("yyyy-MM-dd");
					string newDate = Bars.LastBarTime    .Date.ToString("yyyy-MM-dd");
					tkData = SuriServer.GetTkData(id.Value, oldDate, newDate);
				}
			}
		}
		public override string DisplayName { get { return Name; } }
        
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
	        base.OnRender(chartControl, chartScale);
	        if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
        }
        
		protected override void OnBarUpdate() {
			if (tkData.IsNullOrEmpty()) return;
			for (int i = nextIndex; i < tkData.Count; i++) {
				if (tkData[i].Date.Date.Equals(Time[0].Date)) {
					Values[0][0] = tkData[i].Volume;
					Values[1][0] = tkData[i].OpenInterest;
					nextIndex = i;
					return;
				}
				if (tkData[i].Date.Date > Time[0].Date) {
					Values[0][0] = Values[0][1];
					Values[1][0] = Values[1][1];
					Values[2][0] = Values[2][1];
					return;
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
		private Suri.dev.DevDailyOpenInterest[] cacheDevDailyOpenInterest;
		public Suri.dev.DevDailyOpenInterest DevDailyOpenInterest()
		{
			return DevDailyOpenInterest(Input);
		}

		public Suri.dev.DevDailyOpenInterest DevDailyOpenInterest(ISeries<double> input)
		{
			if (cacheDevDailyOpenInterest != null)
				for (int idx = 0; idx < cacheDevDailyOpenInterest.Length; idx++)
					if (cacheDevDailyOpenInterest[idx] != null &&  cacheDevDailyOpenInterest[idx].EqualsInput(input))
						return cacheDevDailyOpenInterest[idx];
			return CacheIndicator<Suri.dev.DevDailyOpenInterest>(new Suri.dev.DevDailyOpenInterest(), input, ref cacheDevDailyOpenInterest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevDailyOpenInterest DevDailyOpenInterest()
		{
			return indicator.DevDailyOpenInterest(Input);
		}

		public Indicators.Suri.dev.DevDailyOpenInterest DevDailyOpenInterest(ISeries<double> input )
		{
			return indicator.DevDailyOpenInterest(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevDailyOpenInterest DevDailyOpenInterest()
		{
			return indicator.DevDailyOpenInterest(Input);
		}

		public Indicators.Suri.dev.DevDailyOpenInterest DevDailyOpenInterest(ISeries<double> input )
		{
			return indicator.DevDailyOpenInterest(input);
		}
	}
}

#endregion
