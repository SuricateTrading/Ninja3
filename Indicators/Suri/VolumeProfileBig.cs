#region Using declarations

using System;
using NinjaTrader.Gui.Chart;
using SharpDX;
using System.Windows.Media;
using NinjaTrader.Gui;
using SharpDX.Direct2D1;
using Brush = SharpDX.Direct2D1.Brush;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class VolumeProfileBig : Indicator {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "VolumeProfileBig";
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
			SetZOrder(-1);
			
			double height = chartScale.GetYByValue(chartScale.MaxValue - TickSize * 1000000.0) / 1000000.0;
			RectangleF rect = new RectangleF();
			rect.Height = (float)height;
			
			SolidColorBrush s = Brushes.MediumBlue.Clone();
			s.Opacity = 0.8;
			Brush fill = s.ToDxBrush(RenderTarget);

			rect.X = 0;
			double highTicks = Instrument.MasterInstrument.RoundToTickSize(chartScale.MaxValue) / TickSize;
			double lowTicks = Instrument.MasterInstrument.RoundToTickSize(chartScale.MinValue) / TickSize;
			double ticks = Math.Round(highTicks - lowTicks);
			
			for (int i = 0; i < ticks; i++) {
				rect.Y = (float)(i * height);
				rect.Width = Math.Abs(100-i);
				RenderTarget.FillRectangle(rect, fill);
				RenderTarget.DrawRectangle(rect, Brushes.Transparent.ToDxBrush(RenderTarget));
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
		private Suri.VolumeProfileBig[] cacheVolumeProfileBig;
		public Suri.VolumeProfileBig VolumeProfileBig()
		{
			return VolumeProfileBig(Input);
		}

		public Suri.VolumeProfileBig VolumeProfileBig(ISeries<double> input)
		{
			if (cacheVolumeProfileBig != null)
				for (int idx = 0; idx < cacheVolumeProfileBig.Length; idx++)
					if (cacheVolumeProfileBig[idx] != null &&  cacheVolumeProfileBig[idx].EqualsInput(input))
						return cacheVolumeProfileBig[idx];
			return CacheIndicator<Suri.VolumeProfileBig>(new Suri.VolumeProfileBig(), input, ref cacheVolumeProfileBig);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.VolumeProfileBig VolumeProfileBig()
		{
			return indicator.VolumeProfileBig(Input);
		}

		public Indicators.Suri.VolumeProfileBig VolumeProfileBig(ISeries<double> input )
		{
			return indicator.VolumeProfileBig(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.VolumeProfileBig VolumeProfileBig()
		{
			return indicator.VolumeProfileBig(Input);
		}

		public Indicators.Suri.VolumeProfileBig VolumeProfileBig(ISeries<double> input )
		{
			return indicator.VolumeProfileBig(input);
		}
	}
}

#endregion
