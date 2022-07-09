#region Using declarations
using System;
using NinjaTrader.Gui.Chart;
using System.Windows.Media;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using SharpDX;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriVolumeProfileBig : Indicator {
		private SuriVpBigData suriVpBigData;
		private bool dataLoaded;
		private SimpleFont font;
		
		#region Properties
		private SharpDX.Direct2D1.Brush normalAreaFill;
		private SharpDX.Direct2D1.Brush pocFill;
		private SharpDX.Direct2D1.Brush textFill;
		private SharpDX.Direct2D1.Brush lvnFill;
		private SharpDX.DirectWrite.TextFormat textFormat;
		
		/*[Browsable(false)]
		[Display(Name = "Jahre zu laden", Order = 1, GroupName = "Parameter", Description = "Wie viele Jahre an Daten geladen werden sollen.")]
		public int years { get; set; }*/
		
		[NinjaScriptProperty]
		[Display(Name = "Schnelles VP", Order = 0, GroupName = "Parameter", Description = "Wenn aktiv, wird das aktuelle Volumen Profil geladen und die beiden Parameter 'Jahre zu laden' und 'End Datum' haben dann keine Auswirkung mehr! Hierfür ist die Premium Version nötig.")]
		public bool loadRecent { get; set; }
		
		[Range(1, 30)]
		[NinjaScriptProperty]
		[Display(Name = "Jahre zu laden", Order = 1, GroupName = "Parameter", Description = "Wie viele Jahre geladen werden sollen.")]
		public int years { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "End Datum", Order = 2, GroupName = "Parameter", Description = "Bis wann Daten geladen werden sollen.")]
		public DateTime dateTo { get; set; }
		
		[Range(10, 100)]
		[Display(Name = "Breite in %", Order = 3, GroupName = "Parameter", Description = "Die Breite des Volumenprofils prozentual zur Chartbreite.")]
		public int width { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Dynamische Breite", Order = 4, GroupName = "Parameter", Description = "Wenn aktiv, dann wird die Breite des VPs ausgehend von dem breitesten aktuell sichtbaren Tick berechnet.\nWenn nicht aktiv, wird die Breite des VPs ausgehend vom Haupt-POC berechnet. \nDie Berechnung mit aktivierter Checkbox ist rechenintensiv! Schalte es aus, wenn es bei Dir hängt.")]
		public bool dynamicWidth { get; set; }
		
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

				width										= 60;
				dynamicWidth								= false;
				loadRecent									= true;
				years										= 20;
				dateTo										= DateTime.Now;
				normalAreaBrush								= Brushes.DimGray;
				pocBrush									= Brushes.Red;
				textBrush									= Brushes.White;
			} else if (State == State.DataLoaded) {
				SetZOrder(-100);
				dataLoaded = false;
				if ((SuriAddOn.license == License.Premium || SuriAddOn.license == License.Dev) && loadRecent) {
					suriVpBigData = SuriBigRepo.GetVpBig(Instrument);
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
		public override string DisplayName { get { return Name; } }

		public override void OnRenderTargetChanged() {
			// if dxBrush exists on first render target change, dispose of it
			if (normalAreaFill != null) {
				normalAreaFill.Dispose();
				pocFill.Dispose();
				textFill.Dispose();
				lvnFill.Dispose();
			}
			if (RenderTarget != null) {
				normalAreaFill = normalAreaBrush.ToDxBrush(RenderTarget);
				pocFill = pocBrush.ToDxBrush(RenderTarget);
				textFill = textBrush.ToDxBrush(RenderTarget);
				lvnFill = Brushes.Yellow.ToDxBrush(RenderTarget);
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (SuriAddOn.license == License.None) {
				SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
				return;
			}
			if (!dataLoaded || Bars == null || Bars.Instrument == null || IsInHitTest) return;
			if (!suriVpBigData.isPrepared) suriVpBigData.Prepare();
			
			int highestValue = (int) Math.Floor(chartScale.MaxValue / TickSize);
			int lowestValue  = (int) Math.Floor(chartScale.MinValue / TickSize);

			double? highestVisibleTick = null;
			if (dynamicWidth) {
				highestVisibleTick = double.MinValue;
				for (int tick = highestValue+1; tick >= lowestValue; tick--) {
					if (suriVpBigData.tickData.ContainsKey(tick)) {
						highestVisibleTick = Math.Max(suriVpBigData.tickData[tick].volume, highestVisibleTick.Value);
					}
				}
			}

			bool isFirstBar = true;
			for (int tick = highestValue+1; tick >= lowestValue; tick--) {
				if (suriVpBigData.tickData.ContainsKey(tick)) {
					DrawVolumeBar(chartScale, suriVpBigData.tickData[tick], highestVisibleTick, isFirstBar);
					isFirstBar = false;
				}
			}
			
			// draw main poc again because it might be overlapped by other volume bars 
			DrawVolumeBar(chartScale, suriVpBigData.PocTickData(), highestVisibleTick, false);
		}

		
		private bool showSpaceBetweenBars;
		private bool showText;
		private void DrawVolumeBar(ChartScale chartScale, SuriVpTickData tickData, double? highestVisibleTick, bool isFirstBar) {
			RectangleF rect = new RectangleF { X = 0 };
			double priceLower = tickData.tick * TickSize - TickSize / 2;
			float yLower = chartScale.GetYByValue(priceLower);
			float yUpper = chartScale.GetYByValue(priceLower + TickSize);
			float height = Math.Abs(yUpper - yLower);
			if (isFirstBar) {
				if      ( showSpaceBetweenBars && height <= 4) showSpaceBetweenBars = false;
				else if (!showSpaceBetweenBars && height >= 6) showSpaceBetweenBars = true;
				
				if      ( showText && height <= 10) showText = false;
				else if (!showText && height >= 12) showText = true;
			}
			if (showSpaceBetweenBars) height -= 1;
			height = Math.Max(tickData.isMainPoc ? 2 : 1, height);
			
			rect.Width = (float) ((ChartPanel.W * width / 100.0) * tickData.volume / (highestVisibleTick ?? suriVpBigData.pocVolume));
			rect.Y = yUpper;
			rect.Height = height;
			RenderTarget.FillRectangle(rect, tickData.isLvn ? lvnFill : ((tickData.isMainPoc || tickData.isSubPoc) ? pocFill : normalAreaFill));
			
			if (showText) {
				if (font == null || isFirstBar) {
					font = new SimpleFont {Size = rect.Height * 0.5};
					textFormat = font.ToDirectWriteTextFormat();
					textFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
					textFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
				}
				SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, tickData.volume.ToString("F0"), textFormat, 250, textFormat.FontSize);
				RenderTarget.DrawTextLayout(new Vector2(0, rect.Y), textLayout, textFill, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
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
		public Suri.SuriVolumeProfileBig SuriVolumeProfileBig(bool loadRecent, int years, DateTime dateTo, bool dynamicWidth)
		{
			return SuriVolumeProfileBig(Input, loadRecent, years, dateTo, dynamicWidth);
		}

		public Suri.SuriVolumeProfileBig SuriVolumeProfileBig(ISeries<double> input, bool loadRecent, int years, DateTime dateTo, bool dynamicWidth)
		{
			if (cacheSuriVolumeProfileBig != null)
				for (int idx = 0; idx < cacheSuriVolumeProfileBig.Length; idx++)
					if (cacheSuriVolumeProfileBig[idx] != null && cacheSuriVolumeProfileBig[idx].loadRecent == loadRecent && cacheSuriVolumeProfileBig[idx].years == years && cacheSuriVolumeProfileBig[idx].dateTo == dateTo && cacheSuriVolumeProfileBig[idx].dynamicWidth == dynamicWidth && cacheSuriVolumeProfileBig[idx].EqualsInput(input))
						return cacheSuriVolumeProfileBig[idx];
			return CacheIndicator<Suri.SuriVolumeProfileBig>(new Suri.SuriVolumeProfileBig(){ loadRecent = loadRecent, years = years, dateTo = dateTo, dynamicWidth = dynamicWidth }, input, ref cacheSuriVolumeProfileBig);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriVolumeProfileBig SuriVolumeProfileBig(bool loadRecent, int years, DateTime dateTo, bool dynamicWidth)
		{
			return indicator.SuriVolumeProfileBig(Input, loadRecent, years, dateTo, dynamicWidth);
		}

		public Indicators.Suri.SuriVolumeProfileBig SuriVolumeProfileBig(ISeries<double> input , bool loadRecent, int years, DateTime dateTo, bool dynamicWidth)
		{
			return indicator.SuriVolumeProfileBig(input, loadRecent, years, dateTo, dynamicWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriVolumeProfileBig SuriVolumeProfileBig(bool loadRecent, int years, DateTime dateTo, bool dynamicWidth)
		{
			return indicator.SuriVolumeProfileBig(Input, loadRecent, years, dateTo, dynamicWidth);
		}

		public Indicators.Suri.SuriVolumeProfileBig SuriVolumeProfileBig(ISeries<double> input , bool loadRecent, int years, DateTime dateTo, bool dynamicWidth)
		{
			return indicator.SuriVolumeProfileBig(input, loadRecent, years, dateTo, dynamicWidth);
		}
	}
}

#endregion
