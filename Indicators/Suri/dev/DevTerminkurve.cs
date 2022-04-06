#region Using declarations

using System;
using System.Collections.Generic;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DevTerminkurve : Indicator {
		private List<TkData> tkData;
		private int nextIndex;
		private int days = 250;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Terminkurve";
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
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Line, "Status");
				AddPlot(new Stroke(Brushes.DarkGray, 2), PlotStyle.Line, "Delta");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "Oszillator");
			} else if (State == State.DataLoaded) {
				int? id = SuriStrings.GetId(Instrument);
				if (id != null) {
					string oldDate = ChartBars.Bars.GetTime(0).Date.ToString("yyyy-MM-dd");
					string newDate = ChartBars.Bars.LastBarTime    .Date.ToString("yyyy-MM-dd");
					tkData = SuriServer.GetTkData(id.Value, oldDate, newDate);
				}
			}
		}
		
		//public override void OnCalculateMinMax() { MinValue = -2; MaxValue = 2; }
		public override void OnCalculateMinMax() { MinValue = 0; MaxValue = 100; }
		
		protected override void OnBarUpdate() {
			if (tkData == null) return;
			for (int i = nextIndex; i < tkData.Count; i++) {
				if (tkData[i].Date.Date.Equals(Time[0].Date)) {
					Values[0][0] = (tkData[i].TkState+2)*25;
					Values[1][0] = tkData[i].Delta;
					if (CurrentBar >= days) {
						double min = double.MaxValue;
						double max = double.MinValue;
						for (int barsAgo = 0; barsAgo < days; barsAgo++) {
							double v = Values[1][barsAgo];
							if (min > v) min = v;
							if (max < v) max = v;
						}
						Values[2][0] = 100.0 * (tkData[i].Delta - min) / (max - min);
					}
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
		private Suri.dev.DevTerminkurve[] cacheDevTerminkurve;
		public Suri.dev.DevTerminkurve DevTerminkurve()
		{
			return DevTerminkurve(Input);
		}

		public Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input)
		{
			if (cacheDevTerminkurve != null)
				for (int idx = 0; idx < cacheDevTerminkurve.Length; idx++)
					if (cacheDevTerminkurve[idx] != null &&  cacheDevTerminkurve[idx].EqualsInput(input))
						return cacheDevTerminkurve[idx];
			return CacheIndicator<Suri.dev.DevTerminkurve>(new Suri.dev.DevTerminkurve(), input, ref cacheDevTerminkurve);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve()
		{
			return indicator.DevTerminkurve(Input);
		}

		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input )
		{
			return indicator.DevTerminkurve(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve()
		{
			return indicator.DevTerminkurve(Input);
		}

		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input )
		{
			return indicator.DevTerminkurve(input);
		}
	}
}

#endregion
