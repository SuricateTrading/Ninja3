#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
using NinjaTrader.Gui.NinjaScript;
using SharpDX;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.Weiteres {
	public class SuriNonAdjusted : Indicator {
		private Bars bars;
		private bool isPrepared;

		[NinjaScriptProperty]
		[Display(Name = "Instrumentname", Order = 0, GroupName = "Parameter", Description = "Lasse das Feld leer, damit das Haupt-Instrument benutzt wird. Ansonsten kann beispielsweise 'GC 04-23' eingegeben werden.")]
		public string market { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Merge", Order = 1, GroupName = "Parameter", Description = "Wenn aktiv werden die Daten mit anderen Instrumenten gemerged. Wenn deaktiviert wird nur 1 Instrument angezeigt, siehe Parameter 'Instrumentname'.")]
		public bool merge { get; set; }

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
				merge										= false;
			} else if (State == State.Configure) {
				AddPlot(Brushes.White, "O");
				AddPlot(Brushes.White, "H");
				AddPlot(Brushes.White, "L");
				AddPlot(Brushes.White, "C");
				AddPlot(Brushes.White, "V");

				if (market.IsNullOrEmpty()) market = Instrument.FullName;
			} else if (State == State.DataLoaded) {
				Instrument instrument = Instrument.GetInstrument(market);
				if (instrument == null) return;
				new BarsRequest(instrument, Bars.GetTime(0), Bars.LastBarTime.Date) {
					MergePolicy = merge ? MergePolicy.MergeNonBackAdjusted : MergePolicy.DoNotMerge,
					BarsPeriod = new BarsPeriod {BarsPeriodType = Bars.BarsPeriod.BarsPeriodType, Value = Bars.BarsPeriod.Value},
				}.Request((barsRequest, minuteErrorCode, minuteErrorMessage) => {
					bars = barsRequest.Bars;
					ForceRefresh();
				});
			}
		}
		public override string DisplayName { get { return Name + " " + market; } }
		
		protected override void OnBarUpdate() {
			PlotBrushes[0][0] = ChartControl.Properties.ChartText;
			PlotBrushes[1][0] = ChartControl.Properties.ChartText;
			PlotBrushes[2][0] = ChartControl.Properties.ChartText;
			PlotBrushes[3][0] = ChartControl.Properties.ChartText;
			PlotBrushes[4][0] = ChartControl.Properties.ChartText;
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

		private void Prepare() {
			if (isPrepared || Bars == null || bars == null) return;
			
			Tuple<int, int> index = SuriCommon.SynchronizeIndex(new Tuple<int, int>(0, 0), Bars, bars);
			for (int i = 0; i < Bars.Count; i++) {
				int j = Bars.Count - 1 - i;
				if (index != null && i == index.Item1) {
					opens[j] 	=	bars.GetOpen(index.Item2);
					highs[j] 	=	bars.GetHigh(index.Item2);
					lows[j]		=	bars.GetLow(index.Item2);
					closes[j]	=	bars.GetClose(index.Item2);
					volumes[j]	=	bars.GetVolume(index.Item2);
					index = SuriCommon.SynchronizeIndex(new Tuple<int, int>(index.Item1 + 1, index.Item2 + 1), Bars, bars);
				} else {
					opens[j] 	= double.NaN;
					highs[j] 	= double.NaN;
					lows[j]		= double.NaN;
					closes[j]	= double.NaN;
					volumes[j]	= double.NaN;
					if (index == null) break;
				}
			}
			isPrepared = true;
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (ChartControl == null) return;
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
			if (!isPrepared) TriggerCustomEvent(o => Prepare(), null);
			if (upFill == null) OnRenderTargetChanged();

			float wickWidth = ChartBars.Properties.ChartStyle.Stroke.Width;
			float barPaintWidth = Math.Max(3, 1 + 2 * ((int)ChartControl.BarWidth - 1) + 2 * wickWidth);

			for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++) {
				try {
					if (double.IsNaN(opens.GetValueAt(idx))) continue;
				} catch (Exception) {
					continue;
				}
				if (idx - Displacement < 0 || idx - Displacement >= BarsArray[0].Count || idx - Displacement < BarsRequiredToPlot) continue;

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
			if (!isPrepared || Bars == null || bars == null || ChartControl == null) return;
			try {
				MaxValue = double.MinValue;
				MinValue = double.MaxValue;
			
				for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++) {
					if (opens.GetValueAt(idx) == 0) continue;
					double tmpHigh = highs.GetValueAt(idx);
					double tmpLow  = lows.GetValueAt(idx);
					if (tmpHigh > MaxValue)	MaxValue = tmpHigh;
					if (tmpLow  < MinValue)	MinValue = tmpLow;
				}
			} catch (Exception) {
				MaxValue = 1;
				MinValue = 0;
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
		public Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(string market, bool merge)
		{
			return SuriNonAdjusted(Input, market, merge);
		}

		public Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(ISeries<double> input, string market, bool merge)
		{
			if (cacheSuriNonAdjusted != null)
				for (int idx = 0; idx < cacheSuriNonAdjusted.Length; idx++)
					if (cacheSuriNonAdjusted[idx] != null && cacheSuriNonAdjusted[idx].market == market && cacheSuriNonAdjusted[idx].merge == merge && cacheSuriNonAdjusted[idx].EqualsInput(input))
						return cacheSuriNonAdjusted[idx];
			return CacheIndicator<Suri.Weiteres.SuriNonAdjusted>(new Suri.Weiteres.SuriNonAdjusted(){ market = market, merge = merge }, input, ref cacheSuriNonAdjusted);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(string market, bool merge)
		{
			return indicator.SuriNonAdjusted(Input, market, merge);
		}

		public Indicators.Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(ISeries<double> input , string market, bool merge)
		{
			return indicator.SuriNonAdjusted(input, market, merge);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(string market, bool merge)
		{
			return indicator.SuriNonAdjusted(Input, market, merge);
		}

		public Indicators.Suri.Weiteres.SuriNonAdjusted SuriNonAdjusted(ISeries<double> input , string market, bool merge)
		{
			return indicator.SuriNonAdjusted(input, market, merge);
		}
	}
}

#endregion
