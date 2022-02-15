#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class Cot22 : StrategyIndicator {
		private CotBase cotData;
		private double min = double.MaxValue;
		private double max = double.MinValue;
		private int minIndex;
		private int maxIndex;

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int days { get; set; }
		#endregion

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"CoT 2 Commercials Short";
				Name										= "CoT 2";
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
				days										= 1000;
				cotData = CotBase(SuriCotReportField.CommercialShort);
				
				AddPlot(new Stroke(Brushes.DarkGray, 3), PlotStyle.Line, "Com Short");
				AddPlot(new Stroke(Brushes.Red, 3), PlotStyle.Line, "75%");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "50%");
				AddPlot(new Stroke(Brushes.Green, 3), PlotStyle.Line, "25%");
			}
		}
		public override string DisplayName {
			get { return Instrument == null ? "COT 2" : "COT 2 - " + SuriStrings.instrumentToName(Instrument.FullName); }
		}
		private double ValueOf(double percent) { return min + percent * (max - min); }

		protected override void OnBarUpdate() {
			Values[0][0] = cotData.Value[0];
			SetMinMax();
			Values[1][0] = ValueOf(0.75);
			Values[2][0] = ValueOf(0.5);
			Values[3][0] = ValueOf(0.25);
			MoveLines();
			Analyze();
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			chartScale.Properties.AutoScaleMarginType = AutoScaleMarginType.Percent;
			chartScale.Properties.AutoScaleMarginUpper = 30;
			chartScale.Properties.AutoScaleMarginLower = 30;
		}

		private void SetMinMax() {
			if (min > cotData.Value[0]) { min = cotData.Value[0]; minIndex = CurrentBar; }
			if (max < cotData.Value[0]) { max = cotData.Value[0]; maxIndex = CurrentBar; }
			
			if (CurrentBar - maxIndex > days || CurrentBar - minIndex > days) {
				// the last max or min is too far away. Recalculate.
				min = double.MaxValue;
				max = double.MinValue;
				for (int i = 0; i < days; i++) {
					if (min > cotData.Value[i]) { min = cotData.Value[i]; minIndex = CurrentBar-i; }
					if (max < cotData.Value[i]) { max = cotData.Value[i]; maxIndex = CurrentBar-i; }
				}
			}
		}

		private void MoveLines() {
			if (CurrentBar <= days) return;
			
			double line25 = ValueOf(0.25);
			double line75 = ValueOf(0.75);
			double? localHigh = null;
			double lowestHigh = double.MaxValue;
			double? localLow = null;
			double highestLow = double.MinValue;
			int countHigh = 0;
			int countLow = 0;

			for (int i = days; i >= 0; i--) {
				if (Value[i] > line75 && (Value[i+1] < line75 || Value[i] > localHigh)) localHigh = Value[i];
				if (Value[i] < line25 && (Value[i+1] > line25 || Value[i] < localLow))  localLow  = Value[i];
				
				if (localHigh!=null && Value[i] < line75 && Value[i+1] > line75) {
					if (localHigh < lowestHigh) lowestHigh = localHigh.Value;
					countHigh++;
				}
				if (localLow!=null && Value[i] > line25 && Value[i+1] < line25) {
					if (highestLow < localLow) highestLow = localLow.Value;
					countLow++;
				}
			}
			
			Values[1][0] = countHigh > 1 ? lowestHigh : line75;
			Values[3][0] = countLow  > 1 ? highestLow : line25;
		}
		
		private void Analyze() {
			if (CurrentBar <= days) {
				PlotBrushes[1][0] = Brushes.DimGray;
				PlotBrushes[2][0] = Brushes.DimGray;
				PlotBrushes[3][0] = Brushes.DimGray;
				return;
			}
			
			if (Values[0][0] > Values[1][0]) {
				PlotBrushes[0][0] = Brushes.Red;
			}
			if (Values[0][0] < Values[3][0]) {
				PlotBrushes[0][0] = Brushes.Green;
			}
			
		}
		
		[XmlIgnore]
		[Browsable(false)]
		public override TradePosition tradePosition {
			get {
				if (CurrentBar<=days) return TradePosition.Middle;
				if (Values[0][0] > Values[2][0]) return TradePosition.Short;
				if (Values[0][0] < Values[2][0]) return TradePosition.Long;
				return TradePosition.Middle;
			}
		}

		[XmlIgnore]
		[Browsable(false)]
		public override double? stop {
			set { }
			get {

				return null;
			}
		}

		[XmlIgnore]
		[Browsable(false)]
		public override bool? isSignal {
			set { }
			get { return Values[0][0] > Values[1][0] || Values[0][0] < Values[3][0]; }
		}

		/// Used to be called iff a mega bar / mega volume occured.
		/// Calculations include gaps !
		public SignalVariant? GetSignalVariant(bool useGap = false) {
			TradePosition t = tradePosition;
			if (t == TradePosition.Middle) return null;

			double candleSize;
			double upperBody;
			double lowerBody;
			if (useGap) {
				candleSize = Math.Max(Close[1], High[0]) - Math.Min(Close[1], Low[0]);
				upperBody = Math.Max(Close[1], Math.Max(Open[0], Close[0]));
				lowerBody = Math.Min(Close[1], Math.Min(Open[0], Close[0]));
			} else {
				candleSize = High[0] - Low[0];
				upperBody = Math.Max(Open[0], Close[0]);
				lowerBody = Math.Min(Open[0], Close[0]);
			}
			double bodySizeWithGap = upperBody - lowerBody;
			
			if (bodySizeWithGap < candleSize * 0.10) {
				double upperCandleWickSize = candleSize - upperBody;
				double lowerCandleWickSize = candleSize - upperCandleWickSize - bodySizeWithGap;
				Print(CurrentBar + " Found a reversal bar @ " + Time[0] + " " + candleSize + " " + upperBody + " " + lowerBody + " " + bodySizeWithGap + " " + upperCandleWickSize + " " + lowerCandleWickSize);

				if (lowerCandleWickSize / candleSize > 0.6) return t == TradePosition.Long ? SignalVariant.V3 : SignalVariant.V4;
				if (upperCandleWickSize / candleSize > 0.6) return t == TradePosition.Long ? SignalVariant.V4 : SignalVariant.V3;
				Print(CurrentBar + " The reversal bar is positioned too much in the middle.");
			}
			Print(CurrentBar + " Found a mega bar @ " + Time[0] + " " + candleSize + " " + upperBody + " " + lowerBody + " " + bodySizeWithGap);
			if (Open[0] > Close[0]) return t == TradePosition.Long ? SignalVariant.V1 : SignalVariant.V2;
			if (Open[0] < Close[0]) return t == TradePosition.Long ? SignalVariant.V2 : SignalVariant.V1;
			
			Print(CurrentBar + " Did not found a variant! " + Time[0]);
			return null;
		}

		
        // German: "Markantes Tief / Hoch".
        public Tuple<Tuple<int, double?>, Tuple<int, double?>> StrikingSpot(bool searchForHigh, int barsAgo = 0) {
            int initialBarsAgo = barsAgo;
			
            // first we have to check if the bar was in an upward or downward trend.
            Tuple<int, double?> t1 = SearchForLowHigh(!searchForHigh, barsAgo);
            Tuple<int, double?> t2 = SearchForLowHigh(searchForHigh, barsAgo);
            // iff inDirection = true and searchForHigh = true, then there was a high before there was a low.
            bool inDirection = t1.Item1 > t2.Item1;
			
            while (barsAgo < CurrentBar && barsAgo - initialBarsAgo < 500) {
                if (inDirection) {
                    t1 = SearchForLowHigh(!searchForHigh, barsAgo);
                    t2 = SearchForLowHigh(searchForHigh, t1.Item1);
                } else {
                    t1 = SearchForLowHigh(searchForHigh, barsAgo);
                    t2 = SearchForLowHigh(!searchForHigh, t1.Item1);
                }
                if (t2.Item2 != null && (searchForHigh && t2.Item2 < High[barsAgo] || !searchForHigh && t2.Item2 > Low[barsAgo])) {
                    return new Tuple<Tuple<int, double?>, Tuple<int, double?>>(t1, t2);
                }
                barsAgo = t2.Item1;
            }
            return null;
        }
        
        /// Returns a Tuple of a high or low with index inclusive before the given barsAgo.
        private Tuple<int, double?> SearchForLowHigh(bool searchForHigh, int barsAgo, int adjacentsToCheck = 4) {
            double? value = null;
            int adjacentsChecked = 0;
            Tuple<int, double?> tuple = null;
            while (barsAgo < CurrentBar && adjacentsChecked < adjacentsToCheck) {
                if (value==null || searchForHigh && High[barsAgo] > value || !searchForHigh && Low[barsAgo] < value) {
                    value = searchForHigh ? High[barsAgo] : Low[barsAgo];
                    adjacentsChecked = 0;
                    tuple = new Tuple<int, double?>(barsAgo, value);
                } else {
                    adjacentsChecked++;
                }
                barsAgo++;
            }
            return tuple;
        }

        
        
	}
	public enum SignalVariant { V1, V2, V3, V4 }
}






























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.Cot22[] cacheCot22;
		public Suri.Cot22 Cot22(int days)
		{
			return Cot22(Input, days);
		}

		public Suri.Cot22 Cot22(ISeries<double> input, int days)
		{
			if (cacheCot22 != null)
				for (int idx = 0; idx < cacheCot22.Length; idx++)
					if (cacheCot22[idx] != null && cacheCot22[idx].days == days && cacheCot22[idx].EqualsInput(input))
						return cacheCot22[idx];
			return CacheIndicator<Suri.Cot22>(new Suri.Cot22(){ days = days }, input, ref cacheCot22);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.Cot22 Cot22(int days)
		{
			return indicator.Cot22(Input, days);
		}

		public Indicators.Suri.Cot22 Cot22(ISeries<double> input , int days)
		{
			return indicator.Cot22(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.Cot22 Cot22(int days)
		{
			return indicator.Cot22(Input, days);
		}

		public Indicators.Suri.Cot22 Cot22(ISeries<double> input , int days)
		{
			return indicator.Cot22(input, days);
		}
	}
}

#endregion
