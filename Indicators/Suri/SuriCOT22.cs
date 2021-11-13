#region Using declarations
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class SuriCOT22 : Indicator {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"SuriCOT22";
				Name										= "SuriCOT22";
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
		private Suri.SuriCOT22[] cacheSuriCOT22;
		public Suri.SuriCOT22 SuriCOT22()
		{
			return SuriCOT22(Input);
		}

		public Suri.SuriCOT22 SuriCOT22(ISeries<double> input)
		{
			if (cacheSuriCOT22 != null)
				for (int idx = 0; idx < cacheSuriCOT22.Length; idx++)
					if (cacheSuriCOT22[idx] != null &&  cacheSuriCOT22[idx].EqualsInput(input))
						return cacheSuriCOT22[idx];
			return CacheIndicator<Suri.SuriCOT22>(new Suri.SuriCOT22(), input, ref cacheSuriCOT22);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCOT22 SuriCOT22()
		{
			return indicator.SuriCOT22(Input);
		}

		public Indicators.Suri.SuriCOT22 SuriCOT22(ISeries<double> input )
		{
			return indicator.SuriCOT22(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCOT22 SuriCOT22()
		{
			return indicator.SuriCOT22(Input);
		}

		public Indicators.Suri.SuriCOT22 SuriCOT22(ISeries<double> input )
		{
			return indicator.SuriCOT22(input);
		}
	}
}

#endregion
