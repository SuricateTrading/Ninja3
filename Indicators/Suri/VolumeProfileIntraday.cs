#region Using declarations

using System;
using NinjaTrader.Gui.Chart;
using SharpDX;
using System.Windows.Media;
using NinjaTrader.Gui;
using Brush = SharpDX.Direct2D1.Brush;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class VolumeProfileIntraday : Indicator {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "VolumeProfileIntraday";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			
			double height = chartScale.GetYByValue(chartScale.MaxValue - TickSize * 1000000.0) / 1000000.0;
			RectangleF rect = new RectangleF();
			rect.Height = (float)height;
			double barWidth = 0; // chartControl.BarWidth;
			
			SolidColorBrush s = Brushes.MediumBlue.Clone();
			s.Opacity = 0.8;
			Brush fill = s.ToDxBrush(RenderTarget);

			for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++) {
				int x = chartControl.GetXByBarIndex(ChartBars, idx);
				rect.X = (float)(x - barWidth * 0.5f) + 5;
				double highTicks = Instrument.MasterInstrument.RoundToTickSize(chartScale.GetFirstChartBars().Bars.GetHigh(idx)) / TickSize;
				double lowTicks = Instrument.MasterInstrument.RoundToTickSize(chartScale.GetFirstChartBars().Bars.GetLow(idx)) / TickSize;
				double ticks = Math.Round(highTicks - lowTicks);
				double y = chartScale.GetYByValue(ChartBars.Bars.GetHigh(idx));
				
				for (int i = 0; i < ticks; i++) {
					rect.Y = (float)(y + i * height);
					rect.Width = 100;
					RenderTarget.FillRectangle(rect, fill);
					RenderTarget.DrawRectangle(rect, Brushes.Transparent.ToDxBrush(RenderTarget));
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
		private Suri.VolumeProfileIntraday[] cacheVolumeProfileIntraday;
		public Suri.VolumeProfileIntraday VolumeProfileIntraday()
		{
			return VolumeProfileIntraday(Input);
		}

		public Suri.VolumeProfileIntraday VolumeProfileIntraday(ISeries<double> input)
		{
			if (cacheVolumeProfileIntraday != null)
				for (int idx = 0; idx < cacheVolumeProfileIntraday.Length; idx++)
					if (cacheVolumeProfileIntraday[idx] != null &&  cacheVolumeProfileIntraday[idx].EqualsInput(input))
						return cacheVolumeProfileIntraday[idx];
			return CacheIndicator<Suri.VolumeProfileIntraday>(new Suri.VolumeProfileIntraday(), input, ref cacheVolumeProfileIntraday);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.VolumeProfileIntraday VolumeProfileIntraday()
		{
			return indicator.VolumeProfileIntraday(Input);
		}

		public Indicators.Suri.VolumeProfileIntraday VolumeProfileIntraday(ISeries<double> input )
		{
			return indicator.VolumeProfileIntraday(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.VolumeProfileIntraday VolumeProfileIntraday()
		{
			return indicator.VolumeProfileIntraday(Input);
		}

		public Indicators.Suri.VolumeProfileIntraday VolumeProfileIntraday(ISeries<double> input )
		{
			return indicator.VolumeProfileIntraday(input);
		}
	}
}

#endregion
