#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class ProductionCost : Indicator {
		private ProductionCostData data;
		
		public static readonly Dictionary<Commodity, ProductionCostData> costs = new Dictionary<Commodity, ProductionCostData> {
			{Commodity.Corn,		new ProductionCostData(new Dictionary<int, ProductionSingleData> {
				{2020,new ProductionSingleData(644.72,328.01,355.32)},
				{2019,new ProductionSingleData(664.94,337.38,352.97)},
				{2018,new ProductionSingleData(631.54,330.27,346.75)},
				{2017,new ProductionSingleData(603.28,329.29,339.68)},
				{2016,new ProductionSingleData(603.92,340.55,337.93)},
				{2015,new ProductionSingleData(612.6,333.8,341.53)},
				{2014,new ProductionSingleData(603.18,356.92,332.88)},
				{2013,new ProductionSingleData(720.51,355.6,320.85)},
				{2012,new ProductionSingleData(802.55,349.59,303.98)},
				{2011,new ProductionSingleData(837.77,332.33,281.13)},
				{2010,new ProductionSingleData(689.39,286.41,263.79)},
				{2009,new ProductionSingleData(561.22,295.01,255.69)},
				{2008,new ProductionSingleData(629.36,295.69,233.69)},
				{2007,new ProductionSingleData(468.94,228.99,214.98)},
				{2006,new ProductionSingleData(351.87,205.98,203.76)},
				{2005,new ProductionSingleData(260.43,186.37,200.51)},
				{2004,new ProductionSingleData(362.35,175.94,201.56)},
				{2003,new ProductionSingleData(319.62,161.16,193.25)},
				{2002,new ProductionSingleData(312.82,145.48,188.83)},
				{2001,new ProductionSingleData(266.92,162.3,186.23)},
				{2000,new ProductionSingleData(246.67,164.99,213.33)},
				{1999,new ProductionSingleData(230.7,156.92,207.81)},
				{1998,new ProductionSingleData(262.88,157.71,205.15)},
				{1997,new ProductionSingleData(331.37,162.25,201.48)},
				{1996,new ProductionSingleData(369.93,160.95,192.99)},
			})},
		};
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "ProductionCost";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "Kosten");
				//AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Line, "Bruttowert");
			} else if (State == State.DataLoaded) {
				Commodity? commodity = SuriStrings.GetComm(Instrument.MasterInstrument.Name);
				if (commodity == null) return;
				data = costs[commodity.Value];
			}
		}
		
		protected override void OnBarUpdate() {
			if (data == null) return;
			try {
				if (Time[0].Year <= data.grossValues.First().Key) {
					Values[0][0] = data.grossValues[Time[0].Year].costs;
				} else {
					Values[0][0] = data.grossValues.First().Value.costs;
					PlotBrushes[0][0] = Brushes.Gray;
				}
				//Values[1][0] = data.grossValues[Time[0].Year].grossValue;
			}
			catch (Exception) {/**/}
		}
		
	}

	public class ProductionCostData {
		public Dictionary<int, ProductionSingleData> grossValues;
		public ProductionCostData(Dictionary<int, ProductionSingleData> grossValues) {
			this.grossValues = grossValues;
		}
	}
	public class ProductionSingleData {
		public readonly double grossValue; // Bruttowert
		public readonly double costs; // Laufende Kosten
		public readonly double allocatedOverhead; // Potentielle Ersparnisse
		public double totalCosts { get { return costs + allocatedOverhead; } }
		public double netValueTotalCosts { get { return grossValue - costs - allocatedOverhead; } }
		public double netValueCosts { get { return grossValue - costs; } }

		public ProductionSingleData(double grossValue, double costs, double allocatedOverhead) {
			this.grossValue = grossValue;
			this.costs = costs;
			this.allocatedOverhead = allocatedOverhead;
		}
	}
	
}





















































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.ProductionCost[] cacheProductionCost;
		public Suri.dev.ProductionCost ProductionCost()
		{
			return ProductionCost(Input);
		}

		public Suri.dev.ProductionCost ProductionCost(ISeries<double> input)
		{
			if (cacheProductionCost != null)
				for (int idx = 0; idx < cacheProductionCost.Length; idx++)
					if (cacheProductionCost[idx] != null &&  cacheProductionCost[idx].EqualsInput(input))
						return cacheProductionCost[idx];
			return CacheIndicator<Suri.dev.ProductionCost>(new Suri.dev.ProductionCost(), input, ref cacheProductionCost);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.ProductionCost ProductionCost()
		{
			return indicator.ProductionCost(Input);
		}

		public Indicators.Suri.dev.ProductionCost ProductionCost(ISeries<double> input )
		{
			return indicator.ProductionCost(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.ProductionCost ProductionCost()
		{
			return indicator.ProductionCost(Input);
		}

		public Indicators.Suri.dev.ProductionCost ProductionCost(ISeries<double> input )
		{
			return indicator.ProductionCost(input);
		}
	}
}

#endregion
