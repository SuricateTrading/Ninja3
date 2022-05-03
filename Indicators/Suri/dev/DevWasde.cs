#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DevWasde : Indicator {
		private List<WasdeData> wasdeData;
		private int nextIndex;
		private bool hasStarted;
		
		#region Parameter
		[NinjaScriptProperty]
		[Display(Name="Wasde Feld", Order=0, GroupName="Parameter")]
		public WasdeField field
		{ get; set; }
		private string fieldText;
		
		[NinjaScriptProperty]
		[Display(Name="Zeige alte Ernte", Order=2, GroupName="Parameter")]
		public bool showOldCrop
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Zeige berechnete Ernte", Order=3, GroupName="Parameter")]
		public bool showEstimatedCrop
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Zeige zukünftige Ernte", Order=4, GroupName="Parameter")]
		public bool showProjectedCrop
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Nur Amerika (an) oder gesamte Welt (aus)", Order=1, GroupName="Parameter")]
		public bool isAmerica
		{ get; set; }
		
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der Linie", Order=2, GroupName="Parameter")]
		public int lineWidth
		{ get; set; }
		
		#region Colors
		[Display(Name = "Alte Ernte", Order = 0, GroupName = "Farben")]
		public Brush oldCropBrush { get; set; }
		[Browsable(false)]
		public string oldCropBrushSerialize {
			get { return Serialize.BrushToString(oldCropBrush); }
			set { oldCropBrush = Serialize.StringToBrush(value); }
		}
		[Display(Name = "Berechnete Ernte", Order = 1, GroupName = "Farben")]
		public Brush estimatedCropBrush { get; set; }
		[Browsable(false)]
		public string estimatedCropBrushSerialize {
			get { return Serialize.BrushToString(estimatedCropBrush); }
			set { estimatedCropBrush = Serialize.StringToBrush(value); }
		}
		[Display(Name = "Zukünftige Ernte", Order = 2, GroupName = "Farben")]
		public Brush projectedCropBrush { get; set; }
		[Browsable(false)]
		public string projectedCropBrushSerialize {
			get { return Serialize.BrushToString(projectedCropBrush); }
			set { projectedCropBrush = Serialize.StringToBrush(value); }
		}
		[Display(Name = "Neues Erntejahr", Order = 3, GroupName = "Farben")]
		public Brush newCropYearBrush { get; set; }
		[Browsable(false)]
		public string newCropYearBrushSerialize {
			get { return Serialize.BrushToString(newCropYearBrush); }
			set { newCropYearBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		#endregion
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Wasde";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				
				field										= WasdeField.EndingStocks;
				showOldCrop									= true;
				showEstimatedCrop							= true;
				showProjectedCrop							= true;
				isAmerica									= true;
				oldCropBrush								= Brushes.DarkGreen;
				estimatedCropBrush							= Brushes.YellowGreen;
				projectedCropBrush							= Brushes.LightGreen;
				newCropYearBrush							= Brushes.CornflowerBlue.Clone();
				newCropYearBrush.Opacity = 0.7;
				lineWidth									= 2;
			} else if (State == State.Configure) {
				fieldText = Regex.Replace(field.ToString(), "([a-z])([A-Z])", "$1 $2");;
				AddPlot(new Stroke(showOldCrop       ? oldCropBrush       : Brushes.Transparent, lineWidth), PlotStyle.Line, "Alte Ernte");
				AddPlot(new Stroke(showEstimatedCrop ? estimatedCropBrush : Brushes.Transparent, lineWidth), PlotStyle.Line, "Berechnet");
				AddPlot(new Stroke(showProjectedCrop ? projectedCropBrush : Brushes.Transparent, lineWidth), PlotStyle.Line, "Vorhersage");
			} else if (State == State.DataLoaded) {
				int? id = SuriStrings.GetId(Instrument);
				if (id != null) {
					string oldDate = ChartBars.Bars.GetTime(0).AddMonths(-1).Date.ToString("yyyy-MM-dd");
					string newDate = ChartBars.Bars.LastBarTime    .AddMonths(+1).Date.ToString("yyyy-MM-dd");
					wasdeData = SuriServer.GetWasdeData(id.Value, isAmerica, oldDate, newDate);
				}
			}
		}
		
		protected override void OnBarUpdate() {
			if (wasdeData.IsNullOrEmpty()) return;
			
			for (int i = nextIndex; i < wasdeData.Count; i++) {
				if (wasdeData[i].Date.Date.Equals(Time[0].Date)) {
					hasStarted = true;
					double entry;
					if (wasdeData[i].Attributes.TryGetValue(fieldText, out entry)) Values[0][0] = entry; i++;
					if (wasdeData[i].Attributes.TryGetValue(fieldText, out entry)) Values[1][0] = entry; i++;
					if (wasdeData[i].Attributes.TryGetValue(fieldText, out entry)) Values[2][0] = entry;
					nextIndex = i;
					break;
				}
				if (hasStarted && wasdeData[i].Date.Date > Time[0].Date) {
					Values[0][0] = Values[0][1];
					Values[1][0] = Values[1][1];
					Values[2][0] = Values[2][1];
					break;
				}
			}
			if (CurrentBar > 0 && Time[0].Month == 5 && Math.Abs(Value[0] - Value[1]) > 0.0000001) {
				Draw.VerticalLine(this, Time[0].Year.ToString(), 0, newCropYearBrush, DashStyleHelper.Dot, 1, false);
			}
		}
	}
}

