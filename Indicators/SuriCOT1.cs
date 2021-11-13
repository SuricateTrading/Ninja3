#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators {
	public class SuriCOT1 : Indicator {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "SuriCOT1";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event.
				IsSuspendedWhileInactive					= true;
				Period										= 125;
				
				AddPlot(new Stroke(Brushes.DarkOrange, 2), PlotStyle.Line, "SuriCOT1");
				AddLine(new Stroke(Brushes.Red, 2), 10.0, "10%");
				AddLine(Brushes.DimGray, 50.0, "50%");
				AddLine(new Stroke(Brushes.Green, 2), 90.0, "90%");
			}
		}
		
		protected override void OnBarUpdate() {
			if (CurrentBar < Period) return;
			
			double? min = null, max = null;
			for (int barsAgo = 0; barsAgo < Period; barsAgo++) {
				double v = Input[barsAgo];
				if (min == null || min > v) min = v;
				if (max == null || max < v) max = v;
			}
			
			// min and max cannot be null at this point
			double osci = 100.0 * (Input[0] - min.Value) / (max.Value - min.Value);
			OsciPlot[0] = osci;
			
			
			if(osci > 90.0 || osci < 10.0) {
				
				bool signal = false;
				int signalStartindex = 0;
				
				for (int barsAgo = 1; barsAgo <= CurrentBar - Period; barsAgo++) {
					double v = OsciPlot[barsAgo];
					
					if (v > 90.0 && osci > 90.0 || v < 10.0 && osci < 10.0) {
						signal = false;
						break;
					}
					if (v < 10.0 && osci > 90.0 || v > 90.0 && osci < 10.0) {
						signal = true;
					}
					if (signal && (v > 10.0 && osci > 90.0 || v < 90.0 && osci < 10.0)) {
						signalStartindex = barsAgo;
						break;
					}
				}
				if (signal) {
					for (int i = 0; i < signalStartindex; i++) {
						
						/*
						todo
						
						SMA sma = SMA(125);
						if ( sma[0] > sma[1] && osci > 90.0 || sma[0] > sma[1] && osci > 90.0 ) {
							PlotBrushes[0][i] = Brushes.Gray;
						} else*/ if (osci > 90.0) {
							PlotBrushes[0][i] = Brushes.Green;
						} else {
							PlotBrushes[0][i] = Brushes.Red;
						}
					}
				}
			}
			
		}

		#region Properties

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Wochen", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OsciPlot {
			get { return Values[0]; }
		}

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SuriCOT1[] cacheSuriCOT1;
		public SuriCOT1 SuriCOT1(int period)
		{
			return SuriCOT1(Input, period);
		}

		public SuriCOT1 SuriCOT1(ISeries<double> input, int period)
		{
			if (cacheSuriCOT1 != null)
				for (int idx = 0; idx < cacheSuriCOT1.Length; idx++)
					if (cacheSuriCOT1[idx] != null && cacheSuriCOT1[idx].Period == period && cacheSuriCOT1[idx].EqualsInput(input))
						return cacheSuriCOT1[idx];
			return CacheIndicator<SuriCOT1>(new SuriCOT1(){ Period = period }, input, ref cacheSuriCOT1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SuriCOT1 SuriCOT1(int period)
		{
			return indicator.SuriCOT1(Input, period);
		}

		public Indicators.SuriCOT1 SuriCOT1(ISeries<double> input , int period)
		{
			return indicator.SuriCOT1(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SuriCOT1 SuriCOT1(int period)
		{
			return indicator.SuriCOT1(Input, period);
		}

		public Indicators.SuriCOT1 SuriCOT1(ISeries<double> input , int period)
		{
			return indicator.SuriCOT1(input, period);
		}
	}
}

#endregion
