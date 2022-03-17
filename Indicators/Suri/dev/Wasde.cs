#region Using declarations
using System.Collections.Generic;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class Wasde : Indicator {
		private List<WasdeData> wasdeData;
		private int nextIndex;
		private bool hasStarted;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Wasde";
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
				AddPlot(new Stroke(Brushes.DarkGreen, 2), PlotStyle.Line, "Alte Ernte");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "Berechnet");
				AddPlot(new Stroke(Brushes.LightGreen, 2), PlotStyle.Line, "Vorhersage");
			} else if (State == State.DataLoaded) {
				int? id = SuriStrings.GetId(Instrument);
				if (id != null) {
					string oldDate = ChartBars.Bars.GetTime(0).AddMonths(-1).Date.ToString("yyyy-MM-dd");
					string newDate = ChartBars.Bars.LastBarTime    .AddMonths(+1).Date.ToString("yyyy-MM-dd");
					wasdeData = SuriServer.GetWasdeData(id.Value, oldDate, newDate);
				}
			}
		}
		
		protected override void OnBarUpdate() {
			if (wasdeData == null) return;
			for (int i = nextIndex; i < wasdeData.Count; i++) {
				if (wasdeData[i].Date.Date.Equals(Time[0].Date)) {
					hasStarted = true;
					Values[0][0] = wasdeData[i].endingStocks;	i++;
					Values[1][0] = wasdeData[i].endingStocks;	i++;
					Values[2][0] = wasdeData[i].endingStocks;
					nextIndex = i;
					return;
				}
				if (hasStarted && wasdeData[i].Date.Date > Time[0].Date) {
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
		private Suri.dev.Wasde[] cacheWasde;
		public Suri.dev.Wasde Wasde()
		{
			return Wasde(Input);
		}

		public Suri.dev.Wasde Wasde(ISeries<double> input)
		{
			if (cacheWasde != null)
				for (int idx = 0; idx < cacheWasde.Length; idx++)
					if (cacheWasde[idx] != null &&  cacheWasde[idx].EqualsInput(input))
						return cacheWasde[idx];
			return CacheIndicator<Suri.dev.Wasde>(new Suri.dev.Wasde(), input, ref cacheWasde);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.Wasde Wasde()
		{
			return indicator.Wasde(Input);
		}

		public Indicators.Suri.dev.Wasde Wasde(ISeries<double> input )
		{
			return indicator.Wasde(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.Wasde Wasde()
		{
			return indicator.Wasde(Input);
		}

		public Indicators.Suri.dev.Wasde Wasde(ISeries<double> input )
		{
			return indicator.Wasde(input);
		}
	}
}

#endregion
