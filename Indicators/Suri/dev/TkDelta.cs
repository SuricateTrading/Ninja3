#region Using declarations
using System.Collections.Generic;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class TkDelta : Indicator {
		private List<TkData> tkData;
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
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "TK Delta");
			} else if (State == State.DataLoaded) {
				int? id = SuriStrings.GetId(Instrument);
				if (id != null) {
					string oldDate = From.AddDays(-5).Date.ToString("yyyy-MM-dd");
					string newDate = To  .AddDays( 5).Date.ToString("yyyy-MM-dd");
					tkData = SuriServer.GetTkData(id.Value, oldDate, newDate);
				}
			}
		}
		
		protected override void OnBarUpdate() {
			if (tkData == null) return;
			
			for (int i = nextIndex; i < tkData.Count; i++) {
				if (tkData[i].Date.Date.Equals(Time[0].Date)) {
					Value[0] = tkData[i].Delta;
					nextIndex = i;
					return;
				}
				if (tkData[i].Date.Date > Time[0].Date) {
					Value[0] = Value[1];
					return;
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
