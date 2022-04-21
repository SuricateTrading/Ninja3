#region Using declarations

using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class SuriBidAskDelta : Indicator {
		private SuriVpIntraData suriVpIntraData = new SuriVpIntraData();
		private int lastBarStored;
		private int lastBarLoaded;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Bid Ask Delta";
				Name										= "Bid Ask Delta";
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
				AddPlot(new Stroke(Brushes.Gray, 1), PlotStyle.Line, "0");
			} else if (State == State.DataLoaded && !Bars.IsTickReplay && SuriAddOn.license == License.Dev) {
				//string json = File.ReadAllText(SuriVpSerialization.dbPath + @"\" + Instrument.MasterInstrument.Name + ".vpintra");
				//suriVpIntraData = JsonSerializer.Deserialize<SuriVpIntraData>(json); // Newtonsoft.Json.JsonConvert.DeserializeObject<SuriVpIntraData>(json);
				suriVpIntraData = SuriVpSerialization.GetVpIntra(Instrument, Bars.GetTime(0).Date, Bars.LastBarTime.Date);
			}
		}
		
		protected override void OnMarketData(MarketDataEventArgs e) {
			if (SuriAddOn.license == License.None || Bars.Count <= 0 || !Bars.IsTickReplay) return;
			if (lastBarStored != CurrentBar) {
				lastBarStored = CurrentBar;
				suriVpIntraData.barData.Add(new SuriVpBarData(TickSize, e.Time));
			}
			suriVpIntraData.barData.Last().AddTick(e);
		}
		
		protected override void OnBarUpdate() {
			if (suriVpIntraData == null || suriVpIntraData.barData.IsNullOrEmpty()) return;
			
			for (; lastBarLoaded < suriVpIntraData.barData.Count; lastBarLoaded++) {
				if (suriVpIntraData.barData[lastBarLoaded].dateTime.Date == Time[0].Date) break;
			}
			if (lastBarLoaded == suriVpIntraData.barData.Count) return;
			
			Values[0][0] = 100 * suriVpIntraData.barData[lastBarLoaded].delta / suriVpIntraData.barData[lastBarLoaded].totalVolume;
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
		private Suri.SuriBidAskDelta[] cacheSuriBidAskDelta;
		public Suri.SuriBidAskDelta SuriBidAskDelta()
		{
			return SuriBidAskDelta(Input);
		}

		public Suri.SuriBidAskDelta SuriBidAskDelta(ISeries<double> input)
		{
			if (cacheSuriBidAskDelta != null)
				for (int idx = 0; idx < cacheSuriBidAskDelta.Length; idx++)
					if (cacheSuriBidAskDelta[idx] != null &&  cacheSuriBidAskDelta[idx].EqualsInput(input))
						return cacheSuriBidAskDelta[idx];
			return CacheIndicator<Suri.SuriBidAskDelta>(new Suri.SuriBidAskDelta(), input, ref cacheSuriBidAskDelta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriBidAskDelta SuriBidAskDelta()
		{
			return indicator.SuriBidAskDelta(Input);
		}

		public Indicators.Suri.SuriBidAskDelta SuriBidAskDelta(ISeries<double> input )
		{
			return indicator.SuriBidAskDelta(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriBidAskDelta SuriBidAskDelta()
		{
			return indicator.SuriBidAskDelta(Input);
		}

		public Indicators.Suri.SuriBidAskDelta SuriBidAskDelta(ISeries<double> input )
		{
			return indicator.SuriBidAskDelta(input);
		}
	}
}

#endregion
