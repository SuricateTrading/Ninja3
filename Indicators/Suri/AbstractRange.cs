#region Using declarations
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class AbstractRange : Indicator {
		
		private double? min;
		private double? max;
		private int dotedLineCounter;
		private bool drawDotedLine;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Abstract Range";
				Name										= "Abstract Range";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				Days										= 900;
				DrawDotedLinePara							= true;
				DrawLinesPara								= true;
				
				AddPlot(new Stroke(Brushes.Yellow, 3), PlotStyle.Line, "75%");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "50%");
				AddPlot(new Stroke(Brushes.Yellow, 3), PlotStyle.Line, "25%");
			}
		}

		protected override void OnBarUpdate() {}

		protected void CalcMinMax(double value) {
			CalculateMinMax(value);
			if (!DrawLinesPara) return;
			if (CurrentBar > Days) {
				DrawLines();
			} else if (DrawDotedLinePara) {
				dotedLineCounter++;
				if (dotedLineCounter>10) {
					dotedLineCounter = 0;
					drawDotedLine = !drawDotedLine;
				}
				if (drawDotedLine) DrawLines();
			}
		}
		
		private void CalculateMinMax(double value) {
			if (min==null || min > value) min = value;
			if (max==null || max < value) max = value;
		}
		
		private void DrawLines() {
			Values[1][0]= ValueOf(0.75);
			Values[2][0] = ValueOf(0.5);
			Values[3][0] = ValueOf(0.25);
		}
		
		protected double ValueOf(double percent) {
			return min.Value + percent * (max.Value - min.Value);
		}
		
		public override void OnCalculateMinMax() {
			MinValue = min ?? 0;
			MaxValue = max ?? 1;
		}
		
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Zeichne 25 - 75% Linien", Order=0, GroupName="Parameter")]
		public bool DrawLinesPara { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Zeige durchgezogene Linie erst ab X Tagen", Order=1, GroupName="Parameter")]
		public int Days { get; set; }

		[NinjaScriptProperty]
		[Display(Name="Zeichne gestrichelte Linie", Order=2, GroupName="Parameter")]
		public bool DrawDotedLinePara { get; set; }
        #endregion
		
	}
}























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.AbstractRange[] cacheAbstractRange;
		public Suri.AbstractRange AbstractRange(bool drawLinesPara, int days, bool drawDotedLinePara)
		{
			return AbstractRange(Input, drawLinesPara, days, drawDotedLinePara);
		}

		public Suri.AbstractRange AbstractRange(ISeries<double> input, bool drawLinesPara, int days, bool drawDotedLinePara)
		{
			if (cacheAbstractRange != null)
				for (int idx = 0; idx < cacheAbstractRange.Length; idx++)
					if (cacheAbstractRange[idx] != null && cacheAbstractRange[idx].DrawLinesPara == drawLinesPara && cacheAbstractRange[idx].Days == days && cacheAbstractRange[idx].DrawDotedLinePara == drawDotedLinePara && cacheAbstractRange[idx].EqualsInput(input))
						return cacheAbstractRange[idx];
			return CacheIndicator<Suri.AbstractRange>(new Suri.AbstractRange(){ DrawLinesPara = drawLinesPara, Days = days, DrawDotedLinePara = drawDotedLinePara }, input, ref cacheAbstractRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.AbstractRange AbstractRange(bool drawLinesPara, int days, bool drawDotedLinePara)
		{
			return indicator.AbstractRange(Input, drawLinesPara, days, drawDotedLinePara);
		}

		public Indicators.Suri.AbstractRange AbstractRange(ISeries<double> input , bool drawLinesPara, int days, bool drawDotedLinePara)
		{
			return indicator.AbstractRange(input, drawLinesPara, days, drawDotedLinePara);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.AbstractRange AbstractRange(bool drawLinesPara, int days, bool drawDotedLinePara)
		{
			return indicator.AbstractRange(Input, drawLinesPara, days, drawDotedLinePara);
		}

		public Indicators.Suri.AbstractRange AbstractRange(ISeries<double> input , bool drawLinesPara, int days, bool drawDotedLinePara)
		{
			return indicator.AbstractRange(input, drawLinesPara, days, drawDotedLinePara);
		}
	}
}

#endregion
