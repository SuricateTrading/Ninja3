#region Using declarations

using System.IO;
using System.Linq;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DevVpMeta : Indicator {
		private SuriVpIntraData suriVpIntraData;
		private int lastBar;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "VpMeta";
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
				
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Bar, "Delta %");
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Bar, "0");
			} else if (State == State.DataLoaded) {
				string json = File.ReadAllText(SuriRepo.dbPath + @"\" + Instrument.MasterInstrument.Name + ".vpintra");
				//suriVpIntraData = Newtonsoft.Json.JsonConvert.DeserializeObject<SuriVpIntraData>(json);
			}
		}
		
		//public override void OnCalculateMinMax() { MinValue = -2; MaxValue = 2; }
		
		protected override void OnBarUpdate() {
			if (suriVpIntraData == null || suriVpIntraData.barData.IsNullOrEmpty()) return;
			Print(suriVpIntraData.barData.First().dateTime);
			
			for (; lastBar <= suriVpIntraData.barData.Count; lastBar++) {
				if (lastBar == suriVpIntraData.barData.Count) return;
				if (suriVpIntraData.barData[lastBar].dateTime.Date == Time[0].Date) break;
			}
			
			Values[0][0] = 100 * suriVpIntraData.barData[lastBar].delta / suriVpIntraData.barData[lastBar].totalVolume;
			if      (Values[0][0] > 0) PlotBrushes[0][0] = Brushes.Green;
			else if (Values[0][0] < 0) PlotBrushes[0][0] = Brushes.Red;
			else                       PlotBrushes[0][0] = Brushes.Yellow;
			Values[1][0] = 0;
		}
		
	}
}





















































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.DevVpMeta[] cacheDevVpMeta;
		public Suri.dev.DevVpMeta DevVpMeta()
		{
			return DevVpMeta(Input);
		}

		public Suri.dev.DevVpMeta DevVpMeta(ISeries<double> input)
		{
			if (cacheDevVpMeta != null)
				for (int idx = 0; idx < cacheDevVpMeta.Length; idx++)
					if (cacheDevVpMeta[idx] != null &&  cacheDevVpMeta[idx].EqualsInput(input))
						return cacheDevVpMeta[idx];
			return CacheIndicator<Suri.dev.DevVpMeta>(new Suri.dev.DevVpMeta(), input, ref cacheDevVpMeta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevVpMeta DevVpMeta()
		{
			return indicator.DevVpMeta(Input);
		}

		public Indicators.Suri.dev.DevVpMeta DevVpMeta(ISeries<double> input )
		{
			return indicator.DevVpMeta(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevVpMeta DevVpMeta()
		{
			return indicator.DevVpMeta(Input);
		}

		public Indicators.Suri.dev.DevVpMeta DevVpMeta(ISeries<double> input )
		{
			return indicator.DevVpMeta(input);
		}
	}
}

#endregion
