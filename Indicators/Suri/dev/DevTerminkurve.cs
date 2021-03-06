#region Using declarations

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DevTerminkurve : Indicator {
		private TkRepo tkRepo;
		
		[NinjaScriptProperty]
		[Browsable(false)]
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
				days = 150;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Line, "Status");
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "Delta");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "Oszillator");
				AddLine(new Stroke(Brushes.DimGray, 1), 10, "0");
				AddLine(new Stroke(Brushes.DimGray, 1), 50, "50");
				AddLine(new Stroke(Brushes.DimGray, 1), 90, "100");
			} else if (State == State.DataLoaded) {
				if (Bars.Count > 0) tkRepo = new TkRepo(Instrument, Bars);
			}
		}
		public override void OnCalculateMinMax() { MinValue = 0; MaxValue = 120; }
		public override string DisplayName { get { return Name; } }

		public TkState GetTkState(int barIndex) {
			var tkData = tkRepo.Get(barIndex);
			return tkData == null ? TkState.None : tkData.tkState;
		}
		public TkData GetTkData(int barIndex) { return tkRepo.Get(barIndex); }
		
		protected override void OnBarUpdate() {
			TkData tkData = tkRepo.Get(CurrentBar);
			if (tkData == null) return;

			switch (tkData.tkState) {
				case TkState.Backwardation:				Values[0][0] = 0; break;
				case TkState.FirstHighestAndLastLowest:	Values[0][0] = 17; break;
				case TkState.FirstHigherThanLast:		Values[0][0] = 34; break;
				case TkState.None:						Values[0][0] = 50; break;
				case TkState.FirstLowerThanLast:		Values[0][0] = 67; break;
				case TkState.FirstLowestAndLastHighest:	Values[0][0] = 84; break;
				case TkState.Contango:					Values[0][0] = 100; break;
				case TkState.FirstThreeContango:		Values[0][0] = 110; break;
			}
			//Values[0][0] = (int) tkData.tkState * 10;
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
			} else {
				Values[2][0] = double.NaN;
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
		public Suri.dev.DevTerminkurve DevTerminkurve(int days)
		{
			return DevTerminkurve(Input, days);
		}

		public Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input, int days)
		{
			if (cacheDevTerminkurve != null)
				for (int idx = 0; idx < cacheDevTerminkurve.Length; idx++)
					if (cacheDevTerminkurve[idx] != null && cacheDevTerminkurve[idx].days == days && cacheDevTerminkurve[idx].EqualsInput(input))
						return cacheDevTerminkurve[idx];
			return CacheIndicator<Suri.dev.DevTerminkurve>(new Suri.dev.DevTerminkurve(){ days = days }, input, ref cacheDevTerminkurve);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(int days)
		{
			return indicator.DevTerminkurve(Input, days);
		}

		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input , int days)
		{
			return indicator.DevTerminkurve(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(int days)
		{
			return indicator.DevTerminkurve(Input, days);
		}

		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input , int days)
		{
			return indicator.DevTerminkurve(input, days);
		}
	}
}

#endregion
