#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Newtonsoft.Json;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using SharpDX;
using Point = System.Windows.Point;
#endregion

namespace NinjaTrader.NinjaScript.DrawingTools {
	public class Cot2Box : MarkableBar {
		[Display(Name = "Long (an) oder Short (aus)", Description = "Ob eine COT 2 Long (aktiviert) oder Short Box (deaktiviert) eingezeichnet werden soll.", GroupName = "Parameter", Order = 0)]
		public bool isLong { get; set; }
		public override object Icon { get { return new Image { Source = new BitmapImage(new Uri(SuriAddOn.path + "cot2.png", UriKind.Absolute)), Width = 16, Height = 16 }; } }
		
		protected override void OnStateChange() {
			base.OnStateChange();
			if (State == State.SetDefaults) {
				Name = "Suri COT2 Box";
				isLong = true;
			}
		}
		
		protected override void DoCustomRenderWork(ChartScale chartScale, ChartControl chartControl, Point startPoint) {
			SharpDX.Direct2D1.Brush textBrush = chartControl.Properties.ChartText.ToDxBrush(RenderTarget);
			ChartBars chartBars = chartScale.GetFirstChartBars();
			Bars bars = chartBars.Bars;
			int barIndex = chartBars.GetBarIdxByTime(chartControl, StartAnchor.Time);
			int barsAgo  = bars.Count - 1 - barIndex;
			if (barsAgo < 0) return;
			
			StrikingSpotData s;
			try {
				s = StrikingCalculator.FindStrikingSpot(!isLong, bars, barIndex);
			} catch (Exception e) {
				Print(e.ToString());
				return;
			}

			double stopLoss = SuriCommon.PriceToCurrency(chartControl.Instrument, Math.Abs(bars.GetClose(barIndex) - s.p2Value));

			Vector2 v1		= new Vector2(chartControl.GetXByBarIndex(chartBars, s.p1Bar), chartScale.GetYByValue(s.p1Value));
			Vector2 v2		= new Vector2(chartControl.GetXByBarIndex(chartBars, s.p2Bar), chartScale.GetYByValue(s.p2Value));
			Vector2 v3		= new Vector2(chartControl.GetXByBarIndex(chartBars, s.p3Bar), chartScale.GetYByValue(s.p3Value));
			Vector2 vEnd1 = new Vector2((float) startPoint.X, chartScale.GetYByValue(s.p3Value));
			Vector2 vEnd2 = new Vector2((float) startPoint.X, chartScale.GetYByValue(s.p2Value));
			
			RenderTarget.DrawLine(v1, v2, Brushes.CornflowerBlue.ToDxBrush(RenderTarget));
			RenderTarget.DrawLine(v2, v3, Brushes.CornflowerBlue.ToDxBrush(RenderTarget));
			RenderTarget.DrawLine(v3, vEnd1, Brushes.Green.ToDxBrush(RenderTarget));
			RenderTarget.DrawLine(v2, vEnd2, Brushes.Red.ToDxBrush(RenderTarget));
			
			SharpDX.DirectWrite.TextFormat textFormat = new SimpleFont { Size = 15 }.ToDirectWriteTextFormat();
			RenderTarget.DrawText(isLong ? "Altes\nHoch" : "Altes\nTief", textFormat, new RectangleF(v1.X - 17, v1.Y - (isLong ? 40 : 0),200,textFormat.FontSize), textBrush);
			RenderTarget.DrawText((isLong ? "Tief @ " : "Hoch @ ") + s.p2Value + "\nRisiko: " + stopLoss.ToString("F0") + " $", textFormat, new RectangleF(v2.X - 10, v2.Y - (isLong ? -5 : 50),200,textFormat.FontSize), textBrush);
			RenderTarget.DrawText((isLong ? "Neues Hoch @ " : "Neues Tief @ ") + s.p3Value, textFormat, new RectangleF(v3.X - 10, v3.Y - (isLong ? 25 : -5),200,textFormat.FontSize), textBrush);
		}
	}
}
