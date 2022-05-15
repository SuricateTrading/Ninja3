#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using SharpDX;
using SharpDX.Direct2D1;
using DashStyle = SharpDX.Direct2D1.DashStyle;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
using Point = System.Windows.Point;
#endregion

namespace NinjaTrader.NinjaScript.DrawingTools {
	public sealed class Cot1Box : MarkableBar {
		[Display(Name = "Long (an) oder Short (aus)", Description = "Ob eine COT 1 Long (aktiviert) oder Short Box (deaktiviert) eingezeichnet werden soll.", GroupName = "Parameter", Order = 0)]
		public bool isLong { get; set; }

		protected override void OnStateChange() {
			base.OnStateChange();
			if (State == State.SetDefaults) {
				Name = "Suri COT1 Box";
				isLong = true;
			}
		}
		public override object Icon { get { return new Image { Source = new BitmapImage(new Uri(SuriAddOn.path + "cot1.png", UriKind.Absolute)), Width = 16, Height = 16 }; } }
		
		
		
		protected override void DoCustomRenderWork(ChartScale chartScale, ChartControl chartControl, Point startPoint) {
			if (SuriAddOn.license == License.Premium || SuriAddOn.license == License.Dev) {
				DrawCot1Box(chartScale, chartControl);
			} else {
				SimpleFont wpfFont							= chartControl.Properties.LabelFont ?? new SimpleFont();
				wpfFont.Size								= 16;
				SharpDX.DirectWrite.TextFormat textFormat	= wpfFont.ToDirectWriteTextFormat();
				textFormat.TextAlignment					= SharpDX.DirectWrite.TextAlignment.Leading;
				textFormat.WordWrapping						= SharpDX.DirectWrite.WordWrapping.NoWrap;
				SharpDX.DirectWrite.TextLayout textLayout  = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, "Nur für Premium Suris", textFormat, 250, textFormat.FontSize);
				RenderTarget.DrawTextLayout(new SharpDX.Vector2((float) startPoint.X, (float) startPoint.Y), textLayout, chartControl.Properties.ChartText.ToDxBrush(RenderTarget), SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			}
		}

