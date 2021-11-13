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
		
		private CotReport CotData;
		private CotBase cot2;
		
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
				
				CotData = new CotReport { ReportType = CotReportType.Futures, Field = CotReportField.CommercialNet };
				
				AddPlot(new Stroke(Brushes.DarkOrange, 2), PlotStyle.Line, "COT1");
				AddLine(new Stroke(Brushes.Red, 2), 10.0, "10%");
				AddLine(Brushes.DimGray, 50.0, "50%");
				AddLine(new Stroke(Brushes.Green, 2), 90.0, "90%");
				
				cot2 = CotBase(CotReportField.CommercialNet);
			}
		}
		
		
		private double getCot(int barsAgo) {
			return cot2.Values[0][barsAgo];
			//return CotData.Calculate(Instrument.MasterInstrument.Name, Time[barsAgo]);
		}
		
		protected override void OnBarUpdate() {
			if (CurrentBar < Days) return;
			
			double? min = null, max = null;
			for (int barsAgo = 0; barsAgo < Days; barsAgo++) {
				double v = getCot(barsAgo);
				if (min == null || min > v) min = v;
				if (max == null || max < v) max = v;
			}
			
			// min and max cannot be null at this point
			double osci = 100.0 * (getCot(0) - min.Value) / (max.Value - min.Value);
			OsciPlot[0] = osci;
			
			
			if(osci > 90.0 || osci < 10.0) {
				
				bool signal = false;
				int signalStartindex = 0;
				
				for (int barsAgo = 1; barsAgo <= CurrentBar - Days; barsAgo++) {
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
							PlotBrushes[0][i] = Brushes.Red;
						} else {
							PlotBrushes[0][i] = Brushes.Green;
						}
					}
				}
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int Days
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