public enum WasdeField {
	AreaHarvested,
	AreaPlanted,
	FarmPrice,
	AvgFarmPriceHigh,
	AvgFarmPriceLow,
	AvgMillingYield,
	BeetSugar,
	BeginningCommercialStocks,
	BeginningStocks,
	BioDiesel,
	BioFuel,
	CaneSugar,
	CommercialExports,
	CommercialUse,
	Crushings,
	Deliveries,
	DisappearanceTo,
	DisappearanceTotal,
	Domestic,
	DomesticAndResidual,
	DomesticCommercialUse,
	DomesticCrush,
	DomesticDisappearance,
	DomesticFeed,
	DomesticUse,
	DomesticTotal,
	EndingCommercialStocks,
	EndingStocks,
	EndingStocksTotal,
	EthanolAndByProducts,
	EthanolForFuel,
	Exports,
	ExportsOther,
	ExportsTotal,
	FarmUse,
	FeedAndResidual,
	Food,
	FoodSeedAndIndustrial,
	FoodFeedAndOtherIndustrial,
	MethylEster,
	FreeStocks,
	Harvested,
	HarvestedAcres,
	HatchingUse,
	Imports,
	ImportsOther,
	Loss,
	Marketings,
	Milled,
	Miscellaneous,
	Other,
	OtherProgram,
	OtherStates,
	OtherStatesHigh,
	OtherStatesLow,
	Output,
	OutstandingLoans,
	PerCapita,
	Planted,
	Prices,
	PricesHigh,
	PricesLow,
	Production,
	Residual,
	Rough,
	Seed,
	StocksToUseRatio,
	SupplyBeginningStocks,
	SupplyTotal,
	TotalDomestic,
	TotalCmlSupply,
	TotalSupply,
	TotalUse,
	Trade,
	Trq,
	Unaccounted,
	UseExports,
	UseTotal,
	Yield,
	YieldPerHarvestedAcre
}



















































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.DevWasde[] cacheDevWasde;
		public Suri.dev.DevWasde DevWasde(WasdeField field, bool showOldCrop, bool showEstimatedCrop, bool showProjectedCrop, bool isAmerica)
		{
			return DevWasde(Input, field, showOldCrop, showEstimatedCrop, showProjectedCrop, isAmerica);
		}

		public Suri.dev.DevWasde DevWasde(ISeries<double> input, WasdeField field, bool showOldCrop, bool showEstimatedCrop, bool showProjectedCrop, bool isAmerica)
		{
			if (cacheDevWasde != null)
				for (int idx = 0; idx < cacheDevWasde.Length; idx++)
					if (cacheDevWasde[idx] != null && cacheDevWasde[idx].field == field && cacheDevWasde[idx].showOldCrop == showOldCrop && cacheDevWasde[idx].showEstimatedCrop == showEstimatedCrop && cacheDevWasde[idx].showProjectedCrop == showProjectedCrop && cacheDevWasde[idx].isAmerica == isAmerica && cacheDevWasde[idx].EqualsInput(input))
						return cacheDevWasde[idx];
			return CacheIndicator<Suri.dev.DevWasde>(new Suri.dev.DevWasde(){ field = field, showOldCrop = showOldCrop, showEstimatedCrop = showEstimatedCrop, showProjectedCrop = showProjectedCrop, isAmerica = isAmerica }, input, ref cacheDevWasde);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevWasde DevWasde(WasdeField field, bool showOldCrop, bool showEstimatedCrop, bool showProjectedCrop, bool isAmerica)
		{
			return indicator.DevWasde(Input, field, showOldCrop, showEstimatedCrop, showProjectedCrop, isAmerica);
		}

		public Indicators.Suri.dev.DevWasde DevWasde(ISeries<double> input , WasdeField field, bool showOldCrop, bool showEstimatedCrop, bool showProjectedCrop, bool isAmerica)
		{
			return indicator.DevWasde(input, field, showOldCrop, showEstimatedCrop, showProjectedCrop, isAmerica);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevWasde DevWasde(WasdeField field, bool showOldCrop, bool showEstimatedCrop, bool showProjectedCrop, bool isAmerica)
		{
			return indicator.DevWasde(Input, field, showOldCrop, showEstimatedCrop, showProjectedCrop, isAmerica);
		}

		public Indicators.Suri.dev.DevWasde DevWasde(ISeries<double> input , WasdeField field, bool showOldCrop, bool showEstimatedCrop, bool showProjectedCrop, bool isAmerica)
		{
			return indicator.DevWasde(input, field, showOldCrop, showEstimatedCrop, showProjectedCrop, isAmerica);
		}
	}
}

#endregion
