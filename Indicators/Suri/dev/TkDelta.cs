#region Using declarations
using System.Collections.Generic;
using System.Windows.Media;
using NinjaTrader.Custom.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class TkDelta : Indicator {
		private List<TkData> tkDeltaData;
		private int nextIndex;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "TK Delta";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				BarsRequiredToPlot							= 0;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "TK Delta");
			} else if (State == State.DataLoaded) {
				int? id = SuriStrings.GetId(Instrument);
				if (id != null) {
					string oldDate = From.Date.ToString("yyyy-MM-dd");
					string newDate = To.Date.ToString("yyyy-MM-dd");
					tkDeltaData = SuriServer.GetTkData(id.Value, oldDate, newDate);
				}
			}
		}
		
		protected override void OnBarUpdate() {
			if (tkDeltaData == null) return;
			
			string now = Time[0].Date.ToString("yyyy-MM-dd");
			for (int i = nextIndex; i < tkDeltaData.Count; i++) {
				if (tkDeltaData[i].Date.Equals(now)) {
					Value[0] = tkDeltaData[i].Delta;
					nextIndex = i;
					return;
				}
			}
		}
/*
		public override void OnCalculateMinMax() {
			MinValue = 0;
			MaxValue = 100;
		}*/

	}
}























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.TkDelta[] cacheTkDelta;
		public Suri.dev.TkDelta TkDelta()
		{
			return TkDelta(Input);
		}

		public Suri.dev.TkDelta TkDelta(ISeries<double> input)
		{
			if (cacheTkDelta != null)
				for (int idx = 0; idx < cacheTkDelta.Length; idx++)
					if (cacheTkDelta[idx] != null &&  cacheTkDelta[idx].EqualsInput(input))
						return cacheTkDelta[idx];
			return CacheIndicator<Suri.dev.TkDelta>(new Suri.dev.TkDelta(), input, ref cacheTkDelta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.TkDelta TkDelta()
		{
			return indicator.TkDelta(Input);
		}

		public Indicators.Suri.dev.TkDelta TkDelta(ISeries<double> input )
		{
			return indicator.TkDelta(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.TkDelta TkDelta()
		{
			return indicator.TkDelta(Input);
		}

		public Indicators.Suri.dev.TkDelta TkDelta(ISeries<double> input )
		{
			return indicator.TkDelta(input);
		}
	}
}

#endregion
