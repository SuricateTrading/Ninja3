#region Using declarations
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class COT22 : Indicator {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"COT22";
				Name										= "COT 22";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
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
		private Suri.COT22[] cacheCOT22;
		public Suri.COT22 COT22()
		{
			return COT22(Input);
		}

		public Suri.COT22 COT22(ISeries<double> input)
		{
			if (cacheCOT22 != null)
				for (int idx = 0; idx < cacheCOT22.Length; idx++)
					if (cacheCOT22[idx] != null &&  cacheCOT22[idx].EqualsInput(input))
						return cacheCOT22[idx];
			return CacheIndicator<Suri.COT22>(new Suri.COT22(), input, ref cacheCOT22);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.COT22 COT22()
		{
			return indicator.COT22(Input);
		}

		public Indicators.Suri.COT22 COT22(ISeries<double> input )
		{
			return indicator.COT22(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.COT22 COT22()
		{
			return indicator.COT22(Input);
		}

		public Indicators.Suri.COT22 COT22(ISeries<double> input )
		{
			return indicator.COT22(input);
		}
	}
}

#endregion
