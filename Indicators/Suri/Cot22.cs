#region Using declarations
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Cot22 : AbstractRange {
		private CotBase cotData;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "Com Short");
				base.OnStateChange();
				Plots[1].Brush = Brushes.Red; // 75%
				Plots[3].Brush = Brushes.Green; // 25%
				Description									= @"COT2";
				Name										= "COT 2";
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
				cotData = CotBase(SuriCotReportField.CommercialShort);
			}
		}
		
		public override string DisplayName {
			get {
				if (Instrument != null)
					return "COT 2 - " + SuriStrings.instrumentToName(Instrument.FullName);
				return "COT 2";
			}
		}
		
		protected override void OnBarUpdate() {
			Values[0][0] = cotData.Value[0];
			CalcMinMax(cotData.Value[0]);

			if (CurrentBar > Days) {
				if (Values[0][0] > ValueOf(0.75)) {
					PlotBrushes[0][0] = Brushes.Red;
				}
				else if (Values[0][0] < ValueOf(0.25)) {
					PlotBrushes[0][0] = Brushes.Green;
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
		private Suri.Cot22[] cacheCot22;
		public Suri.Cot22 Cot22(int days, bool drawDotedLinePara)
		{
			return Cot22(Input, days, drawDotedLinePara);
		}

		public Suri.Cot22 Cot22(ISeries<double> input, int days, bool drawDotedLinePara)
		{
			if (cacheCot22 != null)
				for (int idx = 0; idx < cacheCot22.Length; idx++)
					if (cacheCot22[idx] != null && cacheCot22[idx].Days == days && cacheCot22[idx].DrawDotedLinePara == drawDotedLinePara && cacheCot22[idx].EqualsInput(input))
						return cacheCot22[idx];
			return CacheIndicator<Suri.Cot22>(new Suri.Cot22(){ Days = days, DrawDotedLinePara = drawDotedLinePara }, input, ref cacheCot22);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Cot22 Cot22(int days, bool drawDotedLinePara)
		{
			return indicator.Cot22(Input, days, drawDotedLinePara);
		}

		public Indicators.Suri.Cot22 Cot22(ISeries<double> input , int days, bool drawDotedLinePara)
		{
			return indicator.Cot22(input, days, drawDotedLinePara);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Cot22 Cot22(int days, bool drawDotedLinePara)
		{
			return indicator.Cot22(Input, days, drawDotedLinePara);
		}

		public Indicators.Suri.Cot22 Cot22(ISeries<double> input , int days, bool drawDotedLinePara)
		{
			return indicator.Cot22(input, days, drawDotedLinePara);
		}
	}
}

#endregion
