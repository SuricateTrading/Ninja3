#region Using declarations
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Cot1 : Indicator {
		
		private CotBase cotData;
		private bool hasGoneAbove;
		private bool hasGoneBelow;
		
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
				Days										= 125;
				cotData = CotBase(SuriCotReportField.CommercialNet);
				
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "COT1");
				AddLine(new Stroke(Brushes.Red, 3), 10.0, "10%");
				AddLine(Brushes.DimGray, 50.0, "50%");
				AddLine(new Stroke(Brushes.Green, 3), 90.0, "90%");
				
				Signals = new List<Signal>();
			}
		}
		
        public override string DisplayName {
			get {
				if (Instrument != null)
					return "COT 1 - " + SuriStrings.instrumentToName(Instrument.FullName);
				return "COT 1";
			}
        }
        
        public override void OnCalculateMinMax() {
	        MinValue = 0;
	        MaxValue = 100;
        }
		
		protected override void OnBarUpdate() {
			if (CurrentBar < Days) return;
			
			double? min = null, max = null;
			for (int barsAgo = 0; barsAgo < Days; barsAgo++) {
				double v = cotData.Value[barsAgo];
				if (min == null || min > v) min = v;
				if (max == null || max < v) max = v;
			}
			
			// min and max cannot be null at this point
			double osci = 100.0 * (cotData.Value[0] - min.Value) / (max.Value - min.Value);
			Value[0] = osci;
			
			if (CurrentBar == Days) return;
			
			// this section colors the starting point of cot 1 signals
			if (osci > 90.0 && Value[1] > 90.0 && hasGoneBelow) {
				SMA sma = SMA(125);
				if (sma[0] > sma[1]) {
					PlotBrushes[0][0] = Brushes.Green;
					if (Value[2] < 90.0)
						Signals.Add(new Signal(true, CurrentBar, true));
				} else {
					PlotBrushes[0][0] = Brushes.Yellow;
					if (Value[2] < 90.0)
						Signals.Add(new Signal(true, CurrentBar, false));
				}
			} else if (osci < 10.0 && Value[1] < 10.0 && hasGoneAbove) {
				SMA sma = SMA(125);
				if (sma[0] < sma[1]) {
					PlotBrushes[0][0] = Brushes.Red;
					if (Value[2] > 10.0)
						Signals.Add(new Signal(false, CurrentBar, true));
				} else {
					PlotBrushes[0][0] = Brushes.Yellow;
					if (Value[2] > 10.0)
						Signals.Add(new Signal(false, CurrentBar, false));
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
		}
		
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int Days
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
        public List<Signal> Signals {
			get;
			set;
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
		private Suri.Cot1[] cacheCot1;
		public Suri.Cot1 Cot1(int days)
		{
			return Cot1(Input, days);
		}

		public Suri.Cot1 Cot1(ISeries<double> input, int days)
		{
			if (cacheCot1 != null)
				for (int idx = 0; idx < cacheCot1.Length; idx++)
					if (cacheCot1[idx] != null && cacheCot1[idx].Days == days && cacheCot1[idx].EqualsInput(input))
						return cacheCot1[idx];
			return CacheIndicator<Suri.Cot1>(new Suri.Cot1(){ Days = days }, input, ref cacheCot1);
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
