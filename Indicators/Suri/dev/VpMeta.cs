#region Using declarations

using System.IO;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class VpMeta : Indicator {
		private VpIntraData vpIntraData;
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
				
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Bar, "Status");
				AddPlot(new Stroke(Brushes.DarkGray, 2), PlotStyle.Line, "Delta");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "Oszillator");
			} else if (State == State.DataLoaded && !Bars.IsTickReplay && SuriAddOn.license == License.Dev) {
				string json = File.ReadAllText(VpSerialization.dbPath + @"\" + Instrument.MasterInstrument.Name + ".vpintra");
				vpIntraData = Newtonsoft.Json.JsonConvert.DeserializeObject<VpIntraData>(json);
			}
		}
		
		//public override void OnCalculateMinMax() { MinValue = -2; MaxValue = 2; }
		
		protected override void OnBarUpdate() {
			if (vpIntraData == null || vpIntraData.barData.IsNullOrEmpty()) return;
			
			for (; lastBar <= vpIntraData.barData.Count; lastBar++) {
				if (lastBar == vpIntraData.barData.Count) return;
				if (vpIntraData.barData[lastBar].dateTime.Date == Time[0].Date) break;
			}
			//RenderTarget.Draw

			Values[0][0] = 100 * vpIntraData.barData[lastBar].delta / vpIntraData.barData[lastBar].totalVolume;
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
		private Suri.dev.VpMeta[] cacheVpMeta;
		public Suri.dev.VpMeta VpMeta()
		{
			return VpMeta(Input);
		}

		public Suri.dev.VpMeta VpMeta(ISeries<double> input)
		{
			if (cacheVpMeta != null)
				for (int idx = 0; idx < cacheVpMeta.Length; idx++)
					if (cacheVpMeta[idx] != null &&  cacheVpMeta[idx].EqualsInput(input))
						return cacheVpMeta[idx];
			return CacheIndicator<Suri.dev.VpMeta>(new Suri.dev.VpMeta(), input, ref cacheVpMeta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.VpMeta VpMeta()
		{
			return indicator.VpMeta(Input);
		}

		public Indicators.Suri.dev.VpMeta VpMeta(ISeries<double> input )
		{
			return indicator.VpMeta(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.VpMeta VpMeta()
		{
			return indicator.VpMeta(Input);
		}

		public Indicators.Suri.dev.VpMeta VpMeta(ISeries<double> input )
		{
			return indicator.VpMeta(input);
		}
	}
}

#endregion
