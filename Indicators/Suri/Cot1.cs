#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Cot1 : StrategyIndicator {
		private CotBase cotData;
		private bool hasGoneAbove;
		private bool hasGoneBelow;
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int days
		{ get; set; }
		#endregion

		public Cot1() {
			//VendorLicense("SuricateTradingGmbH", "Basis", "https://www.suricate-trading.de/", "info@suricate-trading.de",null);
		}
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Cot 1";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				days										= 125;
				cotData = CotBase(SuriCotReportField.CommercialNet);
				
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "COT1");
				AddLine(new Stroke(Brushes.Red, 3), 10.0, "10%");
				AddLine(Brushes.DimGray, 50.0, "50%");
				AddLine(new Stroke(Brushes.Green, 3), 90.0, "90%");
			}
		}
		
        public override string DisplayName {
			get {
				if (Instrument == null) return "COT 1";
				return "COT 1 - " + SuriStrings.instrumentToName(Instrument.FullName);
			}
        }
        
        public override void OnCalculateMinMax() {
	        MinValue = 0;
	        MaxValue = 100;
        }
		
		protected override void OnBarUpdate() {
			Reset();
			if (CurrentBar < days) return;
			
			double? min = null, max = null;
			for (int barsAgo = 0; barsAgo < days; barsAgo++) {
				double v = cotData.Value[barsAgo];
				if (min == null || min > v) min = v;
				if (max == null || max < v) max = v;
			}
			
			// min and max cannot be null at this point
			double osci = 100.0 * (cotData.Value[0] - min.Value) / (max.Value - min.Value);
			Value[0] = osci;
			
			if (CurrentBar == days) return;
			
			// this section colors the starting point of cot 1 signals
			if (osci > 90.0 && Value[1] > 90.0 && hasGoneBelow) {
				isSignal = true;
				SMA sma = SMA(125);
				if (sma[0] > sma[1]) {
					PlotBrushes[0][0] = Brushes.Green;
					if (Value[2] < 90) firstSignalDate = Time[0];
					if (Value[4] < 90 && Time[0].DayOfWeek==DayOfWeek.Friday) {
						isEntry = true;
						entry = GetEntry();
						Draw.Line(this, "Entry " + SuriTest.random, false, 1, entry.Value, -1, entry.Value, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
						Draw.Text(this, "Entry " + SuriTest.random, "COT 1 Entry\n@" + entry.Value, 0, entry.Value, ChartControl.Properties.ChartText);
					}
				} else {
					PlotBrushes[0][0] = Brushes.Yellow;
				}
			} else if (osci < 10.0 && Value[1] < 10.0 && hasGoneAbove) {
				isSignal = true;
				SMA sma = SMA(125);
				if (sma[0] < sma[1]) {
					PlotBrushes[0][0] = Brushes.Red;
					if (Value[2] > 10) firstSignalDate = Time[0];
					if (Value[4] > 10 && Time[0].DayOfWeek==DayOfWeek.Friday) {
						isEntry = true;
						entry = GetEntry();
						Draw.Line(this, "Entry " + SuriTest.random, false, 1, entry.Value, -1, entry.Value, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
						Draw.Text(this, "Entry " + SuriTest.random, "COT 1 Entry\n@" + entry.Value, 0, entry.Value, ChartControl.Properties.ChartText);
					}
				} else {
					PlotBrushes[0][0] = Brushes.Yellow;
				}
			} else {
				if (Value[1] < 10.0 && Value[0] > 10.0) {
					hasGoneBelow = true;
					hasGoneAbove = false;
				}
				if (Value[1] > 90.0 && Value[0] < 90.0) {
					hasGoneAbove = true;
					hasGoneBelow = false;
				}
			}

			if (Value[1]>50 && Value[0]<50 || Value[1]<50 && Value[0]>50) {
				exitBar = CurrentBar+3;
			}
		}

		[XmlIgnore]
		[Browsable(false)]
		public override TradePosition tradePosition {
			get {
				if (Value[0] > 50) return TradePosition.Long;
				if (Value[0] < 50) return TradePosition.Short;
				return TradePosition.Middle;
			}
		}
		
		[Browsable(false)]
		private double? GetEntry() {
			if (CurrentBar < 5) return null;
			double max = double.MinValue, min = double.MaxValue;
			for (int i = 0; i < 5; i++) {
				if (High[i] > max) max = High[i];
				if (Low[i]  < min) min =  Low[i];
				if (Time[i].DayOfWeek == DayOfWeek.Monday) break;
			}
			SDraw.RectangleS(this, "Entrybox " + SuriTest.random, 4, min - TickSize, 0, max + TickSize, Brushes.Blue, false);
			
			switch (tradePosition) {
				case TradePosition.Long:
					//Draw.Line(this, "Entry " + SuriTest.random, false, 4, max + TickSize, 0, max + TickSize, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
					return max + TickSize;
				case TradePosition.Short:
					//Draw.Line(this, "Entry " + SuriTest.random, false, 4, min - TickSize, 0, min - TickSize, Brushes.LimeGreen, DashStyleHelper.Solid, 1);
					return min - TickSize;
			}
			return null;
		}



	
		[XmlIgnore]
		[Browsable(false)]
		public override double? stop {
			set {}
			get {
				if (CurrentBar < 10) return null;
				double max = double.MinValue, min = double.MaxValue;
				for (int i = 0; i < 10; i++) {
					if (High[i] > max) max = High[i];
					if (Low[i]  < min) min =  Low[i];
				}
				switch (tradePosition) {
					case TradePosition.Long:  return min - TickSize;
					case TradePosition.Short: return max + TickSize;
				}
				return null;
			}
		}

    }
}
























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.Cot1[] cacheCot1;
		public Suri.Cot1 Cot1(int days)
		{
			return Cot1(Input, days);
		}

		public Suri.Cot1 Cot1(ISeries<double> input, int days)
		{
			if (cacheCot1 != null)
				for (int idx = 0; idx < cacheCot1.Length; idx++)
					if (cacheCot1[idx] != null && cacheCot1[idx].days == days && cacheCot1[idx].EqualsInput(input))
						return cacheCot1[idx];
			return CacheIndicator<Suri.Cot1>(new Suri.Cot1(){ days = days }, input, ref cacheCot1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Cot1 Cot1(int days)
		{
			return indicator.Cot1(Input, days);
		}

		public Indicators.Suri.Cot1 Cot1(ISeries<double> input , int days)
		{
			return indicator.Cot1(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Cot1 Cot1(int days)
		{
			return indicator.Cot1(Input, days);
		}

		public Indicators.Suri.Cot1 Cot1(ISeries<double> input , int days)
		{
			return indicator.Cot1(input, days);
		}
	}
}

#endregion
