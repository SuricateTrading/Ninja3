#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using SharpDX;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.Weiteres {
	public class SuriNonAdjusted : Indicator {
		private Bars bars;

		[NinjaScriptProperty]
		[Display(Name = "Instrumentname", Order = 0)]
		public string market { get; set; }

		private Series<double> opens   { get { return Values[0]; } }
		private Series<double> highs   { get { return Values[1]; } }
		private Series<double> lows	   { get { return Values[2]; } }
		private Series<double> closes  { get { return Values[3]; } }
		private Series<double> volumes { get { return Values[4]; } }

		private SharpDX.Direct2D1.Brush upFill;
		private SharpDX.Direct2D1.Brush downFill;
		private SharpDX.Direct2D1.Brush wickFill;


		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Nicht Adjustiert";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				market										= "";
			} else if (State == State.Configure) {
				AddPlot(Brushes.White, "O");
				AddPlot(Brushes.White, "H");
				AddPlot(Brushes.White, "L");
				AddPlot(Brushes.White, "C");
				AddPlot(Brushes.White, "V");

				if (market.IsNullOrEmpty()) market = Instrument.FullName;
				Instrument instrument = Instrument.GetInstrument(market);
				if (instrument == null) return;
				new BarsRequest(instrument, Bars.GetTime(0), Bars.LastBarTime.Date) {
					MergePolicy = MergePolicy.DoNotMerge,
					BarsPeriod = new BarsPeriod {BarsPeriodType = Bars.BarsPeriod.BarsPeriodType, Value = Bars.BarsPeriod.Value},
				}.Request((bars, minuteErrorCode, minuteErrorMessage) => {
					this.bars = bars.Bars;
					Update();
				});
			}
		}
		public override string DisplayName { get { return Name; } }
		
		private int index;
		protected override void OnBarUpdate() {
			PlotBrushes[0][0] = ChartControl.Properties.ChartText;
			PlotBrushes[1][0] = ChartControl.Properties.ChartText;
			PlotBrushes[2][0] = ChartControl.Properties.ChartText;
			PlotBrushes[3][0] = ChartControl.Properties.ChartText;
			PlotBrushes[4][0] = ChartControl.Properties.ChartText;
			
			if (bars == null || Time[0] < bars.GetTime(0)) return;
			try {
				while (index < bars.Count - 1 && Time[0] > bars.GetTime(index)) index++;
				if (index == bars.Count) return;

				opens[0] 	=	bars.GetOpen(index);
				highs[0] 	=	bars.GetHigh(index);
				lows[0]		=	bars.GetLow(index);
				closes[0]	=	bars.GetClose(index);
				volumes[0]	=	bars.GetVolume(index);
			} catch (Exception) {
				Print("error line " + CurrentBar);
			}
		}

		public override void OnRenderTargetChanged() {
			if (upFill != null) {
				upFill.Dispose();
				downFill.Dispose();
				wickFill.Dispose();
			}
			if (RenderTarget != null && ChartBars != null) {
				upFill = ChartBars.Properties.ChartStyle.UpBrush.ToDxBrush(RenderTarget);
				downFill = ChartBars.Properties.ChartStyle.DownBrush.ToDxBrush(RenderTarget);
				wickFill = ChartBars.Properties.ChartStyle.Stroke.Brush.ToDxBrush(RenderTarget);
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (Bars == null || bars == null || ChartControl == null) return;

			if (upFill == null) OnRenderTargetChanged();

			float wickWidth = ChartBars.Properties.ChartStyle.Stroke.Width;
			float barPaintWidth = Math.Max(3, 1 + 2 * ((int)ChartControl.BarWidth - 1) + 2 * wickWidth);

			for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++) {
				if (opens.GetValueAt(idx) == 0 || idx - Displacement < 0 || idx - Displacement >= BarsArray[0].Count || idx - Displacement < BarsRequiredToPlot) continue;

				int x  = chartControl.GetXByBarIndex(ChartBars, idx);
				int y1 = chartScale.GetYByValue(opens.GetValueAt(idx));
				int y2 = chartScale.GetYByValue(highs.GetValueAt(idx));
				int y3 = chartScale.GetYByValue(lows.GetValueAt(idx));
				int y4 = chartScale.GetYByValue(closes.GetValueAt(idx));

				var xy2 = new Vector2(x, y2);
				var xy3 = new Vector2(x, y3);
				RenderTarget.DrawLine(xy2, xy3, wickFill, wickWidth);

				if (y4 == y1) {
					RenderTarget.DrawLine(new Vector2(x - barPaintWidth / 2, y1), new Vector2(x + barPaintWidth / 2, y1), wickFill, wickWidth);
				} else {
					if (y4 > y1) {
						RenderTarget.FillRectangle( new RectangleF(x - barPaintWidth / 2, y1, barPaintWidth, y4 - y1), downFill);
					} else {
						RenderTarget.FillRectangle( new RectangleF(x - barPaintWidth / 2, y4, barPaintWidth, y1 - y4), upFill);
					}
					RenderTarget.DrawRectangle( new RectangleF( x - barPaintWidth / 2 + wickWidth / 2, Math.Min(y4, y1), barPaintWidth - wickWidth, Math.Abs(y4 - y1)), wickFill, wickWidth);
				}
			}
		}

		public override void OnCalculateMinMax() {
			base.OnCalculateMinMax();
			if (Bars == null || bars == null || ChartControl == null) return;
			
			MaxValue = double.MinValue;
			MinValue = double.MaxValue;
			
			for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++) {
				if (opens.GetValueAt(idx) == 0) continue;
				double tmpHigh = highs.GetValueAt(idx);
				double tmpLow  = lows.GetValueAt(idx);
				if (tmpHigh > MaxValue)	MaxValue = tmpHigh;
				if (tmpLow  < MinValue)	MinValue = tmpLow;
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
		private Suri.Weiteres.SuriNonAdjusted[] cacheSuriNonAdjusted;
		public Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(string market)
		{
			return SuriNonAdjusted(Input, market);
		}

		public Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(ISeries<double> input, string market)
		{
			if (cacheSuriNonAdjusted != null)
				for (int idx = 0; idx < cacheSuriNonAdjusted.Length; idx++)
					if (cacheSuriNonAdjusted[idx] != null && cacheSuriNonAdjusted[idx].market == market && cacheSuriNonAdjusted[idx].EqualsInput(input))
						return cacheSuriNonAdjusted[idx];
			return CacheIndicator<Suri.Weiteres.SuriNonAdjusted>(new Suri.Weiteres.SuriNonAdjusted(){ market = market }, input, ref cacheSuriNonAdjusted);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(string market)
		{
			return indicator.SuriNonAdjusted(Input, market);
		}

		public Indicators.Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(ISeries<double> input , string market)
		{
			return indicator.SuriNonAdjusted(input, market);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(string market)
		{
			return indicator.SuriNonAdjusted(Input, market);
		}

		public Indicators.Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(ISeries<double> input , string market)
		{
			return indicator.SuriNonAdjusted(input, market);
		}
	}
}

#endregion
