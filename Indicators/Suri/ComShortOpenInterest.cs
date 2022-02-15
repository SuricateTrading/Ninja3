#region Using declarations
using System.ComponentModel;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class ComShortOpenInterest : Indicator {
		private CotBase comShort;
		private CotBase openInterest;
		
		private double min = double.MaxValue;
		private double max = double.MinValue;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Commercials Short geteilt durch Open Interest";
				Name										= "Com Short / OI in %";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				drawLines									= true;
				brush25										= Brushes.Yellow;
				brush75										= Brushes.Yellow;
				comShort									= CotBase(SuriCotReportField.CommercialShort);
				openInterest								= CotBase(SuriCotReportField.OpenInterest);
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "Com Short / OI in %");
			} else if (State == State.Configure) {
				if (drawLines) {
					AddLine(brush25, 1, "25%");
					AddLine(brush75, 1, "75%");
				}
			}
		}

		protected override void OnBarUpdate() {
			Values[0][0] = 100 * comShort.Value[0] / openInterest.Value[0];
			if (min > Values[0][0]) min = Values[0][0];
			if (max < Values[0][0]) max = Values[0][0];
			if (drawLines) {
				Lines[0].Value = ValueOf(0.25);
				Lines[1].Value = ValueOf(0.75);
			}
		}
		private double ValueOf(double percent) { return min + percent * (max - min); }
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Zeichne 25 - 75% Linien", Order=0, GroupName="Parameter")]
		public bool drawLines { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Farbe der 25% Linie", Order = 1, GroupName = "Parameter")]
		public Brush brush25 { get; set; }
		[Browsable(false)]
		public string brushSerialize25 {
			get { return Serialize.BrushToString(brush25); }
			set { brush25 = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Farbe der 75% Linie", Order = 2, GroupName = "Parameter")]
		public Brush brush75 { get; set; }
		[Browsable(false)]
		public string brushSerialize75 {
			get { return Serialize.BrushToString(brush75); }
			set { brush75 = Serialize.StringToBrush(value); }
		}
		#endregion
		
	}
}


































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.ComShortOpenInterest[] cacheComShortOpenInterest;
		public Suri.ComShortOpenInterest ComShortOpenInterest(bool drawLines)
		{
			return ComShortOpenInterest(Input, drawLines);
		}

		public Suri.ComShortOpenInterest ComShortOpenInterest(ISeries<double> input, bool drawLines)
		{
			if (cacheComShortOpenInterest != null)
				for (int idx = 0; idx < cacheComShortOpenInterest.Length; idx++)
					if (cacheComShortOpenInterest[idx] != null && cacheComShortOpenInterest[idx].drawLines == drawLines && cacheComShortOpenInterest[idx].EqualsInput(input))
						return cacheComShortOpenInterest[idx];
			return CacheIndicator<Suri.ComShortOpenInterest>(new Suri.ComShortOpenInterest(){ drawLines = drawLines }, input, ref cacheComShortOpenInterest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest(bool drawLines)
		{
			return indicator.ComShortOpenInterest(Input, drawLines);
		}

		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest(ISeries<double> input , bool drawLines)
		{
			return indicator.ComShortOpenInterest(input, drawLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest(bool drawLines)
		{
			return indicator.ComShortOpenInterest(Input, drawLines);
		}

		public Indicators.Suri.ComShortOpenInterest ComShortOpenInterest(ISeries<double> input , bool drawLines)
		{
			return indicator.ComShortOpenInterest(input, drawLines);
		}
	}
}

#endregion
