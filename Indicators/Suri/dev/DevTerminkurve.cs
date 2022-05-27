#region Using declarations
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DevTerminkurve : Indicator {
		private TkRepo tkRepo;
		
		[Display(Name = "Tage", Order = 0)]
		public int days { get; set; }
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Terminkurve";
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
				days = 250;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Line, "Status");
				AddPlot(new Stroke(Brushes.DarkGray, 2), PlotStyle.Line, "Delta");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "Oszillator");
				AddLine(new Stroke(Brushes.DimGray, 1), 10, "0");
				AddLine(new Stroke(Brushes.DimGray, 1), 50, "50");
				AddLine(new Stroke(Brushes.DimGray, 1), 90, "100");
			} else if (State == State.DataLoaded) {
				if (Bars.Count > 0) tkRepo = new TkRepo(Instrument, Bars);
			}
		}
		public override void OnCalculateMinMax() { MinValue = -25; MaxValue = 125; }
		public override string DisplayName { get { return Name; } }

		public TkState GetTkState(int barIndex) {
			var tkData = (TkData) tkRepo.Get(barIndex);
			if (tkData == null) return TkState.None;
			return tkData.tkState;
		}
		public TkData GetTkData(int barIndex) { return (TkData) tkRepo.Get(barIndex); }
		
		protected override void OnBarUpdate() {
			TkData tkData = (TkData) tkRepo.Get(CurrentBar);
			if (tkData == null) return;
			
			Values[0][0] = (int) tkData.tkState * 10;
			Values[1][0] = tkData.delta;
			
			// calculate delta osci
			if (CurrentBar >= days) {
				var min = double.MaxValue;
				var max = double.MinValue;
				for (int barsAgo = 0; barsAgo < days; barsAgo++) {
					double v = Values[1][barsAgo];
					if (min > v) min = v;
					if (max < v) max = v;
				}
				Values[2][0] = 100.0 * (tkData.delta - min) / (max - min);
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
		private Suri.dev.DevTerminkurve[] cacheDevTerminkurve;
		public Suri.dev.DevTerminkurve DevTerminkurve()
		{
			return DevTerminkurve(Input);
		}

		public Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input)
		{
			if (cacheDevTerminkurve != null)
				for (int idx = 0; idx < cacheDevTerminkurve.Length; idx++)
					if (cacheDevTerminkurve[idx] != null &&  cacheDevTerminkurve[idx].EqualsInput(input))
						return cacheDevTerminkurve[idx];
			return CacheIndicator<Suri.dev.DevTerminkurve>(new Suri.dev.DevTerminkurve(), input, ref cacheDevTerminkurve);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve()
		{
			return indicator.DevTerminkurve(Input);
		}

		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input )
		{
			return indicator.DevTerminkurve(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve()
		{
			return indicator.DevTerminkurve(Input);
		}

		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input )
		{
			return indicator.DevTerminkurve(input);
		}
	}
}

#endregion
