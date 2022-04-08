#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriCot2 : StrategyIndicator {
		private SuriCot suriCotData;
		private double min = double.MaxValue;
		private double max = double.MinValue;
		private int minIndex;
		private int maxIndex;

		#region Properties
		[NinjaScriptProperty]
		[Browsable(false)]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int days { get; set; }
		
		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der Hauptlinie", Order=2, GroupName="Parameter")]
		public int lineWidth
		{ get; set; }
		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Display(Name="Breite der sekundären Linien", Order=3, GroupName="Parameter")]
		public int lineWidthSecondary
		{ get; set; }
		
		#region Colors
		[XmlIgnore]
		[Display(Name = "Long", Order = 0, GroupName = "Farben")]
		public Brush longBrush { get; set; }
		[Browsable(false)]
		public string longBrushSerialize {
			get { return Serialize.BrushToString(longBrush); }
			set { longBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Short", Order = 1, GroupName = "Farben")]
		public Brush shortBrush { get; set; }
		[Browsable(false)]
		public string shortBrushSerialize {
			get { return Serialize.BrushToString(shortBrush); }
			set { shortBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Normale Linie", Order = 2, GroupName = "Farben")]
		public Brush regularLineBrush { get; set; }
		[Browsable(false)]
		public string regularLineBrushSerialize {
			get { return Serialize.BrushToString(regularLineBrush); }
			set { regularLineBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "50% Linie", Order = 3, GroupName = "Farben")]
		public Brush brush50Percent { get; set; }
		[Browsable(false)]
		public string brush50PercentSerialize {
			get { return Serialize.BrushToString(brush50Percent); }
			set { brush50Percent = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Farbe wenn noch nicht genügend Daten", Order = 4, GroupName = "Farben", Description = "CoT 2 braucht normalerweise ungefähr 4 Jahre, bis es korrekt angezeigt werden kann. Wenn noch nicht 4 Jahre geladen sind, wird diese Farbe benutzt.")]
		public Brush notReadyBrush { get; set; }
		[Browsable(false)]
		public string notReadyBrushSerialize {
			get { return Serialize.BrushToString(notReadyBrush); }
			set { notReadyBrush = Serialize.StringToBrush(value); }
		}
		#endregion
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
				longBrush									= Brushes.Green;
				shortBrush									= Brushes.Red;
				brush50Percent								= Brushes.DimGray;
				regularLineBrush							= Brushes.DarkGray;
				notReadyBrush								= Brushes.Transparent;
				lineWidth									= 4;
				lineWidthSecondary							= 2;
				days										= 1000;
				suriCotData = SuriCot(SuriCotReportField.CommercialShort);
			} else if (State == State.Configure) {
				AddPlot(new Stroke(shortBrush, lineWidthSecondary), PlotStyle.Line, "75%");
				AddPlot(new Stroke(brush50Percent, lineWidthSecondary), PlotStyle.Line, "50%");
				AddPlot(new Stroke(longBrush, lineWidthSecondary), PlotStyle.Line, "25%");
				AddPlot(new Stroke(regularLineBrush, lineWidth), PlotStyle.Line, "Com Short");
			}
		}
		public override string DisplayName { get { return SuriStrings.DisplayName(Name, Instrument); } }
		private double ValueOf(double percent) { return min + percent * (max - min); }
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			chartScale.Properties.AutoScaleMarginType = AutoScaleMarginType.Percent;
			chartScale.Properties.AutoScaleMarginUpper = 30;
			chartScale.Properties.AutoScaleMarginLower = 30;
			
			base.OnRender(chartControl, chartScale);
			if (SuriAddOn.license == License.None) SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
		}
		
		protected override void OnBarUpdate() {
			if (SuriAddOn.license == License.None) return;
			Values[3][0] = suriCotData.Value[0];
			SetMinMax();
			Values[0][0] = ValueOf(0.75);
			Values[1][0] = ValueOf(0.5);
			Values[2][0] = ValueOf(0.25);
			MoveLines();
			Analyze();
		}

		private void SetMinMax() {
			if (min > suriCotData.Value[0]) { min = suriCotData.Value[0]; minIndex = CurrentBar; }
			if (max < suriCotData.Value[0]) { max = suriCotData.Value[0]; maxIndex = CurrentBar; }
			
			if (CurrentBar - maxIndex > days || CurrentBar - minIndex > days) {
				// the last max or min is too far away. Recalculate.
				min = double.MaxValue;
				max = double.MinValue;
				for (int i = 0; i < days; i++) {
					if (min > suriCotData.Value[i]) { min = suriCotData.Value[i]; minIndex = CurrentBar-i; }
					if (max < suriCotData.Value[i]) { max = suriCotData.Value[i]; maxIndex = CurrentBar-i; }
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
				if (Values[3][i] > line75 && (Values[3][i+1] < line75 || Values[3][i] > localHigh)) localHigh = Values[3][i];
				if (Values[3][i] < line25 && (Values[3][i+1] > line25 || Values[3][i] < localLow))  localLow  = Values[3][i];
				
				if (localHigh!=null && Values[3][i] < line75 && Values[3][i+1] > line75) {
					if (localHigh < lowestHigh) lowestHigh = localHigh.Value;
					countHigh++;
				}
				if (localLow!=null && Values[3][i] > line25 && Values[3][i+1] < line25) {
					if (highestLow < localLow) highestLow = localLow.Value;
					countLow++;
				}
			}
			
			Values[0][0] = countHigh > 1 ? lowestHigh : line75;
			Values[2][0] = countLow  > 1 ? highestLow : line25;
		}
		
		private void Analyze() {
			if (SuriAddOn.license == License.Basic) return;
			
			if (CurrentBar <= days) {
				PlotBrushes[0][0] = notReadyBrush;
				PlotBrushes[1][0] = notReadyBrush;
				PlotBrushes[2][0] = notReadyBrush;
				return;
			}
			if (Values[3][0] > Values[0][0]) {
				PlotBrushes[3][0] = shortBrush;
			}
			if (Values[3][0] < Values[2][0]) {
				PlotBrushes[3][0] = longBrush;
			}
		}
		
		
		
		
		#region Strategy
		[XmlIgnore]
		[Browsable(false)]
		public override TradePosition tradePosition {
			get {
				if (CurrentBar<=days) return TradePosition.Middle;
				if (Values[3][0] > Values[1][0]) return TradePosition.Short;
				if (Values[3][0] < Values[1][0]) return TradePosition.Long;
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
			get { return Values[3][0] > Values[0][0] || Values[3][0] < Values[2][0]; }
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
        #endregion
	}
	public enum SignalVariant { V1, V2, V3, V4 }
}






























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriCot2[] cacheSuriCot2;
		public Suri.SuriCot2 SuriCot2(int days)
		{
			return SuriCot2(Input, days);
		}

		public Suri.SuriCot2 SuriCot2(ISeries<double> input, int days)
		{
			if (cacheSuriCot2 != null)
				for (int idx = 0; idx < cacheSuriCot2.Length; idx++)
					if (cacheSuriCot2[idx] != null && cacheSuriCot2[idx].days == days && cacheSuriCot2[idx].EqualsInput(input))
						return cacheSuriCot2[idx];
			return CacheIndicator<Suri.SuriCot2>(new Suri.SuriCot2(){ days = days }, input, ref cacheSuriCot2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCot2 SuriCot2(int days)
		{
			return indicator.SuriCot2(Input, days);
		}

		public Indicators.Suri.SuriCot2 SuriCot2(ISeries<double> input , int days)
		{
			return indicator.SuriCot2(input, days);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCot2 SuriCot2(int days)
		{
			return indicator.SuriCot2(Input, days);
		}

		public Indicators.Suri.SuriCot2 SuriCot2(ISeries<double> input , int days)
		{
			return indicator.SuriCot2(input, days);
		}
	}
}

#endregion
