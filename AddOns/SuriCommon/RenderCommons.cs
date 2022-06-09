using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;
using TextAlignment = System.Windows.TextAlignment;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public static class RenderCommons {
        
        public static void DrawTextAt(RenderTarget renderTarget, ChartControl chartControl, int barIndex, int y) {
            renderTarget.DrawText(barIndex.ToString(), chartControl.Properties.LabelFont.ToDirectWriteTextFormat(), new RectangleF(chartControl.GetXByBarIndex(chartControl.PrimaryBars, barIndex), y, 100, 100), chartControl.Properties.ChartText.ToDxBrush(renderTarget));
        }
        
        public static void DrawText(NinjaScriptBase ninjaScriptBase, string tag, string text, int barsAgo, double y, int offset) {
            Draw.Text(ninjaScriptBase, tag + " " + SuriCommon.random, false, text, barsAgo, y, offset, Brushes.LightGray, new SimpleFont(), TextAlignment.Center, null, null, 0);
        }
    }
}
