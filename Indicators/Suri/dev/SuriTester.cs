#region Using declarations

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class SuriTester : Indicator {
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "SuriTester";
				BarsRequiredToPlot							= 0;
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "GC 02-16");
			} else if (State == State.Configure) {
				 AddDataSeries("GC 02-20", BarsPeriodType.Day, 1);
			}
		}

		protected override void OnBarUpdate() {
	    	if (BarsInProgress == 1 || BarsInProgress == 2) return;
			Value[0] = Closes[1][0];
		}
		
		#region Properties
        [TypeConverter(typeof(FriendlyEnumConverter))]
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")]
		[NinjaScriptProperty]
		[Display(Name = "COT Daten", GroupName = "Parameter")]
		[XmlIgnore]
		public String ReportField { get; set; }
		#endregion
	}
}

































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.SuriTester[] cacheSuriTester;
		public Suri.dev.SuriTester SuriTester(String reportField)
		{
			return SuriTester(Input, reportField);
		}

		public Suri.dev.SuriTester SuriTester(ISeries<double> input, String reportField)
		{
			if (cacheSuriTester != null)
				for (int idx = 0; idx < cacheSuriTester.Length; idx++)
					if (cacheSuriTester[idx] != null && cacheSuriTester[idx].ReportField == reportField && cacheSuriTester[idx].EqualsInput(input))
						return cacheSuriTester[idx];
			return CacheIndicator<Suri.dev.SuriTester>(new Suri.dev.SuriTester(){ ReportField = reportField }, input, ref cacheSuriTester);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.SuriTester SuriTester(String reportField)
		{
			return indicator.SuriTester(Input, reportField);
		}

		public Indicators.Suri.dev.SuriTester SuriTester(ISeries<double> input , String reportField)
		{
			return indicator.SuriTester(input, reportField);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.SuriTester SuriTester(String reportField)
		{
			return indicator.SuriTester(Input, reportField);
		}

		public Indicators.Suri.dev.SuriTester SuriTester(ISeries<double> input , String reportField)
		{
			return indicator.SuriTester(input, reportField);
		}
	}
}

#endregion
