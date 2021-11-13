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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators {
	public class SuriCOT2 : Indicator {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"SuriCOT2";
				Name										= "SuriCOT2";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				AddPlot(new Stroke(Brushes.DarkOrange, 2), PlotStyle.Line, "Com Net");
				AddLine(new Stroke(Brushes.Red, 2), 0, "75%");
				AddLine(Brushes.DimGray, 0, "50%");
				AddLine(new Stroke(Brushes.Green, 2), 0, "25%");
			}
		}

		protected override void OnBarUpdate() {
			MyPlot[0] = Input[0];
			
			if (Max==null) Max = Input[0];
			if (Min==null) Min = Input[0];
			
			if (Max < Input[0]) {
				Max = Input[0];
				redrawLines();
			}
			if (Min > Input[0]) {
				Min = Input[0];
				redrawLines();
			}
		}

		private void redrawLines() {
			Lines[0].Value = Min.Value + 0.75 * (Max.Value - Min.Value);
			Lines[1].Value = Min.Value + 0.5 * (Max.Value - Min.Value);
			Lines[2].Value = Min.Value + 0.25 * (Max.Value - Min.Value);
		}
		
		
		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MyPlot {
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public DrawingTools.Line Line75 { get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public DrawingTools.Line Line25 { get; set; }
		
		
		[Browsable(false)]
		[XmlIgnore]
		public double? Max { get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public double? Min { get; set; }
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SuriCOT2[] cacheSuriCOT2;
		public SuriCOT2 SuriCOT2()
		{
			return SuriCOT2(Input);
		}

		public SuriCOT2 SuriCOT2(ISeries<double> input)
		{
			if (cacheSuriCOT2 != null)
				for (int idx = 0; idx < cacheSuriCOT2.Length; idx++)
					if (cacheSuriCOT2[idx] != null &&  cacheSuriCOT2[idx].EqualsInput(input))
						return cacheSuriCOT2[idx];
			return CacheIndicator<SuriCOT2>(new SuriCOT2(), input, ref cacheSuriCOT2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SuriCOT2 SuriCOT2()
		{
			return indicator.SuriCOT2(Input);
		}

		public Indicators.SuriCOT2 SuriCOT2(ISeries<double> input )
		{
			return indicator.SuriCOT2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SuriCOT2 SuriCOT2()
		{
			return indicator.SuriCOT2(Input);
		}

		public Indicators.SuriCOT2 SuriCOT2(ISeries<double> input )
		{
			return indicator.SuriCOT2(input);
		}
	}
}

#endregion
