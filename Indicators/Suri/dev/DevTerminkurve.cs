#region Using declarations
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DevTerminkurve : Indicator {
		private List<TkData> tkData;
		private int nextIndex;
		private int days = 250;
		private bool comesFromContango;
		private bool comesFromBackwardation;
		
		[NinjaScriptProperty] [Display(Name="SuriTest", Order=2, GroupName="Parameter")] [Browsable(false)] [XmlIgnore]
		public SuriTest suriTest { get; set; }
		
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
			} else if (State == State.Configure) {
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Line, "Status");
				AddPlot(new Stroke(Brushes.DarkGray, 2), PlotStyle.Line, "Delta");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "Oszillator");
				AddLine(new Stroke(Brushes.DimGray, 1), 50, "Oszillator");
			} else if (State == State.DataLoaded) {
				int? id = SuriStrings.GetId(Instrument);
				if (id != null) {
					string oldDate = Bars.GetTime(0).Date.ToString("yyyy-MM-dd");
					string newDate = Bars.LastBarTime    .Date.ToString("yyyy-MM-dd");
					tkData = SuriServer.GetTkData(id.Value, oldDate, newDate);
				}
			}
		}
		
		//public override void OnCalculateMinMax() { MinValue = -2; MaxValue = 2; }
		public override void OnCalculateMinMax() { MinValue = -25; MaxValue = 125; }
		
		protected override void OnBarUpdate() {
			if (tkData == null) return;
			for (int i = nextIndex; i < tkData.Count; i++) {
				if (tkData[i].Date.Date.Equals(Time[0].Date)) {
					Values[0][0] = (tkData[i].TkState+2)*25;

					if (i >= 1) {
						if (tkData[i-1].TkState >=  1 && tkData[i].TkState <= -2) ExitShort();
						if (tkData[i-1].TkState <= -1 && tkData[i].TkState >=  2) ExitLong();
					}
					
					if (comesFromContango      && tkData[i].TkState <= -1) EnterLong();
					if (comesFromBackwardation && tkData[i].TkState >=  1) EnterShort();
					
					switch (tkData[i].TkState) {
						case  3: comesFromContango = true;  comesFromBackwardation = false; break;
						case -3: comesFromContango = false; comesFromBackwardation = true;  break;
						case  1: case  2:                   comesFromBackwardation = false; break;
						case -1: case -2: comesFromContango = false;                        break;
					}
					
					Values[1][0] = tkData[i].Delta;
					
					// calculate delta osci
					if (CurrentBar >= days) {
						double min = double.MaxValue;
						double max = double.MinValue;
						for (int barsAgo = 0; barsAgo < days; barsAgo++) {
							double v = Values[1][barsAgo];
							if (min > v) min = v;
							if (max < v) max = v;
						}
						Values[2][0] = 100.0 * (tkData[i].Delta - min) / (max - min);
					}
				
					nextIndex = i;
					return;
				}
				if (tkData[i].Date.Date > Time[0].Date) {
					Values[0][0] = Values[0][1];
					Values[1][0] = Values[1][1];
					Values[2][0] = Values[2][1];
					return;
				}
			}
		}

		private double GetStop(bool isLong) {
			return isLong ? Low[0] - TickSize * 200 : High[0] + TickSize * 200;
		}

		private void EnterLong() {
			Draw.VerticalLine(this, "ToBackwardationSignal " + SuriCommon.random, 0, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
			if (suriTest == null) return;
			ExitShort();
			double stop = GetStop(true);
			if (stop < 2000) {
				suriTest.tkOrder = suriTest.SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Market, 1, 0, 0, null, "TK Long");
				suriTest.tkStopLossOrder = suriTest.SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.StopMarket, 1, 0, stop, null, "TK Long Stoploss");	
			}
		}
		private void EnterShort() {
			Draw.VerticalLine(this, "ToContangoSignal " + SuriCommon.random, 0, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
			if (suriTest == null) return;
			ExitLong();
			double stop = GetStop(false);
			if (CurrentBar > 1000 && stop < 2000) {
				suriTest.tkOrder = suriTest.SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, 1, 0, 0, null, "TK Short");
				suriTest.tkStopLossOrder = suriTest.SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, 1, 0, stop, null, "TK Short Stoploss");
			}
		}

		private void ExitLong() {
			if (suriTest == null) return;
			if (suriTest.tkOrder != null && suriTest.tkOrder.IsLong && suriTest.tkStopLossOrder.OrderState != OrderState.Filled) {
				suriTest.SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Market, 1, 0, 0, null,"TK SellShort");
			}
			suriTest.tkOrder = null;
			suriTest.CancelOrder(suriTest.tkStopLossOrder);
			suriTest.tkStopLossOrder = null;
		}
		private void ExitShort() {
			if (suriTest == null) return;
			if (suriTest.tkOrder != null && suriTest.tkOrder.IsShort && suriTest.tkStopLossOrder.OrderState != OrderState.Filled) {
				suriTest.SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, 1, 0, 0, null,"TK BuyToCover");
			}
			suriTest.tkOrder = null;
			suriTest.CancelOrder(suriTest.tkStopLossOrder);
			suriTest.tkStopLossOrder = null;
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
		public Suri.dev.DevTerminkurve DevTerminkurve(SuriTest suriTest)
		{
			return DevTerminkurve(Input, suriTest);
		}

		public Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input, SuriTest suriTest)
		{
			if (cacheDevTerminkurve != null)
				for (int idx = 0; idx < cacheDevTerminkurve.Length; idx++)
					if (cacheDevTerminkurve[idx] != null && cacheDevTerminkurve[idx].suriTest == suriTest && cacheDevTerminkurve[idx].EqualsInput(input))
						return cacheDevTerminkurve[idx];
			return CacheIndicator<Suri.dev.DevTerminkurve>(new Suri.dev.DevTerminkurve(){ suriTest = suriTest }, input, ref cacheDevTerminkurve);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(SuriTest suriTest)
		{
			return indicator.DevTerminkurve(Input, suriTest);
		}

		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input , SuriTest suriTest)
		{
			return indicator.DevTerminkurve(input, suriTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(SuriTest suriTest)
		{
			return indicator.DevTerminkurve(Input, suriTest);
		}

		public Indicators.Suri.dev.DevTerminkurve DevTerminkurve(ISeries<double> input , SuriTest suriTest)
		{
			return indicator.DevTerminkurve(input, suriTest);
		}
	}
}

#endregion
