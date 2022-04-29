#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Brush = System.Windows.Media.Brush;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class SuriProductionCost : Indicator {
		private ProductionCostData data;
		private Commodity? commodity;
		
		private static readonly Dictionary<Commodity, ProductionCostData> costs = new Dictionary<Commodity, ProductionCostData> {
			{Commodity.Corn,		new ProductionCostData(new Dictionary<int, ProductionSingleData> {
				{2020,new ProductionSingleData(644.72,328.01,355.32,178)},	{2019,new ProductionSingleData(664.94,337.38,352.97,173)},	{2018,new ProductionSingleData(631.54,330.27,346.75,184)},	{2017,new ProductionSingleData(603.28,329.29,339.68,185)},	{2016,new ProductionSingleData(603.92,340.55,337.93,183)},	{2015,new ProductionSingleData(612.6,333.8,341.53,167)},	{2014,new ProductionSingleData(603.18,356.92,332.88,170)},	{2013,new ProductionSingleData(720.51,355.6,320.85,156)},	{2012,new ProductionSingleData(802.55,349.59,303.98,118)},	{2011,new ProductionSingleData(837.77,332.33,281.13,146)},	{2010,new ProductionSingleData(689.39,286.41,263.79,159)},	{2009,new ProductionSingleData(561.22,295.01,255.69,156)},	{2008,new ProductionSingleData(629.36,295.69,233.69,144)},	{2007,new ProductionSingleData(468.94,228.99,214.98,143)},	{2006,new ProductionSingleData(351.87,205.98,203.76,138)},	{2005,new ProductionSingleData(260.43,186.37,200.51,149)},	{2004,new ProductionSingleData(362.35,175.94,201.56,169)},	{2003,new ProductionSingleData(319.62,161.16,193.25,149)},	{2002,new ProductionSingleData(312.82,145.48,188.83,134)},	{2001,new ProductionSingleData(266.92,162.3,186.23,144)},	{2000,new ProductionSingleData(246.67,164.99,213.33,138)},	{1999,new ProductionSingleData(230.7,156.92,207.81,135)},	{1998,new ProductionSingleData(262.88,157.71,205.15,136)},	{1997,new ProductionSingleData(331.37,162.25,201.48,130)},	{1996,new ProductionSingleData(369.93,160.95,192.99,130)},
			})},
			{Commodity.Cotton,		new ProductionCostData(new Dictionary<int, ProductionSingleData> {
				{2020,new ProductionSingleData(605.45,401.57,286.65,747)},	{2019,new ProductionSingleData(651.17,440.18,294.26,880)},	{2018,new ProductionSingleData(544.04,397.36,311.47,623)},	{2017,new ProductionSingleData(637.81,420.42,303,794)},	{2016,new ProductionSingleData(655.68,426.47,296.52,804)},	{2015,new ProductionSingleData(636.84,436.58,293.64,792)},	{2014,new ProductionSingleData(565.52,516.76,316.89,685)},	{2013,new ProductionSingleData(617.42,496.69,307.13,581)},	{2012,new ProductionSingleData(613.84,507,298.89,667)},	{2011,new ProductionSingleData(588.01,465.03,281.78,496)},	{2010,new ProductionSingleData(740.82,463.9,266.8,780)},	{2009,new ProductionSingleData(444.54,431.06,256.99,618)},	{2008,new ProductionSingleData(491.73,443.63,242.06,632)},	{2007,new ProductionSingleData(637.19,436.86,229.53,911)},	{2006,new ProductionSingleData(384.61,356.46,198.61,686)},	{2005,new ProductionSingleData(456.69,349.26,194.97,817)},	{2004,new ProductionSingleData(483.49,315.35,186.16,812)},	{2003,new ProductionSingleData(561.72,304.29,192.45,742)},	{2002,new ProductionSingleData(307.83,278,251.02,614)},	{2001,new ProductionSingleData(271.4,284.24,246.28,636)},	{2000,new ProductionSingleData(375.18,269.38,248.28,569)},	{1999,new ProductionSingleData(314.8,244.26,243.81,584)},	{1998,new ProductionSingleData(356.1,230.87,230.29,480)},	{1997,new ProductionSingleData(544.62,271.46,244.81,692)},	
			})},
			{Commodity.Soybeans,	new ProductionCostData(new Dictionary<int, ProductionSingleData> {
				{2020,new ProductionSingleData(515.4,182.25,314.72,53)},	{2019,new ProductionSingleData(429.34,188.95,314.78,50)},	{2018,new ProductionSingleData(458.91,187.16,308.7,53)},	{2017,new ProductionSingleData(454.72,158.08,285.42,49)},	{2016,new ProductionSingleData(491.92,160.31,283.15,52)},	{2015,new ProductionSingleData(430.56,166.97,296.33,48)},	{2014,new ProductionSingleData(522.24,176.79,289.93,48)},	{2013,new ProductionSingleData(571.04,176.63,281.68,43)},	{2012,new ProductionSingleData(596.82,172.29,265.3,42)},	{2011,new ProductionSingleData(525.36,136.87,246.17,44)},	{2010,new ProductionSingleData(449.32,131.89,232.19,47)},	{2009,new ProductionSingleData(437.1,130.49,227.78,47)},	{2008,new ProductionSingleData(450.64,127.79,206.9,43)},	{2007,new ProductionSingleData(357.75,106.63,190.42,45)},	{2006,new ProductionSingleData(254.84,93.41,184.68,46)},	{2005,new ProductionSingleData(264.57,90.21,174.18,47)},	{2004,new ProductionSingleData(253.46,81.77,167.24,45)},	{2003,new ProductionSingleData(233.47,77.66,160.83,36)},	{2002,new ProductionSingleData(208,73.5,158.5,40)},	{2001,new ProductionSingleData(178.62,81.83,182.25,43)},	{2000,new ProductionSingleData(182.45,77.28,176.82,41)},	{1999,new ProductionSingleData(178,76.33,172.69,40)},	{1998,new ProductionSingleData(223.17,79.32,168.24,43)},	{1997,new ProductionSingleData(281.22,79.47,166.36,43)},	
			})},
			{Commodity.WheatZw,		new ProductionCostData(new Dictionary<int, ProductionSingleData> {
				{2020,new ProductionSingleData(243.49,125.36,194.75,52)},	{2019,new ProductionSingleData(249.8,129.4,194.97,53)},	{2018,new ProductionSingleData(257.78,125.69,189.72,49)},	{2017,new ProductionSingleData(224.61,122.89,183.87,46)},	{2016,new ProductionSingleData(208.46,108.8,192.14,51)},	{2015,new ProductionSingleData(213.9,116.35,194.15,40)},	{2014,new ProductionSingleData(247.53,126.33,189.95,37)},	{2013,new ProductionSingleData(286.63,128.15,183.88,39)},	{2012,new ProductionSingleData(342.7,126.72,175.28,44)},	{2011,new ProductionSingleData(285.71,121.89,165.68,38)},	{2010,new ProductionSingleData(222.24,102.78,154.62,45)},	{2009,new ProductionSingleData(228.7,112.92,150.32,40)},	{2008,new ProductionSingleData(333.83,125.68,151.72,41)},	{2007,new ProductionSingleData(204.11,93.03,138.96,37)},	{2006,new ProductionSingleData(144.01,85.01,131.77,33)},	{2005,new ProductionSingleData(132.68,79.45,128,40)},	{2004,new ProductionSingleData(142.48,70.82,119.78,40)},	{2003,new ProductionSingleData(130.02,67.68,123.44,41)},	{2002,new ProductionSingleData(95.17,57.07,118.56,28)},	{2001,new ProductionSingleData(98.4,64.94,118.4,35)},	{2000,new ProductionSingleData(95.7,58.38,115.48,38)},	{1999,new ProductionSingleData(98.78,54.82,111.33,39)},	{1998,new ProductionSingleData(114.27,57.41,107.78,41)},		
			})},
			{Commodity.Oats,		new ProductionCostData(new Dictionary<int, ProductionSingleData> {
				{2020,new ProductionSingleData(269.94,122.22,292.67,70)},	{2019,new ProductionSingleData(281.13,127.83,292.35,68)},	{2018,new ProductionSingleData(260.84,122.78,280.02,69)},	{2017,new ProductionSingleData(229.61,121.89,272.13,65)},	{2016,new ProductionSingleData(207.61,123.3,268.12,70)},	{2015,new ProductionSingleData(249.76,135.37,279.39,75)},	{2014,new ProductionSingleData(370.27,117.3,249.03,66)},	{2013,new ProductionSingleData(378.23,114.09,235.51,63)},	{2012,new ProductionSingleData(339.94,120.75,227.24,59)},	{2011,new ProductionSingleData(283.28,112.81,210.72,53)},	{2010,new ProductionSingleData(193.02,95.53,202.48,61)},	{2009,new ProductionSingleData(235.75,101.78,197.06,65)},	{2008,new ProductionSingleData(325.87,113.14,181.09,61)},	{2007,new ProductionSingleData(213.27,81.61,165.82,59)},	{2006,new ProductionSingleData(166.11,95.68,184.82,56)},	{2005,new ProductionSingleData(148.56,71.89,158.46,63)},									
			})},
			{Commodity.Rice,		new ProductionCostData(new Dictionary<int, ProductionSingleData> {
				{2020,new ProductionSingleData(1233.76,506.87,456.39,88)},	{2019,new ProductionSingleData(1019.2,543.14,459.76,80)},	{2018,new ProductionSingleData(1106.28,533.7,441.98,84)},	{2017,new ProductionSingleData(972,523.97,430.91,80)},	{2016,new ProductionSingleData(863.46,516.4,424.05,78)},	{2015,new ProductionSingleData(1060.29,543.79,427.81,81)},	{2014,new ProductionSingleData(1152.1,601.3,412.3,82)},	{2013,new ProductionSingleData(1395.23,602.52,413.86,83)},	{2012,new ProductionSingleData(1166.1,534.32,448.06,78)},	{2011,new ProductionSingleData(1069.94,525.77,419.5,72)},	{2010,new ProductionSingleData(795.75,446.11,379.13,70)},	{2009,new ProductionSingleData(1072.26,447.35,374.75,74)},	{2008,new ProductionSingleData(1287.36,480.43,345.65,72)},	{2007,new ProductionSingleData(779.76,395.96,323.78,76)},	{2006,new ProductionSingleData(620.64,366.48,314.49,72)},	{2005,new ProductionSingleData(469.14,375.12,328.52,71)},	{2004,new ProductionSingleData(602.33,328.95,322.86,74)},	{2003,new ProductionSingleData(449.69,308.55,305.82,71)},	{2002,new ProductionSingleData(280.56,280.67,305.65,71)},	{2001,new ProductionSingleData(328.67,299.11,295.01,69)},	{2000,new ProductionSingleData(368.77,283.8,295.09,68)},				
			})},
			/*{Commodity.Milk,		new ProductionCostData(new Dictionary<int, ProductionSingleData> {
			})},
			{Commodity.FeederCattle,new ProductionCostData(new Dictionary<int, ProductionSingleData> {
			})},
			{Commodity.LiveHogs,	new ProductionCostData(new Dictionary<int, ProductionSingleData> {
			})},*/
		};
		
		
		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der Linie", Order=2, GroupName="Parameter")]
		public int lineWidth
		{ get; set; }
		
		#region Colors
		[XmlIgnore]
		[Display(Name = "Genauer Wert", Order = 0, GroupName = "Farben")]
		public Brush exactLineBrush { get; set; }
		[Browsable(false)]
		public string exactBrushSerialize {
			get { return Serialize.BrushToString(exactLineBrush); }
			set { exactLineBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Geschätzter Wert", Order = 1, GroupName = "Farben")]
		public Brush estimatedLineBrush { get; set; }
		[Browsable(false)]
		public string estimatedLineBrushSerialize {
			get { return Serialize.BrushToString(estimatedLineBrush); }
			set { estimatedLineBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Produktionskosten";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				lineWidth									= 2;
				exactLineBrush								= Brushes.Red;
				estimatedLineBrush							= Brushes.Gray;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(exactLineBrush, lineWidth), PlotStyle.Line, "Kosten");
			} else if (State == State.DataLoaded) {
				commodity = SuriStrings.GetComm(Instrument.MasterInstrument.Name);
				if (commodity == Commodity.WheatKe) commodity = Commodity.WheatZw;
				if (commodity == Commodity.LiveCattle) commodity = Commodity.FeederCattle;
				if (commodity != null && costs.ContainsKey(commodity.Value)) {
					data = costs[commodity.Value];
				}
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}
		
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None) return;
			try {
				switch (commodity) {
					case Commodity.Sugar: Values[0][0] = 16; break;
					case Commodity.LiveHogs: Values[0][0] = 50; break;
					case Commodity.Coffee: Values[0][0] = 150; break;
					case Commodity.OrangeJuice: Values[0][0] = 80; break;
					case Commodity.SoybeanOil: Values[0][0] = 30; break;
					case Commodity.Gold: Values[0][0] = 870; break;
					case Commodity.Silver: Values[0][0] = 15; break;
					case Commodity.Platinum: Values[0][0] = 950; break;
					case Commodity.Copper: Values[0][0] = 2; break;
					case Commodity.CrudeOil: Values[0][0] = 25; break;
					case Commodity.Palladium: Values[0][0] = 650; break;
					case Commodity.Cacao: Values[0][0] = 1900; break;
					case Commodity.SoybeanMeal: Values[0][0] = 344; break;
					case null: break;
					
					case Commodity.Rice:
					case Commodity.Corn:
					case Commodity.Cotton:
					case Commodity.Soybeans:
					case Commodity.WheatZw:
					case Commodity.Oats:
						if (Time[0].Year < data.grossValues.Last().Key) {
							Values[0][0] = data.grossValues.Last().Value.costs;
							PlotBrushes[0][0] = estimatedLineBrush;
						} else if (Time[0].Year <= data.grossValues.First().Key) {
							Values[0][0] = data.grossValues[Time[0].Year].costs;
						} else {
							Values[0][0] = data.grossValues.First().Value.costs;
							PlotBrushes[0][0] = estimatedLineBrush;
						}
						if (commodity != Commodity.Rice) Values[0][0] *= 100;
						break;
				}
			} catch (Exception) {/**/}
		}
		
	}

	public class ProductionCostData {
		public Dictionary<int, ProductionSingleData> grossValues;
		public ProductionCostData(Dictionary<int, ProductionSingleData> grossValues) {
			this.grossValues = grossValues;
		}
	}
	public class ProductionSingleData {
		public readonly double? grossValue; // Bruttowert
		public readonly double costsPerAcre; // Laufende Kosten
		public readonly double allocatedOverhead; // Potentielle Ersparnisse
		public readonly double yield;
		public readonly double costs;
		//public double totalCosts { get { return costs + allocatedOverhead; } }
		//public double netValueTotalCosts { get { return grossValue - costs - allocatedOverhead; } }
		//public double netValueCosts { get { return grossValue - costs; } }

		public ProductionSingleData(double? grossValue, double costsPerAcre, double allocatedOverhead, double yield) {
			this.grossValue = grossValue;
			this.costsPerAcre = costsPerAcre;
			this.allocatedOverhead = allocatedOverhead;
			this.yield = yield;
			costs = costsPerAcre / yield;
		}
	}
	
}





















































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriProductionCost[] cacheSuriProductionCost;
		public Suri.SuriProductionCost SuriProductionCost()
		{
			return SuriProductionCost(Input);
		}

		public Suri.SuriProductionCost SuriProductionCost(ISeries<double> input)
		{
			if (cacheSuriProductionCost != null)
				for (int idx = 0; idx < cacheSuriProductionCost.Length; idx++)
					if (cacheSuriProductionCost[idx] != null &&  cacheSuriProductionCost[idx].EqualsInput(input))
						return cacheSuriProductionCost[idx];
			return CacheIndicator<Suri.SuriProductionCost>(new Suri.SuriProductionCost(), input, ref cacheSuriProductionCost);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriProductionCost SuriProductionCost()
		{
			return indicator.SuriProductionCost(Input);
		}

		public Indicators.Suri.SuriProductionCost SuriProductionCost(ISeries<double> input )
		{
			return indicator.SuriProductionCost(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriProductionCost SuriProductionCost()
		{
			return indicator.SuriProductionCost(Input);
		}

		public Indicators.Suri.SuriProductionCost SuriProductionCost(ISeries<double> input )
		{
			return indicator.SuriProductionCost(input);
		}
	}
}

#endregion
