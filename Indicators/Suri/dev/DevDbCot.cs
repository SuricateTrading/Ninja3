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
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "DbCot");
			} else if (State == State.DataLoaded) {
				int? id = SuriStrings.GetId(Instrument);
				if (id != null) {
					string oldDate = From.Date.ToString("yyyy-MM-dd");
					string newDate = To.Date.ToString("yyyy-MM-dd");
					dbCotData = SuriServer.GetCotData(id.Value, oldDate, newDate, 0);
				}
			}
		}
		
		protected override void OnBarUpdate() {
			if (dbCotData == null) return;
			if (nextIndex >= dbCotData.Count) {
				Value[0] = dbCotData[dbCotData.Count-1].OpenInterest;
				return;
			}
			
			string now = Time[0].Date.ToString("yyyy-MM-dd");
			if (dbCotData[nextIndex].Date.Equals(now)) {
				Value[0] = dbCotData[nextIndex].OpenInterest;
				nextIndex++;
			} else if (nextIndex >= 1) {
				Value[0] = dbCotData[nextIndex-1].OpenInterest;
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
