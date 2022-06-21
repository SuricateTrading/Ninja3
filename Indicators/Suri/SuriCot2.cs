#region Using declarations
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.NinjaScript.Strategies;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriCot2 : Suri2080Indicator {
		#region Properties
		[XmlIgnore]
		[Display(Name = "Long", Order = 1, GroupName = "Farben")]
		public override Brush bottomBrush { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Short", Order = 3, GroupName = "Farben")]
		public override Brush topBrush { get; set; }
		
		[Browsable(false)] public override int topLinePercent { get; set; }
		[Browsable(false)] public override int bottomLinePercent { get; set; }
		#endregion

		protected override void OnStateChange() {
			base.OnStateChange();
			if (State == State.SetDefaults) {
				Description									= @"CoT 2 Commercials Short";
				Name										= "CoT 2";
				bottomBrush									= Brushes.Green;
				topBrush									= Brushes.Red;
				lineWidth									= 4;
				lineWidthSecondary							= 2;
				moveLines									= true;
				topLinePercent								= 75;
				bottomLinePercent							= 25;
				reportField									= SuriCotReportField.CommercialShort;
			}
		}
		protected override string plotName { get { return "Com Short"; } }
		protected override double GetMainValue(DbCotData cotData) { return cotData.commercialsShort; }

		[Browsable(false)] public Series<double> seriesMain { get { return Values[0]; } }
		[Browsable(false)] public Series<double> series25   { get { return Values[1]; } }
		[Browsable(false)] public Series<double> series50   { get { return Values[2]; } }
		[Browsable(false)] public Series<double> series75   { get { return Values[3]; } }
		public bool IsInLongHalf(int barIndex) { return seriesMain.GetValueAt(barIndex) < series50.GetValueAt(barIndex); }
		public bool IsInShortHalf(int barIndex) { return !IsInLongHalf(barIndex); }

		/** Returns a position iff over 75% or under 25%. */
		public SuriPosition GetSuriPosition(int i) {
			if (seriesMain.GetValueAt(i) <= series25.GetValueAt(i)) return SuriPosition.Long;
			if (seriesMain.GetValueAt(i) >= series75.GetValueAt(i)) return SuriPosition.Short;
			return SuriPosition.None;
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriCot2[] cacheSuriCot2;
		public Suri.SuriCot2 SuriCot2()
		{
			return SuriCot2(Input);
		}

		public Suri.SuriCot2 SuriCot2(ISeries<double> input)
		{
			if (cacheSuriCot2 != null)
				for (int idx = 0; idx < cacheSuriCot2.Length; idx++)
					if (cacheSuriCot2[idx] != null &&  cacheSuriCot2[idx].EqualsInput(input))
						return cacheSuriCot2[idx];
			return CacheIndicator<Suri.SuriCot2>(new Suri.SuriCot2(), input, ref cacheSuriCot2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriCot2 SuriCot2()
		{
			return indicator.SuriCot2(Input);
		}

		public Indicators.Suri.SuriCot2 SuriCot2(ISeries<double> input )
		{
			return indicator.SuriCot2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriCot2 SuriCot2()
		{
			return indicator.SuriCot2(Input);
		}

		public Indicators.Suri.SuriCot2 SuriCot2(ISeries<double> input )
		{
			return indicator.SuriCot2(input);
		}
	}
}

#endregion
