#region Using declarations
using System.Collections.Generic;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DevDbCot : Indicator {
		private List<DbCotData> dbCotData;
		private int nextIndex;
		private bool hasStarted;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "DbCot";
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
				AddPlot(new Stroke(Brushes.DarkGray, 2), PlotStyle.Line, "DbCot");
				AddPlot(new Stroke(Brushes.DarkGray, 2), PlotStyle.Line, "Min");
				AddPlot(new Stroke(Brushes.DarkGray, 2), PlotStyle.Line, "Mid");
				AddPlot(new Stroke(Brushes.DarkGray, 2), PlotStyle.Line, "Max");
			} else if (State == State.DataLoaded) {
				Commodity? commodity = SuriStrings.GetComm(Instrument);
				if (commodity != null) {
					dbCotData = SuriCotRepo.GetCotData(commodity.Value, Bars.GetTime(0).Date, Bars.LastBarTime.Date).Result;
				}
			}
		}
		
		protected override void OnBarUpdate() {
			if (dbCotData == null) return;
			for (int i = nextIndex; i < dbCotData.Count; i++) {
				if (dbCotData[i].Date.Date.Equals(Time[0].Date)) {
					Values[0][0] = dbCotData[i].CommercialsShort;
					Values[1][0] = dbCotData[i].Cot2Min;
					Values[2][0] = dbCotData[i].Cot2Mid;
					Values[3][0] = dbCotData[i].Cot2Max;
					nextIndex = i;
					hasStarted = true;
					return;
				}
				if (hasStarted && dbCotData[i].Date.Date > Time[0].Date) {
					Values[0][0] = Values[0][1];
					Values[1][0] = Values[1][1];
					Values[2][0] = Values[2][1];
					Values[3][0] = Values[3][1];
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
		private Suri.dev.DevDbCot[] cacheDevDbCot;
		public Suri.dev.DevDbCot DevDbCot()
		{
			return DevDbCot(Input);
		}

		public Suri.dev.DevDbCot DevDbCot(ISeries<double> input)
		{
			if (cacheDevDbCot != null)
				for (int idx = 0; idx < cacheDevDbCot.Length; idx++)
					if (cacheDevDbCot[idx] != null &&  cacheDevDbCot[idx].EqualsInput(input))
						return cacheDevDbCot[idx];
			return CacheIndicator<Suri.dev.DevDbCot>(new Suri.dev.DevDbCot(), input, ref cacheDevDbCot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevDbCot DevDbCot()
		{
			return indicator.DevDbCot(Input);
		}

		public Indicators.Suri.dev.DevDbCot DevDbCot(ISeries<double> input )
		{
			return indicator.DevDbCot(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevDbCot DevDbCot()
		{
			return indicator.DevDbCot(Input);
		}

		public Indicators.Suri.dev.DevDbCot DevDbCot(ISeries<double> input )
		{
			return indicator.DevDbCot(input);
		}
	}
}

#endregion
