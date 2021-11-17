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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.DirectWrite;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class CotBase : Indicator {
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description = Custom.Resource.NinjaScriptIndicatorDescriptionCOT;
				Name = "COT";
				IsSuspendedWhileInactive = true;
				
				SCot = new CotReport { ReportType = CotReportType.Futures, Field = CotReportField.OpenInterest };
				
				AddPlot(Brushes.CornflowerBlue, Custom.Resource.COT1);
			}
		}
		
        public override string DisplayName {
          get {
				if (SCot != null && Instrument != null)
					return "COT " + SCot.Field + " - " + SuriStrings.instrumentToName(Instrument.FullName);
				else
					return "COT Daten";
			}
        }
		
		protected override void OnBarUpdate() {
			if (CotData.GetCotReportNames(Instrument.MasterInstrument.Name).Count == 0) {
				Draw.TextFixed(this, "Error", Custom.Resource.CotDataError, TextPosition.BottomRight);
				return;
			}
			if (!Core.Globals.MarketDataOptions.DownloadCotData)
				Draw.TextFixed(this, "Warning", Custom.Resource.CotDataWarning, TextPosition.BottomRight);

			if (CotData.IsDownloadingData) {
				Draw.TextFixed(this, "Warning", Custom.Resource.CotDataStillDownloading, TextPosition.BottomRight);
				return;
			}
			
			double value = SCot.Calculate(Instrument.MasterInstrument.Name, Time[0]);
			if (!double.IsNaN(value))
				Values[0][0] = value;
		}
		
		#region Properties
		[Browsable(false)]
		public int Cot1Serialize {
			get { return (int)SCot.ReportType * 100 + (int)SCot.Field; }
			set { SCot = new CotReport { ReportType = (CotReportType)(value / 100), Field = (CotReportField)(value % 100) }; }
		}
		
		[NinjaScriptProperty]
		[Display(Name = "COT Daten", GroupName = "Parameter", Order = 1)]
		[XmlIgnore]
		public CotReport SCot { get; set; }
		#endregion
	}
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.CotBase[] cacheCotBase;
		public Suri.CotBase CotBase(CotReport sCot)
		{
			return CotBase(Input, sCot);
		}

		public Suri.CotBase CotBase(ISeries<double> input, CotReport sCot)
		{
			if (cacheCotBase != null)
				for (int idx = 0; idx < cacheCotBase.Length; idx++)
					if (cacheCotBase[idx] != null && cacheCotBase[idx].SCot == sCot && cacheCotBase[idx].EqualsInput(input))
						return cacheCotBase[idx];
			return CacheIndicator<Suri.CotBase>(new Suri.CotBase(){ SCot = sCot }, input, ref cacheCotBase);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.CotBase CotBase(CotReport sCot)
		{
			return indicator.CotBase(Input, sCot);
		}

		public Indicators.Suri.CotBase CotBase(ISeries<double> input , CotReport sCot)
		{
			return indicator.CotBase(input, sCot);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.CotBase CotBase(CotReport sCot)
		{
			return indicator.CotBase(Input, sCot);
		}

		public Indicators.Suri.CotBase CotBase(ISeries<double> input , CotReport sCot)
		{
			return indicator.CotBase(input, sCot);
		}
	}
}

#endregion
