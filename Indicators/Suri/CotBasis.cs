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
	public class CotBasis : Indicator {
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description = Custom.Resource.NinjaScriptIndicatorDescriptionCOT;
				Name = "Cot";
				IsSuspendedWhileInactive = true;
				
				SCot = new CotReport { ReportType = CotReportType.Futures, Field = CotReportField.OpenInterest };
				
				
				foreach (CotReportField t in Enum.GetValues(typeof(CotReportField))) {
					Print(t + " " + (int)t);
				}
				
				AddPlot(Brushes.CornflowerBlue, Custom.Resource.COT1);
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
		
		[Browsable(false)]
		[XmlIgnore]
		public CotReport SCot {  get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "Report Feld", GroupName = "Parameter", Order = 1)]
		//[TypeConverter(typeof(RangeEnumConverter))]
		//[EnumDataType(typeof(CotReportField))]
		//[RegularExpression(@"0")]
		[NoStringInListBiggerThanAttribute]
		[XmlIgnore]
		public CotReportField SReportField  {
			get { return SCot.Field; }
			set { SCot.Field = value; }
		}
		/*
		[NinjaScriptProperty]
		[Range(0, 1)]
		[Display(Name = "kk", GroupName = "Parameter", Order = 1)]
		[TypeConverter(typeof(RangeEnumConverter))]
		[RefreshProperties(RefreshProperties.All)]
		[XmlIgnore]
		public Test hghg  {
			get;
			set;
		}
		
		
		[Range(1, 5)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumberOfCotPlots", GroupName = "NinjaScriptParameters", Order = 0)]
		[TypeConverter(typeof(RangeEnumConverter))]
		[RefreshProperties(RefreshProperties.All)]
		public int Number { get; set; }
		*/
		
		#endregion
	}
	
	
	
	
	
	
}

public class NoStringInListBiggerThanAttribute : ValidationAttribute {
    

    public NoStringInListBiggerThanAttribute() {
        
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
        return new ValidationResult("The following strings");
    }
}




public enum Test {
	K0 = 0,
	K1 = 1,
	K2 = 2
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.CotBasis[] cacheCotBasis;
		public Suri.CotBasis CotBasis(CotReportField sReportField)
		{
			return CotBasis(Input, sReportField);
		}

		public Suri.CotBasis CotBasis(ISeries<double> input, CotReportField sReportField)
		{
			if (cacheCotBasis != null)
				for (int idx = 0; idx < cacheCotBasis.Length; idx++)
					if (cacheCotBasis[idx] != null && cacheCotBasis[idx].SReportField == sReportField && cacheCotBasis[idx].EqualsInput(input))
						return cacheCotBasis[idx];
			return CacheIndicator<Suri.CotBasis>(new Suri.CotBasis(){ SReportField = sReportField }, input, ref cacheCotBasis);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.CotBasis CotBasis(CotReportField sReportField)
		{
			return indicator.CotBasis(Input, sReportField);
		}

		public Indicators.Suri.CotBasis CotBasis(ISeries<double> input , CotReportField sReportField)
		{
			return indicator.CotBasis(input, sReportField);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.CotBasis CotBasis(CotReportField sReportField)
		{
			return indicator.CotBasis(Input, sReportField);
		}

		public Indicators.Suri.CotBasis CotBasis(ISeries<double> input , CotReportField sReportField)
		{
			return indicator.CotBasis(input, sReportField);
		}
	}
}

#endregion
