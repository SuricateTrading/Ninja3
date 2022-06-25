#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Gui.Chart;
using SharpDX;
using System.Windows.Media;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.Core;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using NinjaTrader.Gui.NinjaScript;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Brush = System.Windows.Media.Brush;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriVolumeProfileIntraday : Indicator {
		private SuriVpIntraData suriVpIntraData = new SuriVpIntraData();
		private int? lastBar;
		private RectangleF rect;
		
		private SharpDX.Direct2D1.Brush normalAreaFill;
		private SharpDX.Direct2D1.Brush pocFill;
		private SharpDX.Direct2D1.Brush vaFill;
		private SharpDX.Direct2D1.Brush textFill;
		private SharpDX.Direct2D1.Brush smaFill;
		private SharpDX.Direct2D1.Brush footprintFill;
		private SharpDX.Direct2D1.Brush boxFill;
		private SharpDX.Direct2D1.Brush tickTextFill;
		private SharpDX.Direct2D1.Brush testing1Fill;
		private SharpDX.Direct2D1.Brush testing2Fill;
		private SharpDX.Direct2D1.Brush testing3Fill;
		private SharpDX.DirectWrite.TextFormat textFormatBarInfo;
		private SharpDX.DirectWrite.TextFormat textFormatTickInfo;
		
		#region Properties
		[Display(Name = "Breite", Order = 0, GroupName = "Parameter", Description = "Wenn leer, dann wird die Breite automatisch berechnet. Ansonsten wird es maximal so breit.")]
		public int? maxWidth { get; set; }
		
		[Display(Name = "Entfernung zur Bar", Order = 0, GroupName = "Parameter", Description = "Wie weit es von der Mitte der Bar entfernt sein soll.")]
		public int offset { get; set; }
		
		[Display(Name = "Textgröße", Order = 0, GroupName = "Parameter")]
		public int textSize { get; set; }
		
		[Display(Name = "Zeige Text unter der Bar", Order = 1, GroupName = "Parameter")]
		public bool drawText { get; set; }
		[Display(Name = "Zeige Text bei Ticks", Order = 2, GroupName = "Parameter")]
		public bool drawTickText { get; set; }
		[Display(Name = "Zeige 'Naked PoC'", Order = 3, GroupName = "Parameter")]
		public bool drawNakedPoc { get; set; }
		[Display(Name = "Unterteile in Bid/Ask", Order = 4, GroupName = "Parameter", Description = "Unterteilt das Volumen in Bids und Asks. Wird auch als Footprintchart bezeichnet.")]
		public bool showBidAsk { get; set; }
		[Display(Name = "Platz zwischen Bid/Ask", Order = 5, GroupName = "Parameter", Description = "Wenn das Volumen in Bids und Asks unterteilt ist, stellt man hiermit den Platz zwischen den Bid und Ask Balken ein.")]
		public int bidAskSpace { get; set; }
		[Display(Name = "Zeige Bid/Ask Delta (P)", Order = 6, GroupName = "Parameter", Description = "Zeichnet eine Linie, die die Differenz zwischen Bids und Asks für jeden Tick anzeigt. Nur für Premium-Suris.")]
		public bool showBidAskDelta { get; set; }
		[Display(Name = "Breite der Bid/Ask Delta Linie", Order = 6, GroupName = "Parameter")]
		[Range(1, 100)]
		public int bidAskDeltaLineWidth { get; set; }
		
		#region Colors
		[XmlIgnore]
		[Display(Name = "Value Area", Order = 0, GroupName = "Farben")]
		public Brush valueAreaBrush { get; set; }
		[Browsable(false)]
		public string valueAreaBrushSerialize {
			get { return Serialize.BrushToString(valueAreaBrush); }
			set { valueAreaBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Normal Area", Order = 1, GroupName = "Farben")]
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
		
		private Brush smaBrush { get; set; }
		private Brush boxBrush { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Bid Ask Delta Linie", Order = 3, GroupName = "Farben")]
		public Brush footprintBrush { get; set; }
		[Browsable(false)]
		public string footprintBrushSerialize {
			get { return Serialize.BrushToString(footprintBrush); }
			set { footprintBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "Text bei Ticks", Order = 4, GroupName = "Farben")]
		public Brush tickTextBrush { get; set; }
		[Browsable(false)]
		public string tickTextBrushSerialize {
			get { return Serialize.BrushToString(tickTextBrush); }
			set { tickTextBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(Name = "Deckkraft der Volumenfarben in %", Order = 5, GroupName = "Farben", Description = "Wie hoch die Deckkraft der Value Area und Normal Area sein soll. Bei niedriger Deckkraft kann man besser die normale Bar dahinter sehen.")]
		[Range(1, 100)]
		public int opacity { get; set; }
		
		#endregion
		#endregion
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"";
				Name										= "Volume Profile - Intraday";
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
				ZOrder										= 2;
				
				textSize									= 12;
				offset										= 0;
				maxWidth									= null;
				drawText									= true;
				drawTickText								= true;
				drawNakedPoc								= false;
				showBidAsk									= false;
				showBidAskDelta								= false;
				valueAreaBrush								= Brushes.RoyalBlue.Clone();
				normalAreaBrush								= Brushes.DarkGray.Clone();
				pocBrush									= Brushes.DarkOrange;
				smaBrush									= Brushes.Yellow;
				footprintBrush								= Brushes.Yellow;
				tickTextBrush								= Brushes.White;
				boxBrush									= Brushes.CornflowerBlue.Clone();
				boxBrush.Opacity							= 0.5;
				opacity										= 100;
				bidAskSpace									= 5;
				bidAskDeltaLineWidth						= 2;
			} else if (State == State.Configure) {
				textFormatBarInfo = new TextFormat(Globals.DirectWriteFactory,"Arial", textSize);
				if (!Bars.IsTickReplay && SuriAddOn.license == License.Dev /*&& Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && Bars.BarsPeriod.Value == 1440*/) {
					suriVpIntraData = SuriIntraRepo.GetVpIntra(Instrument, Bars.GetTime(0).Date, Bars.LastBarTime.Date);
				}
				valueAreaBrush	= valueAreaBrush.Clone();
				normalAreaBrush	= normalAreaBrush.Clone();
				normalAreaBrush.Opacity = opacity / 100.0;
				valueAreaBrush.Opacity = opacity / 100.0;
			}
		}
		/*
		 * todo:
		 *	- wenn eine neue bar entsteht, wird das volumen dort nicht angesammelt.
		 */

		protected override void OnMarketData(MarketDataEventArgs e) {
			if (SuriAddOn.license == License.None || Bars.Count <= 0 || !Bars.IsTickReplay || e.MarketDataType != MarketDataType.Last) return;
			if (lastBar != CurrentBar) {
				lastBar = CurrentBar;
				suriVpIntraData.barData.Add(new SuriVpBarData(TickSize, e.Time));
			}
			//Print(e.Time + "\t" + e.Volume + "\t" + e.Price + "\t" + e.Ask + "\t" + e.Bid);
			suriVpIntraData.barData.Last().AddTick(e);
		}
		
		public override void OnRenderTargetChanged() {
			// if dxBrush exists on first render target change, dispose of it
			if (normalAreaFill != null) {
				normalAreaFill.Dispose();
				pocFill.Dispose();
				vaFill.Dispose();
				textFill.Dispose();
				smaFill.Dispose();
				footprintFill.Dispose();
				boxFill.Dispose();
				tickTextFill.Dispose();
				testing1Fill.Dispose();
				testing2Fill.Dispose();
				testing3Fill.Dispose();
			}
			if (RenderTarget != null) {
				normalAreaFill = normalAreaBrush.ToDxBrush(RenderTarget);
				pocFill = pocBrush.ToDxBrush(RenderTarget);
				vaFill = valueAreaBrush.ToDxBrush(RenderTarget);
				if (ChartControl != null) textFill = ChartControl.Properties.ChartText.ToDxBrush(RenderTarget);
				smaFill = smaBrush.ToDxBrush(RenderTarget);
				footprintFill = footprintBrush.ToDxBrush(RenderTarget);
				boxFill = boxBrush.ToDxBrush(RenderTarget);
				tickTextFill = tickTextBrush.ToDxBrush(RenderTarget);
				testing1Fill = Brushes.Red.ToDxBrush(RenderTarget);
				testing2Fill = Brushes.Green.ToDxBrush(RenderTarget);
				testing3Fill = Brushes.Yellow.ToDxBrush(RenderTarget);
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (SuriAddOn.license == License.None) {
				SuriCommon.NoValidLicenseError(RenderTarget, ChartControl, ChartPanel);
				return;
			}
			if (Bars == null || Bars.Instrument == null || IsInHitTest || suriVpIntraData == null || suriVpIntraData.barData.IsNullOrEmpty()) {
				return;
			}
			if (!suriVpIntraData.isPrepared) suriVpIntraData.Prepare();
			if (textFill == null) textFill = ChartControl.Properties.ChartText.ToDxBrush(RenderTarget);

			rect = new RectangleF();
			float barWidth = (float) (chartControl.GetXByBarIndex(ChartBars, 1) - chartControl.GetXByBarIndex(ChartBars, 0) - chartControl.BarWidth*1.2);

			bool isFirstTick = true;
			for (int barIndex = ChartBars.FromIndex; barIndex <= ChartBars.ToIndex; barIndex++) {
				/*if (!Bars.IsTickReplay) {
					// only for cached VP intra data...
					// this can only happen in a 1440-minute chart when tickreplay is disabled.
					// the user may have MORE bars loaded than vp-data is laoded, because cached vp data is only available since *SuriIntraRepo.startOfCachedData*
					if (ChartBars.Bars.GetTime(barIndex).Date < SuriIntraRepo.startOfCachedData.Date) {
						continue;
					}
					for (; barIndex <= suriVpIntraData.barData.Count; barIndex++) {
						if (barIndex == suriVpIntraData.barData.Count) return;
						DateTime d1 = suriVpIntraData.barData[barIndex].dateTime;
						DateTime d2 = ChartBars.Bars.GetTime(barIndex);
						if (d1 == d2) { // || d1.DayOfYear == d2.DayOfYear && d1.Year == d2.Year
							break;
						}
					}
				}*/

				previousDelta = null;
				
				// Draw for each tick
				rect.X = chartControl.GetXByBarIndex(ChartBars, barIndex) + offset;
				foreach (KeyValuePair<int, SuriVpTickData> entry in suriVpIntraData.barData[barIndex].tickData) {
					SuriVpTickData tickData = entry.Value;
					DrawVolumeBar(chartScale, tickData, barWidth, barIndex, isFirstTick);
					DrawNakedPoc(tickData);
					DrawBidAskDeltaLine(barIndex, tickData, barWidth);
					
					//DrawLvn();
					//DrawDistributedVolume();
					
					isFirstTick = false;
				}
				
				// Draw per Bar
				if (drawText && barWidth > 20) DrawText(barIndex, chartScale);
				//if (SuriAddOn.license == License.Dev) DrawBox(barIndex, chartScale, chartControl);
			}
		}

		private bool showSpaceBetweenBars;
		private bool showTickBarText;
		private void DrawVolumeBar(ChartScale chartScale, SuriVpTickData tickData, float barWidth, int barIndex, bool isFirstBar) {
			double priceLower = tickData.tick * TickSize - TickSize / 2;
			float yLower = chartScale.GetYByValue(priceLower);
			float yUpper = chartScale.GetYByValue(priceLower + TickSize);
			float height = Math.Abs(yUpper - yLower);
			if (isFirstBar) {
				if      ( showSpaceBetweenBars && height <= 4) showSpaceBetweenBars = false;
				else if (!showSpaceBetweenBars && height >= 6) showSpaceBetweenBars = true;
				
				if      ( showTickBarText && height <= 10) showTickBarText = false;
				else if (!showTickBarText && height >= 12) showTickBarText = true;
			}
			if (showSpaceBetweenBars) height -= 1;
			height = Math.Max(tickData.isMainPoc ? 2 : 1, height);
			float initialX = rect.X;
			rect.Y = yUpper;
			rect.Height = height;
			
			SharpDX.Direct2D1.Brush b;
			/*if (entry.Value.isLvn)			b = testing1Fill;
			else if (entry.Value.isHigh)		b = testing2Fill;
			else */
			if (tickData.isMainPoc)				b = pocFill;
			else if (tickData.isInValueArea)	b = vaFill;
			else								b = normalAreaFill;
			
			if (showBidAsk) {
				// left
				rect.Width = -(float) ((maxWidth ?? barWidth) * tickData.bids / suriVpIntraData.barData[barIndex].pocVolume);
				rect.X -= bidAskSpace;
				RenderTarget.FillRectangle(rect, b);
				// right
				rect.Width = (float) ((maxWidth ?? barWidth) * tickData.asks / suriVpIntraData.barData[barIndex].pocVolume);
				rect.X += bidAskSpace * 2;
				RenderTarget.FillRectangle(rect, b);
				
				rect.X -= bidAskSpace;
				
				if (drawTickText && showTickBarText) {
					if (textFormatTickInfo == null || isFirstBar) {
						textFormatTickInfo = new TextFormat(Globals.DirectWriteFactory,"Arial", rect.Height * 0.85f);
					}
					TextLayout textLayout = new TextLayout(Globals.DirectWriteFactory, tickData.bids.ToString("F0"), textFormatTickInfo, rect.X - bidAskSpace - 5, ChartPanel.H);
					textLayout.TextAlignment = TextAlignment.Trailing;
					RenderTarget.DrawTextLayout(new Vector2(0, rect.Y), textLayout, tickTextFill, DrawTextOptions.NoSnap);
					
					textLayout = new TextLayout(Globals.DirectWriteFactory, tickData.asks.ToString("F0"), textFormatTickInfo, ChartPanel.W, ChartPanel.H);
					RenderTarget.DrawTextLayout(new Vector2(rect.X + bidAskSpace + 5, rect.Y), textLayout, tickTextFill, DrawTextOptions.NoSnap);
				}
			}
			
			rect.X = initialX;
			rect.Width = (float) ((maxWidth ?? barWidth) * tickData.volume / suriVpIntraData.barData[barIndex].pocVolume);
			
			if (!showBidAsk) {
				RenderTarget.FillRectangle(rect, b);
				if (drawTickText && showTickBarText) {
					if (textFormatTickInfo == null || isFirstBar) {
						textFormatTickInfo = new TextFormat(Globals.DirectWriteFactory,"Arial", rect.Height * 0.85f);
					}
					TextLayout textLayout = new TextLayout(Globals.DirectWriteFactory, tickData.volume.ToString("F0"), textFormatTickInfo, ChartPanel.W, ChartPanel.H);
					RenderTarget.DrawTextLayout(new Vector2(rect.X + 10, rect.Y), textLayout, tickTextFill, DrawTextOptions.NoSnap);
				}
			}
		}
		
		private void DrawNakedPoc(SuriVpTickData tickData) {
			if (!drawNakedPoc || !tickData.isNakedPoc) return;
			
			float pocX1 = rect.X + rect.Width + 10;
			float pocX2 = (float) ChartPanel.W - 200;
			if (pocX2 - pocX1 > 0) {
				RenderTarget.DrawLine(
					new Vector2(pocX1, rect.Y + rect.Height/2.0f),
					new Vector2(pocX2, rect.Y + rect.Height/2.0f),
					pocFill, 1.5f
				);
				string vpocText = "< VPOC";
				TextLayout textLayout  = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, vpocText, textFormatBarInfo, 250, textFormatBarInfo.FontSize);
				RenderTarget.DrawTextLayout(
					new Vector2(ChartPanel.W - 190, rect.Y + rect.Height/2.0f - textSize + 2f),
					textLayout, pocFill, SharpDX.Direct2D1.DrawTextOptions.NoSnap
				);
			}
		}

		private float? previousDelta;
		private void DrawBidAskDeltaLine(int barIndex, SuriVpTickData tickData, float barWidth) {
			if (!showBidAskDelta || SuriAddOn.license != License.Dev && SuriAddOn.license != License.Premium) return;
			float delta = (float) (((maxWidth ?? barWidth)/2f) * (tickData.asks - tickData.bids) / suriVpIntraData.barData[barIndex].highestDelta);
			if (previousDelta != null) {
				RenderTarget.DrawLine(
					new Vector2(rect.X + previousDelta.Value, rect.Y + rect.Height + rect.Height / 2f),
					new Vector2(rect.X + delta              , rect.Y + rect.Height / 2f),
					footprintFill, bidAskDeltaLineWidth
				);
			}
			previousDelta = delta;
		}

		private void DrawLvn() {
			/*if (entry.Value.isLvn) {
				RenderTarget.DrawLine(
					new Vector2(rect.X + rect.Width + 10 + 100, rect.Y + rect.Height/2.0f),
					new Vector2(rect.X + rect.Width + 10 + 200, rect.Y + rect.Height/2.0f),
					testing1Fill, 1.5f
				);
			}*/
		}

		private void DrawDistributedVolume() {
			/*if (previousDistVolWidth != null) {
				
				float distVolWidth = (float) ((maxWidth ?? barWidth) * entry.Value.distributedVolume / vpIntraData.barData[lastIndex].pocVolume);
				RenderTarget.DrawLine(
					new Vector2(rect.X + previousDistVolWidth.Value, rect.Y + rect.Height + rect.Height / 2f),
					new Vector2(rect.X + distVolWidth, rect.Y + rect.Height / 2f),
					smaFill
				);
			}
			previousDistVolWidth = (float) ((maxWidth ?? barWidth) * entry.Value.distributedVolume / vpIntraData.barData[lastIndex].pocVolume);
			*/
		}

		private void DrawText(int barIndex, ChartScale chartScale) {
			double delta = suriVpIntraData.barData[barIndex].delta;
			string str =	"Σ " + suriVpIntraData.barData[barIndex].totalVolume + "\n" +
			                "∆ " + delta + "\n" +
			                "∆% " + (100 * delta / suriVpIntraData.barData[barIndex].totalVolume).ToString("F1") + "%\n" +
			                "Ticks " + (suriVpIntraData.barData[barIndex].tickData.Count - 1) + "\n" +
			                "VA " + suriVpIntraData.barData[barIndex].vaPercentage + "%"
			;
			var textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, str, textFormatBarInfo, ChartPanel.W, ChartPanel.H);
			float y = chartScale.GetYByValue(ChartBars.Bars.GetLow(barIndex)) + 10f;
			RenderTarget.DrawTextLayout(new Vector2(rect.X, y + rect.Height/2.0f), textLayout, textFill, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
		}

		private void DrawBox(int barIndex, ChartScale chartScale, ChartControl chartControl) {
			if (suriVpIntraData.boxes.ContainsKey(barIndex)) {
				SuriVpBox box = suriVpIntraData.boxes[barIndex];
				RectangleF boxRect = new RectangleF {
					X = chartControl.GetXByBarIndex(ChartBars, barIndex),
					Y = chartScale.GetYByValue(box.boxHigh * TickSize)
				};
				boxRect.Width = chartControl.GetXByBarIndex(ChartBars, barIndex - box.length + 1) - boxRect.X;
				boxRect.Height = chartScale.GetYByValue(box.boxLow * TickSize) - boxRect.Y;
				RenderTarget.FillRectangle(boxRect, boxFill);
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
		private Suri.SuriVolumeProfileIntraday[] cacheSuriVolumeProfileIntraday;
		public Suri.SuriVolumeProfileIntraday SuriVolumeProfileIntraday()
		{
			return SuriVolumeProfileIntraday(Input);
		}

		public Suri.SuriVolumeProfileIntraday SuriVolumeProfileIntraday(ISeries<double> input)
		{
			if (cacheSuriVolumeProfileIntraday != null)
				for (int idx = 0; idx < cacheSuriVolumeProfileIntraday.Length; idx++)
					if (cacheSuriVolumeProfileIntraday[idx] != null &&  cacheSuriVolumeProfileIntraday[idx].EqualsInput(input))
						return cacheSuriVolumeProfileIntraday[idx];
			return CacheIndicator<Suri.SuriVolumeProfileIntraday>(new Suri.SuriVolumeProfileIntraday(), input, ref cacheSuriVolumeProfileIntraday);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriVolumeProfileIntraday SuriVolumeProfileIntraday()
		{
			return indicator.SuriVolumeProfileIntraday(Input);
		}

		public Indicators.Suri.SuriVolumeProfileIntraday SuriVolumeProfileIntraday(ISeries<double> input )
		{
			return indicator.SuriVolumeProfileIntraday(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriVolumeProfileIntraday SuriVolumeProfileIntraday()
		{
			return indicator.SuriVolumeProfileIntraday(Input);
		}

		public Indicators.Suri.SuriVolumeProfileIntraday SuriVolumeProfileIntraday(ISeries<double> input )
		{
			return indicator.SuriVolumeProfileIntraday(input);
		}
	}
}

#endregion
