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

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class COT1 : Indicator {
		
		private CotBase CotData;
		private bool hasGoneAbove = false;
		private bool hasGoneBelow = false;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "COT 1";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				Days										= 125;
				CotData = CotBase(new CotReport { ReportType = CotReportType.Futures, Field = CotReportField.CommercialNet });
				
				AddPlot(new Stroke(Brushes.DarkGray, 2), PlotStyle.Line, "COT1");
				AddLine(new Stroke(Brushes.Red, 2), 10.0, "10%");
				AddLine(Brushes.DimGray, 50.0, "50%");
				AddLine(new Stroke(Brushes.Green, 2), 90.0, "90%");
			}
		}
		
        public override string DisplayName {
          get {
				if (Instrument != null)
					return "COT 1 - " + SuriStrings.instrumentToName(Instrument.FullName);
				else
					return "COT 1";
			}
        }
		
		protected override void OnBarUpdate() {
			if (CurrentBar < Days) return;
			
			double? min = null, max = null;
			for (int barsAgo = 0; barsAgo < Days; barsAgo++) {
				double v = CotData.Value[barsAgo];
				if (min == null || min > v) min = v;
				if (max == null || max < v) max = v;
			}
			
			// min and max cannot be null at this point
			double osci = 100.0 * (CotData.Value[0] - min.Value) / (max.Value - min.Value);
			Value[0] = osci;
			
			if (CurrentBar == Days) return;
			
			// this section colors the starting point of cot 1 signals
			if (osci > 90.0 && Value[1] > 90.0 && hasGoneBelow) {
				SMA sma = SMA(125);
				if (sma[0] > sma[1]) {
					PlotBrushes[0][0] = Brushes.Green;
				} else {
					PlotBrushes[0][0] = Brushes.Yellow;
				}
			} else if (osci < 10.0 && Value[1] < 10.0 && hasGoneAbove) {
				SMA sma = SMA(125);
				if (sma[0] < sma[1]) {
					PlotBrushes[0][0] = Brushes.Red;
				} else {
					PlotBrushes[0][0] = Brushes.Yellow;
				}
			} else {
				if (Value[1] < 10.0 && Value[0] > 10.0) {
					hasGoneBelow = true;
					hasGoneAbove = false;
				}
				if (Value[1] > 90.0 && Value[0] < 90.0) {
					hasGoneAbove = true;
					hasGoneBelow = false;
				}
			}
		}
		
		
		
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int Days
		{ get; set; }
		#endregion
	}
}
























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.COT1[] cacheCOT1;
		public Suri.COT1 COT1(int days)
		{
			return COT1(Input, days);
		}

		public Suri.COT1 COT1(ISeries<double> input, int days)
		{
			if (cacheCOT1 != null)
				for (int idx = 0; idx < cacheCOT1.Length; idx++)
					if (cacheCOT1[idx] != null && cacheCOT1[idx].Days == days && cacheCOT1[idx].EqualsInput(input))
						return cacheCOT1[idx];
			return CacheIndicator<Suri.COT1>(new Suri.COT1(){ Days = days }, input, ref cacheCOT1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.COT1 COT1(int days)
		{
			return indicator.COT1(Input, days);
		}

		public Indicators.Suri.COT1 COT1(ISeries<double> input , int days)
		{
			return indicator.COT1(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.COT1 COT1(int days)
		{
			return indicator.COT1(Input, days);
		}

		public Indicators.Suri.COT1 COT1(ISeries<double> input , int days)
		{
			return indicator.COT1(input, days);
		}
	}
}

#endregion
