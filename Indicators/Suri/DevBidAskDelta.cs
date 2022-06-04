#region Using declarations
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class DevBidAskDelta : Indicator {
		private SuriVpIntraData suriVpIntraData = new SuriVpIntraData();
		private int lastBarStored = int.MinValue;
		private int lastBarLoaded;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Bid Ask Delta";
				Name										= "Bid Ask Delta";
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
				
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Bar, "Delta");
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Bar, "Delta %");
				AddPlot(new Stroke(Brushes.Gray, 1), PlotStyle.Line, "0");
			} else if (State == State.DataLoaded && !Bars.IsTickReplay &&
			           (SuriAddOn.license == License.Premium || SuriAddOn.license == License.Dev) &&
			           Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && Bars.BarsPeriod.Value == 1440
			) {
				/*SuriIntraRepo.GetVpIntra(Instrument, Bars.GetTime(0).Date, Bars.LastBarTime.Date, data => {
					suriVpIntraData = data;
					for (int i = CurrentBar - 1; i >= 0; i--) {
						UpdateData(i);
					}
					ForceRefresh();
				});*/
			}
		}
		
		protected override void OnMarketData(MarketDataEventArgs e) {
			if (SuriAddOn.license == License.None || Bars.Count <= 0 || !Bars.IsTickReplay || e.MarketDataType != MarketDataType.Last) return;
			if (lastBarStored != CurrentBar) {
				lastBarStored = CurrentBar;
				suriVpIntraData.barData.Add(new SuriVpBarData(TickSize, e.Time));
			}
			suriVpIntraData.barData.Last().AddTick(e);
		}
		
		
		private void UpdateData(int barsAgo) {
			if (suriVpIntraData == null || suriVpIntraData.barData.IsNullOrEmpty()) return;
			Values[0][barsAgo] = suriVpIntraData.barData[lastBarLoaded].delta;
			Values[1][barsAgo] = 100 * suriVpIntraData.barData[lastBarLoaded].delta / suriVpIntraData.barData[lastBarLoaded].totalVolume;
			Values[2][barsAgo] = 0;
			if      (Values[0][barsAgo] > 0) PlotBrushes[0][barsAgo] = Brushes.Green;
			else if (Values[0][barsAgo] < 0) PlotBrushes[0][barsAgo] = Brushes.Red;
			else                             PlotBrushes[0][barsAgo] = Brushes.Yellow;
			if (lastBarLoaded < Bars.Count-1) lastBarLoaded++;
		}
		
		protected override void OnBarUpdate() {
			UpdateData(0);
		}
		
	}
}





















































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.DevBidAskDelta[] cacheDevBidAskDelta;
		public Suri.DevBidAskDelta DevBidAskDelta()
		{
			return DevBidAskDelta(Input);
		}

		public Suri.DevBidAskDelta DevBidAskDelta(ISeries<double> input)
		{
			if (cacheDevBidAskDelta != null)
				for (int idx = 0; idx < cacheDevBidAskDelta.Length; idx++)
					if (cacheDevBidAskDelta[idx] != null &&  cacheDevBidAskDelta[idx].EqualsInput(input))
						return cacheDevBidAskDelta[idx];
			return CacheIndicator<Suri.DevBidAskDelta>(new Suri.DevBidAskDelta(), input, ref cacheDevBidAskDelta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.DevBidAskDelta DevBidAskDelta()
		{
			return indicator.DevBidAskDelta(Input);
		}

		public Indicators.Suri.DevBidAskDelta DevBidAskDelta(ISeries<double> input )
		{
			return indicator.DevBidAskDelta(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.DevBidAskDelta DevBidAskDelta()
		{
			return indicator.DevBidAskDelta(Input);
		}

		public Indicators.Suri.DevBidAskDelta DevBidAskDelta(ISeries<double> input )
		{
			return indicator.DevBidAskDelta(input);
		}
	}
}

#endregion
