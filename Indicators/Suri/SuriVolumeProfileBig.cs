#region Using declarations
using System;
using NinjaTrader.Gui.Chart;
using System.Windows.Media;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using SharpDX;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriVolumeProfileBig : Indicator {
		private SuriVpBigData suriVpBigData;
		private bool dataLoaded;
		
		#region Properties
		private SharpDX.Direct2D1.Brush normalAreaFill;
		private SharpDX.Direct2D1.Brush pocFill;
		private SharpDX.Direct2D1.Brush vaFill;
		private SharpDX.Direct2D1.Brush textFill;
		private SharpDX.DirectWrite.TextFormat textFormat;
		
		/*[Browsable(false)]
		[Display(Name = "Jahre zu laden", Order = 1, GroupName = "Parameter", Description = "Wie viele Jahre an Daten geladen werden sollen.")]
		public int years { get; set; }*/
		
		[NinjaScriptProperty]
		[Display(Name = "Aktuelles VP", Order = 0, GroupName = "Parameter", Description = "Wenn aktiv, wird das aktuelle Volumen Profil geladen und die beiden Parameter 'Jahre zu laden' und 'End Datum' haben dann keine Auswirkung mehr! Hierfür ist die Premium Version nötig.")]
		public bool loadRecent { get; set; }
		
		[Range(1, 30)]
		[NinjaScriptProperty]
		[Display(Name = "Jahre zu laden", Order = 1, GroupName = "Parameter", Description = "Wie viele Jahre geladen werden sollen.")]
		public int years { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "End Datum", Order = 2, GroupName = "Parameter", Description = "Bis wann Daten geladen werden sollen.")]
		public DateTime dateTo { get; set; }
		
		[Display(Name = "Breite", Order = 3, GroupName = "Parameter", Description = "Wenn leer, dann wird die Breite automatisch berechnet. Ansonsten werden die VP-Bars maximal so breit wie hier angegeben.")]
		public int? maxWidth { get; set; }
		
		#region Colors
		[XmlIgnore]
		[Display(Name = "Volumen", Order = 1, GroupName = "Farben")]
		public Brush normalAreaBrush { get; set; }
		[Browsable(false)]
		public string normalAreaBrushSerialize {
			get { return Serialize.BrushToString(normalAreaBrush); }
			set { normalAreaBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "PoC", Order = 2, GroupName = "Farben")]
		public Brush pocBrush { get; set; }
		[Browsable(false)]
		public string pocBrushSerialize {
			get { return Serialize.BrushToString(pocBrush); }
			set { pocBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Text", Order = 3, GroupName = "Farben")]
		public Brush textBrush { get; set; }
		[Browsable(false)]
		public string textBrushSerialize {
			get { return Serialize.BrushToString(textBrush); }
			set { textBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		#endregion
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Volume Profile - Groß";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				IsChartOnly									= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 0;
				ZOrder										= 0;

				loadRecent									= true;
				years										= 20;
				dateTo										= DateTime.Now;
				normalAreaBrush								= Brushes.Gray.Clone();
				normalAreaBrush.Opacity						= 0.5;
				pocBrush									= Brushes.Red.Clone();
				pocBrush.Opacity							= 0.5;
				textBrush									= Brushes.White;
			} else if (State == State.DataLoaded) {
				dataLoaded = false;
				if ((SuriAddOn.license == License.PremiumCot || SuriAddOn.license == License.Dev) && loadRecent) {
					suriVpBigData = SuriVpSerialization.GetVpBig(Instrument);
					if (suriVpBigData != null) dataLoaded = true;
				}
				if (!dataLoaded) {
					suriVpBigData = new SuriVpBigData(TickSize);
					new BarsRequest(Instrument, dateTo.AddYears(-years).Date, dateTo.AddDays(-1).Date) {
						MergePolicy		= MergePolicy.MergeBackAdjusted,
						BarsPeriod		= new BarsPeriod { BarsPeriodType = BarsPeriodType.Minute, Value = 1 },
						TradingHours	= TradingHours,
					}.Request((bars, errorCode, errorMessage) => {
						if (errorCode != ErrorCode.NoError) {
							Print(string.Format("Error on requesting bars: {0}, {1}", errorCode, errorMessage));
							return;
						}
						for (int i = 0; i < bars.Bars.Count; i++) {
							suriVpBigData.AddMinuteVolume(bars.Bars.GetVolume(i), bars.Bars.GetHigh(i), bars.Bars.GetLow(i));
						}
						dataLoaded = true;
						ForceRefresh();
					});
				}
			}
		}

		public override void OnRenderTargetChanged() {
			// if dxBrush exists on first render target change, dispose of it
			if (normalAreaFill != null) {
				normalAreaFill.Dispose();
				pocFill.Dispose();
				textFill.Dispose();
			}
			if (RenderTarget != null) {
				normalAreaFill = normalAreaBrush.ToDxBrush(RenderTarget);
				pocFill = pocBrush.ToDxBrush(RenderTarget);
				textFill = textBrush.ToDxBrush(RenderTarget);
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (!dataLoaded || SuriAddOn.license == License.None || Bars == null || Bars.Instrument == null || IsInHitTest) {
				return;
			}
			if (!suriVpBigData.isPrepared) {
				suriVpBigData.Prepare();
			}
			
			int highestValue = (int) Math.Floor(chartScale.MaxValue / TickSize);
			int lowestValue  = (int) Math.Floor(chartScale.MinValue / TickSize);
			int tickCount = highestValue - lowestValue;

			float rectHeight = (chartScale.GetYByValue(0) - chartScale.GetYByValue(1000 * TickSize)) / 1000f;
			RectangleF rect = new RectangleF {
				X = 0,
				Height = rectHeight,
			};
			if (highestValue > suriVpBigData.tickData.Last().Key) {
				rect.Y = chartScale.GetYByValue((suriVpBigData.tickData.Last().Key+1) * TickSize) - rect.Height / 2f;
			} else {
				rect.Y = chartScale.GetYByValue((highestValue+1) * TickSize) - rect.Height / 2f;
			}
			
			if (rect.Height > 13) {
				SimpleFont font = new SimpleFont {Size = rect.Height * 0.53};
				textFormat = font.ToDirectWriteTextFormat();
				textFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
				textFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
			}

			for (int i = 0; i < tickCount; i++) {
				int tick = highestValue - i;
				if (suriVpBigData.tickData.ContainsKey(tick)) {
					SuriVpTickData tickData = suriVpBigData.tickData[tick];

					rect.Width = (float) ((maxWidth ?? ChartPanel.W * 0.6) * tickData.volume / suriVpBigData.pocVolume);
					rect.Y += rect.Height;
					rect.Height = tickData.isMainPoc ? Math.Max(1, rectHeight) : rectHeight;
					
					RenderTarget.FillRectangle(rect, tickData.isMainPoc ? pocFill : normalAreaFill);
					if (rect.Height > 13) {
						SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, tickData.volume.ToString("F0"), textFormat, 250, textFormat.FontSize);
						RenderTarget.DrawTextLayout(new Vector2(0, rect.Y), textLayout, textFill, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					}
				}
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
		private Suri.SuriVolumeProfileBig[] cacheSuriVolumeProfileBig;
		public Suri.SuriVolumeProfileBig SuriVolumeProfileBig(bool loadRecent, int years, DateTime dateTo)
		{
			return SuriVolumeProfileBig(Input, loadRecent, years, dateTo);
		}

		public Suri.SuriVolumeProfileBig SuriVolumeProfileBig(ISeries<double> input, bool loadRecent, int years, DateTime dateTo)
		{
			if (cacheSuriVolumeProfileBig != null)
				for (int idx = 0; idx < cacheSuriVolumeProfileBig.Length; idx++)
					if (cacheSuriVolumeProfileBig[idx] != null && cacheSuriVolumeProfileBig[idx].loadRecent == loadRecent && cacheSuriVolumeProfileBig[idx].years == years && cacheSuriVolumeProfileBig[idx].dateTo == dateTo && cacheSuriVolumeProfileBig[idx].EqualsInput(input))
						return cacheSuriVolumeProfileBig[idx];
			return CacheIndicator<Suri.SuriVolumeProfileBig>(new Suri.SuriVolumeProfileBig(){ loadRecent = loadRecent, years = years, dateTo = dateTo }, input, ref cacheSuriVolumeProfileBig);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriVolumeProfileBig SuriVolumeProfileBig(bool loadRecent, int years, DateTime dateTo)
		{
			return indicator.SuriVolumeProfileBig(Input, loadRecent, years, dateTo);
		}

		public Indicators.Suri.SuriVolumeProfileBig SuriVolumeProfileBig(ISeries<double> input , bool loadRecent, int years, DateTime dateTo)
		{
			return indicator.SuriVolumeProfileBig(input, loadRecent, years, dateTo);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriVolumeProfileBig SuriVolumeProfileBig(bool loadRecent, int years, DateTime dateTo)
		{
			return indicator.SuriVolumeProfileBig(Input, loadRecent, years, dateTo);
		}

		public Indicators.Suri.SuriVolumeProfileBig SuriVolumeProfileBig(ISeries<double> input , bool loadRecent, int years, DateTime dateTo)
		{
			return indicator.SuriVolumeProfileBig(input, loadRecent, years, dateTo);
		}
	}
}

#endregion