		private bool DrawCot1Box(ChartScale chartScale, ChartControl chartControl) {
			Bars bars = chartScale.GetFirstChartBars().Bars;
			SharpDX.Direct2D1.Brush brush = OutlineStroke.BrushDX;
			
			#region week box
			// find first day of week
			int? startOfWeekIndex = null;
			int?   endOfWeekIndex = null;
			for (int i = chartScale.GetFirstChartBars().GetBarIdxByTime(chartControl, StartAnchor.Time); i >= 0; i--) {
				if (bars.GetTime(i).DayOfWeek == DayOfWeek.Monday || i > 0 && bars.GetTime(i).DayOfWeek < bars.GetTime(i-1).DayOfWeek ) {
					startOfWeekIndex = i;
					break;
				}
			}
			if (startOfWeekIndex == null) return false;
			
			// find last day of week
			for (int i = startOfWeekIndex.Value; i < bars.Count; i++) {
				if (bars.GetTime(i).DayOfWeek == DayOfWeek.Friday || i < bars.Count-1 && bars.GetTime(i).DayOfWeek > bars.GetTime(i+1).DayOfWeek ) {
					endOfWeekIndex = i;
					break;
				}
			}
			if (endOfWeekIndex == null) return false;

			double weekHigh = double.MinValue;
			double weekLow  = double.MaxValue;
			int weekHighIndex = 0;
			int weekLowIndex = 0;

			for (int i = startOfWeekIndex.Value; i <= endOfWeekIndex; i++) {
				if (weekHigh < bars.GetHigh(i)) {
					weekHigh = bars.GetHigh(i);
					weekHighIndex = i;
				}
				if (weekLow > bars.GetLow(i)) {
					weekLow = bars.GetLow(i);
					weekLowIndex = i;
				}
			}
			weekHigh += chartControl.Instrument.MasterInstrument.TickSize;
			weekLow  -= chartControl.Instrument.MasterInstrument.TickSize;
			double entryValue = isLong ? weekHigh : weekLow;

			double strokePixAdjust = OutlineStroke.Width % 2 == 0 ? 0.5d : 0d;
			double weekStartX = chartControl.GetXByBarIndex(chartScale.GetFirstChartBars(), startOfWeekIndex.Value) + strokePixAdjust - chartControl.BarWidth;
			double weekEndX   = chartControl.GetXByBarIndex(chartScale.GetFirstChartBars(),   endOfWeekIndex.Value) + strokePixAdjust + chartControl.BarWidth;
			SharpDX.RectangleF rect = new SharpDX.RectangleF(
				(float) weekStartX,
				(float) (chartScale.GetYByValue(weekHigh) + strokePixAdjust),
				(float) (weekEndX - weekStartX),
				(float) chartScale.GetYByValue(weekLow) - chartScale.GetYByValue(weekHigh)
			);
			RenderTarget.DrawRectangle(rect, brush, OutlineStroke.Width, OutlineStroke.StrokeStyle);
			#endregion
			
			
			#region entry and stop
			
			SimpleFont						wpfFont		= chartControl.Properties.LabelFont ?? new SimpleFont();
			wpfFont.Size								= 16;
			SharpDX.DirectWrite.TextFormat	textFormat	= wpfFont.ToDirectWriteTextFormat();
			textFormat.TextAlignment					= SharpDX.DirectWrite.TextAlignment.Leading;
			textFormat.WordWrapping						= SharpDX.DirectWrite.WordWrapping.NoWrap;
			
			double stop = isLong ? double.MaxValue : double.MinValue;
			for (int j = 0; j < 10; j++) {
				if (isLong)  stop = Math.Min(stop, bars.GetLow (endOfWeekIndex.Value - j));
				if (!isLong) stop = Math.Max(stop, bars.GetHigh(endOfWeekIndex.Value - j));
			}
			if (isLong ) stop -= chartControl.Instrument.MasterInstrument.TickSize;
			if (!isLong) stop += chartControl.Instrument.MasterInstrument.TickSize;
			double stopLossValue = SuriCommon.PriceToCurrency(chartControl.Instrument, Math.Abs(entryValue - stop));
			
			float entryY = chartScale.GetYByValue(entryValue);
			SharpDX.DirectWrite.TextLayout textLayout  = new SharpDX.DirectWrite.TextLayout(
				Core.Globals.DirectWriteFactory,
				(isLong ? "Long " : "Short ") + "Entry @ " + (isLong ? weekHigh : weekLow) + "\nInitiales Risiko: " + stopLossValue + " $",
				textFormat, 250, textFormat.FontSize
			);
			RenderTarget.DrawTextLayout(new SharpDX.Vector2((float) weekStartX, entryY - (isLong ? 44f : 0f)), textLayout, chartControl.Properties.ChartText.ToDxBrush(RenderTarget), SharpDX.Direct2D1.DrawTextOptions.NoSnap);

			double entryStartX = chartControl.GetXByBarIndex(chartScale.GetFirstChartBars(), endOfWeekIndex.Value)   + strokePixAdjust + chartControl.BarWidth;
			
			bool foundEntry = false;
			for (int entryIndex = endOfWeekIndex.Value + 1; entryIndex < bars.Count && !foundEntry; entryIndex++) {
				if (isLong && bars.GetHigh(entryIndex) >= weekHigh || !isLong && bars.GetLow(entryIndex) <= weekLow) {
					foundEntry = true;
					double entryEndX   = chartControl.GetXByBarIndex(chartScale.GetFirstChartBars(), entryIndex            ) + strokePixAdjust + chartControl.BarWidth;
					double stopStartX  = chartControl.GetXByBarIndex(chartScale.GetFirstChartBars(), entryIndex-9   ) + strokePixAdjust - chartControl.BarWidth;
					RenderTarget.DrawLine(
						new Vector2((float) entryStartX, entryY),
						new Vector2((float) entryEndX,   entryY),
						Brushes.Green.ToDxBrush(RenderTarget),
						2
					);
					
					// draw stop
					stop = isLong ? double.MaxValue : double.MinValue;
					for (int j = 0; j < 10; j++) {
						if (isLong)  stop = Math.Min(stop, bars.GetLow (entryIndex-j));
						if (!isLong) stop = Math.Max(stop, bars.GetHigh(entryIndex-j));
					}
					if (isLong ) stop -= chartControl.Instrument.MasterInstrument.TickSize;
					if (!isLong) stop += chartControl.Instrument.MasterInstrument.TickSize;
					float stopY = chartScale.GetYByValue(stop);
					RenderTarget.DrawLine(
						new Vector2((float) stopStartX, stopY),
						new Vector2((float) entryEndX,  stopY),
						Brushes.Red.ToDxBrush(RenderTarget),
						2
					);

					stopLossValue = SuriCommon.PriceToCurrency(chartControl.Instrument, Math.Abs(entryValue - stop));
					string signalExpiredWarning = "";
					if ((bars.GetTime(entryIndex) - bars.GetTime(startOfWeekIndex.Value)).TotalDays > 42) {
						signalExpiredWarning = "\nAchtung: Signal ist erloschen!";
					}
					
					textLayout  = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, "Stop @ " + stop + "\nRisiko: " + stopLossValue.ToString("F0") + " $" + signalExpiredWarning, textFormat, 250, textFormat.FontSize);
					RenderTarget.DrawTextLayout(new SharpDX.Vector2((float) (entryEndX - (entryEndX - stopStartX) / 2.0), stopY - (isLong ? 0f : 44f)), textLayout, chartControl.Properties.ChartText.ToDxBrush(RenderTarget), SharpDX.Direct2D1.DrawTextOptions.NoSnap);
				}
			}

			if (!foundEntry) {
				var style = new StrokeStyleProperties { DashStyle = DashStyle.Dash };
				RenderTarget.DrawLine(
					new Vector2((float) entryStartX, entryY),
					new Vector2((float) (chartControl.GetXByBarIndex(chartScale.GetFirstChartBars(), endOfWeekIndex.Value + 3) + strokePixAdjust + chartControl.BarWidth), entryY),
					Brushes.Yellow.ToDxBrush(RenderTarget),
					2,
					new StrokeStyle(NinjaTrader.Core.Globals.D2DFactory, style)
				);
			}
			
			#endregion

			return true;
		}
		
	}
}
