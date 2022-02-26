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
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui.NinjaScript;
using Brush = System.Windows.Media.Brush;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriVolumeProfileIntraday : Indicator {
		private readonly VpIntraData vpIntraData = new VpIntraData();
		private int? lastBar;
		
		#region Properties
		private bool prepared;
		private SharpDX.Direct2D1.Brush normalAreaFill;
		private SharpDX.Direct2D1.Brush pocFill;
		private SharpDX.Direct2D1.Brush vaFill;
		private SharpDX.Direct2D1.Brush textFill;
		private SharpDX.Direct2D1.Brush smaFill;
		private SharpDX.Direct2D1.Brush testing1Fill;
		private SharpDX.Direct2D1.Brush testing2Fill;
		private SharpDX.Direct2D1.Brush testing3Fill;
		private SharpDX.DirectWrite.TextFormat textFormat;
		
		[Display(Name = "Breite", Order = 0, GroupName = "Parameter", Description = "Wenn leer, dann wird die Breite automatisch berechnet. Ansonsten wird es maximal so breit.")]
		public int? maxWidth { get; set; }
		
		[Display(Name = "Entfernung zur Bar", Order = 0, GroupName = "Parameter", Description = "Wie weit es von der Mitte der Bar entfernt sein soll.")]
		public int offset { get; set; }
		
		[Display(Name = "Textgröße", Order = 0, GroupName = "Parameter")]
		public int textSize { get; set; }
		
		[Display(Name = "Zeige Text", Order = 1, GroupName = "Parameter")]
		public bool drawText { get; set; }
		[Display(Name = "Zeige 'Naked PoC'", Order = 2, GroupName = "Parameter")]
		public bool drawNakedPoc { get; set; }
		
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
		[XmlIgnore]
		[Display(Name = "Text", Order = 3, GroupName = "Farben")]
		public Brush textBrush { get; set; }
		[Browsable(false)]
		public string textBrushSerialize {
			get { return Serialize.BrushToString(textBrush); }
			set { textBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore]
		[Display(Name = "SMA", Order = 4, GroupName = "Farben")]
		public Brush smaBrush { get; set; }
		[Browsable(false)]
		public string smaBrushSerialize {
			get { return Serialize.BrushToString(smaBrush); }
			set { smaBrush = Serialize.StringToBrush(value); }
		}
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
				drawNakedPoc								= true;
				valueAreaBrush								= Brushes.RoyalBlue;
				normalAreaBrush								= Brushes.Azure;
				pocBrush									= Brushes.Red;
				textBrush									= Brushes.White;
				smaBrush									= Brushes.Yellow;
			} else if (State == State.Configure) {
				prepared = false;
				SimpleFont font = new SimpleFont { Size = textSize };
				textFormat					= font.ToDirectWriteTextFormat();
				textFormat.TextAlignment	= SharpDX.DirectWrite.TextAlignment.Leading;
				textFormat.WordWrapping		= SharpDX.DirectWrite.WordWrapping.NoWrap;
			}
		}

		protected override void OnMarketData(MarketDataEventArgs e) {
			if (SuriAddOn.license == License.None || Bars.Count <= 0 || !Bars.IsTickReplay) return;
			if (lastBar != CurrentBar) {
				lastBar = CurrentBar;
				vpIntraData.barData.Add(new VpBarData(TickSize));
			}
			vpIntraData.barData.Last().AddTick(e);
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (SuriAddOn.license == License.None || Bars == null || Bars.Instrument == null || IsInHitTest) {
				return;
			}
			if (!vpIntraData.isPrepared) vpIntraData.Prepare();
			if (!prepared) {
				prepared = true;
				normalAreaFill = normalAreaBrush.ToDxBrush(RenderTarget);
				pocFill = pocBrush.ToDxBrush(RenderTarget);
				vaFill = valueAreaBrush.ToDxBrush(RenderTarget);
				textFill = textBrush.ToDxBrush(RenderTarget);
				smaFill = smaBrush.ToDxBrush(RenderTarget);
				testing1Fill = Brushes.Red.ToDxBrush(RenderTarget);
				testing2Fill = Brushes.Green.ToDxBrush(RenderTarget);
				testing3Fill = Brushes.Yellow.ToDxBrush(RenderTarget);
			}
			
			SharpDX.DirectWrite.TextLayout textLayout;
			RectangleF rect = new RectangleF();
			float barWidth = (float) (chartControl.GetXByBarIndex(ChartBars, 1) - chartControl.GetXByBarIndex(ChartBars, 0) - chartControl.BarWidth*1.2);
			
			for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++) {
				rect.X = chartControl.GetXByBarIndex(ChartBars, idx) + offset;
				double y = chartScale.GetYByValue(ChartBars.Bars.GetLow(idx));
				double height = (y - chartScale.GetYByValue(ChartBars.Bars.GetHigh(idx))) / Math.Max(1, vpIntraData.barData[idx].tickData.Count-1);

				int i = 1;
				float? previousDistVolWidth = null;
				foreach(KeyValuePair<int, VpTickData> entry in vpIntraData.barData[idx].tickData) {
					rect.Y = (float) (y - i * height + height * 0.5f);
					rect.Width = (float) ((maxWidth ?? barWidth) * entry.Value.volume / vpIntraData.barData[idx].pocVolume);
					rect.Height = (float) height;

					SharpDX.Direct2D1.Brush b;
					/*if (entry.Value.isLvn)				b = testing1Fill;
					else if (entry.Value.isHigh)		b = testing2Fill;
					else */if (entry.Value.isMainPoc)		b = pocFill;
					else if (entry.Value.isInValueArea)	b = vaFill;
					else								b = normalAreaFill;
					RenderTarget.FillRectangle(rect, b);

					/*if (entry.Value.isLvn) {
						RenderTarget.DrawLine(
							new Vector2(rect.X + rect.Width + 10 + 100, rect.Y + rect.Height/2.0f),
							new Vector2(rect.X + rect.Width + 10 + 200, rect.Y + rect.Height/2.0f),
							testing1Fill, 1.5f
						);
					}*/

					/*if (previousDistVolWidth != null) {
						
						float distVolWidth = (float) ((maxWidth ?? barWidth) * entry.Value.distributedVolume / vpIntraData.barData[idx].pocVolume);
						RenderTarget.DrawLine(
							new Vector2(rect.X + previousDistVolWidth.Value, rect.Y + rect.Height + rect.Height / 2f),
							new Vector2(rect.X + distVolWidth, rect.Y + rect.Height / 2f),
							smaFill
						);
					}
					previousDistVolWidth = (float) ((maxWidth ?? barWidth) * entry.Value.distributedVolume / vpIntraData.barData[idx].pocVolume);
					*/
					
					if (drawNakedPoc && entry.Value.isNakedPoc) {
						float pocX1 = rect.X + rect.Width + 10;
						float pocX2 = (float) ChartPanel.W - 200;
						if (pocX2 - pocX1 > 0) {
							RenderTarget.DrawLine(
								new Vector2(pocX1, rect.Y + rect.Height/2.0f),
								new Vector2(pocX2, rect.Y + rect.Height/2.0f),
								pocFill, 1.5f
							);
							string vpocText = "< VPOC";
							textLayout  = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, vpocText, textFormat, 250, textFormat.FontSize);
							RenderTarget.DrawTextLayout(
								new Vector2(ChartPanel.W - 190, rect.Y + rect.Height/2.0f - textSize + 2f),
								textLayout, pocFill, SharpDX.Direct2D1.DrawTextOptions.NoSnap
							);
						}
					}
					i++;
				}

				if (drawText) {
					string str =	"Σ " + vpIntraData.barData[idx].totalVolume + "\n" +
					                "∆ " + (vpIntraData.barData[idx].totalAsks - vpIntraData.barData[idx].totalBids) + "\n" +
					                "Ticks " + (vpIntraData.barData[idx].tickData.Count - 1) + "\n" +
					                "VA " + vpIntraData.barData[idx].vaPercentage + "%"
					;
					textLayout  = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, str, textFormat, 250, textFormat.FontSize);
					y = chartScale.GetYByValue(ChartBars.Bars.GetLow(idx)) + 10f;
					RenderTarget.DrawTextLayout(new Vector2(rect.X, (float) y + rect.Height/2.0f), textLayout, textFill, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
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
