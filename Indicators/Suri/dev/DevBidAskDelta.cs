#region Using declarations
using System.Linq;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DevBidAskDelta : Indicator {
		private readonly SuriVpIntraData suriVpIntraData = new SuriVpIntraData();
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
			} else if (State == State.Configure) {
				AddPlot(new Stroke(Brushes.CornflowerBlue, 4), PlotStyle.Bar, "Delta");
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Line, "Delta %");
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "0");
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			Plots[0].Width = (float) (chartControl.BarWidth * 2.0);
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
			if      (Values[0][barsAgo] > 0) PlotBrushes[0][barsAgo] = Brushes.LimeGreen;
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
