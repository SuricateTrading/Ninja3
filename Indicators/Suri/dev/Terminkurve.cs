#region Using declarations
using System.Collections.Generic;
using System.Windows.Media;
using NinjaTrader.Custom.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
#endregion



namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class Terminkurve : Indicator {
		private List<TkData> tkData;
		private int nextIndex;
		
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
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "Terminkurve");
			} else if (State == State.DataLoaded) {
				int? id = SuriStrings.GetId(Instrument);
				if (id != null) {
					string oldDate = From.Date.ToString("yyyy-MM-dd");
					string newDate = To.Date.ToString("yyyy-MM-dd");
					tkData = SuriServer.GetTkData(id.Value, oldDate, newDate);
				}
			}
		}
		
		protected override void OnBarUpdate() {
			if (tkData == null) return;
			
			string now = Time[0].Date.ToString("yyyy-MM-dd");
			for (int i = nextIndex; i < tkData.Count; i++) {
				if (tkData[i].Date.Equals(now)) {
					Value[0] = tkData[i].TkState;
					nextIndex = i;
					return;
				}
			}
		}

		public override void OnCalculateMinMax() {
			MinValue = -2;
			MaxValue = 2;
		}
		
	}
}





















































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.Terminkurve[] cacheTerminkurve;
		public Suri.dev.Terminkurve Terminkurve()
		{
			return Terminkurve(Input);
		}

		public Suri.dev.Terminkurve Terminkurve(ISeries<double> input)
		{
			if (cacheTerminkurve != null)
				for (int idx = 0; idx < cacheTerminkurve.Length; idx++)
					if (cacheTerminkurve[idx] != null &&  cacheTerminkurve[idx].EqualsInput(input))
						return cacheTerminkurve[idx];
			return CacheIndicator<Suri.dev.Terminkurve>(new Suri.dev.Terminkurve(), input, ref cacheTerminkurve);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.Terminkurve Terminkurve()
		{
			return indicator.Terminkurve(Input);
		}

		public Indicators.Suri.dev.Terminkurve Terminkurve(ISeries<double> input )
		{
			return indicator.Terminkurve(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.Terminkurve Terminkurve()
		{
			return indicator.Terminkurve(Input);
		}

		public Indicators.Suri.dev.Terminkurve Terminkurve(ISeries<double> input )
		{
			return indicator.Terminkurve(input);
		}
	}
}

#endregion
