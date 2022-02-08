#region Using declarations
using System.Collections.Generic;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DbCot : Indicator {
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
				int id = SuriStrings.getId(Instrument.FullName);
				if (id != -1) {
					string oldDate = From.Date.ToString("yyyy-MM-dd");
					string newDate = To.Date.ToString("yyyy-MM-dd");
					dbCotData = SuriServer.GetCotData(id, oldDate, newDate, 0);
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
		private Suri.dev.DbCot[] cacheDbCot;
		public Suri.dev.DbCot DbCot()
		{
			return DbCot(Input);
		}

		public Suri.dev.DbCot DbCot(ISeries<double> input)
		{
			if (cacheDbCot != null)
				for (int idx = 0; idx < cacheDbCot.Length; idx++)
					if (cacheDbCot[idx] != null &&  cacheDbCot[idx].EqualsInput(input))
						return cacheDbCot[idx];
			return CacheIndicator<Suri.dev.DbCot>(new Suri.dev.DbCot(), input, ref cacheDbCot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DbCot DbCot()
		{
			return indicator.DbCot(Input);
		}

		public Indicators.Suri.dev.DbCot DbCot(ISeries<double> input )
		{
			return indicator.DbCot(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DbCot DbCot()
		{
			return indicator.DbCot(Input);
		}

		public Indicators.Suri.dev.DbCot DbCot(ISeries<double> input )
		{
			return indicator.DbCot(input);
		}
	}
}

#endregion
