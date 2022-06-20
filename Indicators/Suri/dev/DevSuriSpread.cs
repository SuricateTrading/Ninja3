#region Using declarations

using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DevSuriSpread : Indicator {
		private Bars bars1;
		private Bars bars2;
		
		[NinjaScriptProperty]
		//[Browsable(false)]
		[Display(Name = "Tage", Order = 0)]
		public string market1 { get; set; }
		
		[NinjaScriptProperty]
		//[Browsable(false)]
		[Display(Name = "Tage", Order = 0)]
		public string market2 { get; set; }
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Zeigt den Spread zwischen 2 Kontraktmonaten an.";
				Name										= "Spread";
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
				market1										= "";
				market2										= "";
			} else if (State == State.Configure) {
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Line, "Spread");
				
				if (market1.IsNullOrEmpty() || market2.IsNullOrEmpty()) return;

				new BarsRequest(Instrument.GetInstrument(market1), Bars.GetTime(0), Bars.LastBarTime.Date) {
					MergePolicy = MergePolicy.DoNotMerge,
					BarsPeriod = new BarsPeriod {BarsPeriodType = Bars.BarsPeriod.BarsPeriodType, Value = Bars.BarsPeriod.BaseBarsPeriodValue},
				}.Request((bars, minuteErrorCode, minuteErrorMessage) => {
					bars1 = bars.Bars;
					if (bars2 != null) Update();
				});
				
				new BarsRequest(Instrument.GetInstrument(market2), Bars.GetTime(0), Bars.LastBarTime.Date) {
					MergePolicy = MergePolicy.DoNotMerge,
					BarsPeriod = new BarsPeriod {BarsPeriodType = Bars.BarsPeriod.BarsPeriodType, Value = Bars.BarsPeriod.BaseBarsPeriodValue},
				}.Request((bars, minuteErrorCode, minuteErrorMessage) => {
					bars2 = bars.Bars;
					if (bars1 != null) Update();
				});
			}
		}
		public override string DisplayName { get { return Name; } }

		protected override void OnBarUpdate() {
			if (bars1 == null || bars2 == null) {
				return;
			}
			try {
				Value[0] = bars1.GetClose(CurrentBar) - bars2.GetClose(CurrentBar);
			} catch (Exception) {
				Print("error line " + CurrentBar);
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
		private Suri.dev.DevSuriSpread[] cacheDevSuriSpread;
		public Suri.dev.DevSuriSpread DevSuriSpread(string market1, string market2)
		{
			return DevSuriSpread(Input, market1, market2);
		}

		public Suri.dev.DevSuriSpread DevSuriSpread(ISeries<double> input, string market1, string market2)
		{
			if (cacheDevSuriSpread != null)
				for (int idx = 0; idx < cacheDevSuriSpread.Length; idx++)
					if (cacheDevSuriSpread[idx] != null && cacheDevSuriSpread[idx].market1 == market1 && cacheDevSuriSpread[idx].market2 == market2 && cacheDevSuriSpread[idx].EqualsInput(input))
						return cacheDevSuriSpread[idx];
			return CacheIndicator<Suri.dev.DevSuriSpread>(new Suri.dev.DevSuriSpread(){ market1 = market1, market2 = market2 }, input, ref cacheDevSuriSpread);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevSuriSpread DevSuriSpread(string market1, string market2)
		{
			return indicator.DevSuriSpread(Input, market1, market2);
		}

		public Indicators.Suri.dev.DevSuriSpread DevSuriSpread(ISeries<double> input , string market1, string market2)
		{
			return indicator.DevSuriSpread(input, market1, market2);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevSuriSpread DevSuriSpread(string market1, string market2)
		{
			return indicator.DevSuriSpread(Input, market1, market2);
		}

		public Indicators.Suri.dev.DevSuriSpread DevSuriSpread(ISeries<double> input , string market1, string market2)
		{
			return indicator.DevSuriSpread(input, market1, market2);
		}
	}
}

#endregion
