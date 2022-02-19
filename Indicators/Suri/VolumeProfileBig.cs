#region Using declarations

using System;
using NinjaTrader.Gui.Chart;
using System.Windows.Media;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using NinjaTrader.Gui.NinjaScript.Indicators;
using NinjaTrader.Gui.Tools;
using SharpDX;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class VolumeProfileBig : Indicator {
		private VpBigData vpBigData = new VpBigData();
		
		#region Properties
		private bool prepared;
		private SharpDX.Direct2D1.Brush normalAreaFill;
		private SharpDX.Direct2D1.Brush pocFill;
		private SharpDX.Direct2D1.Brush vaFill;
		private SharpDX.Direct2D1.Brush textFill;
		private SharpDX.DirectWrite.TextFormat textFormat;
		
		[Display(Name = "Breite", Order = 0, GroupName = "Parameter", Description = "Wenn leer, dann wird die Breite automatisch berechnet. Ansonsten wird es maximal so breit.")]
		public int? maxWidth { get; set; }
		
		[Display(Name = "Entfernung zur Bar", Order = 0, GroupName = "Parameter", Description = "Wie weit es von der Mitte der Bar entfernt sein soll.")]
		public int offset { get; set; }
		
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
				
				offset										= 0;
				maxWidth									= null;
				drawText									= true;
				drawNakedPoc								= true;
				valueAreaBrush								= Brushes.CornflowerBlue.Clone();
				valueAreaBrush.Opacity						= 0.5;
				normalAreaBrush								= Brushes.Gray.Clone();
				normalAreaBrush.Opacity						= 0.5;
				pocBrush									= Brushes.Orange.Clone();
				pocBrush.Opacity							= 0.5;
				textBrush									= Brushes.White;
			} else if (State == State.Configure) {
				if (Bars.IsTickReplay) {
					AddDataSeries(null, new BarsPeriod { BarsPeriodType = BarsPeriodType.Day, Value = 1 }, 255, null, null);
				} else {
					AddDataSeries(null, new BarsPeriod { BarsPeriodType = BarsPeriodType.Minute, Value = 1 }, 150000, Instrument.MasterInstrument.TradingHours.Name, null);
				}
				prepared = false;
			}
		}

		protected override void OnBarUpdate() {
			if (!Bars.IsTickReplay && Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && Bars.BarsPeriod.Value == 1) {
				vpBigData.AddMinuteVolume((long) Volumes[1][0], Opens[1][0], Highs[1][0], Lows[1][0], Closes[1][0], TickSize, Print);
				//Print("OnBarUpdate\t" + Lows[0][0] + "\t" + Highs[0][0] + "\t" + Volumes[0][0] + "\t" + Times[0][0] + "\t" + Bars.BarsPeriod.BarsPeriodType + "\t" + Bars.BarsPeriod.Value);
				//Print(Times[1][0]);
			}
		}

		private bool test = false;
		protected override void OnMarketData(MarketDataEventArgs e) {
			if (Bars.Count > 0 && Bars.IsTickReplay && Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Day && Bars.BarsPeriod.Value == 1) {
				if (!test) {
					test = true;
					Print("OnMarketData\t" + e.Price + "\t" + e.Volume + "\t" + e.Time + "\t" + Bars.BarsPeriod.BarsPeriodType);
				}
				vpBigData.AddTick(e);
				
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (Bars == null || Bars.Instrument == null || IsInHitTest) return;
			base.OnRender(chartControl, chartScale);
			if (!vpBigData.isPrepared) vpBigData.Prepare(chartScale, TickSize, Instrument, Print);

			if (!prepared) {
				prepared = true;
				normalAreaFill = normalAreaBrush.ToDxBrush(RenderTarget);
				pocFill = pocBrush.ToDxBrush(RenderTarget);
				vaFill = valueAreaBrush.ToDxBrush(RenderTarget);
				textFill = textBrush.ToDxBrush(RenderTarget);
			}
			
			double highestValue = chartScale.MaxValue - chartScale.MaxValue % TickSize;
			double lowestValue = chartScale.MinValue + TickSize - chartScale.MinValue % TickSize;
			double tickCount = (highestValue - lowestValue) / TickSize;
			
			RectangleF rect = new RectangleF {
				X = 0,
				Height = (chartScale.GetYByValue(0) - chartScale.GetYByValue(1000 * TickSize)) / 1000f,
			};
			if (highestValue > vpBigData.tickData.Last().Key) {
				rect.Y = chartScale.GetYByValue(vpBigData.tickData.Last().Key + TickSize) - rect.Height / 2f;
			} else {
				rect.Y = chartScale.GetYByValue(highestValue + TickSize) - rect.Height / 2f;
			}
			
			if (rect.Height > 13) {
				SimpleFont font = new SimpleFont {Size = rect.Height * 0.53};
				textFormat = font.ToDirectWriteTextFormat();
				textFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
				textFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
			}

			for (int i = 0; i < tickCount; i++) {
				double price = highestValue - i * TickSize;
				if (vpBigData.tickData.ContainsKey(price)) {
					VpTickData tick = vpBigData.tickData[highestValue - i * TickSize];

					rect.Y += rect.Height;
					
					rect.Width = (float) ((maxWidth ?? ChartPanel.W * 0.5) * tick.volume / vpBigData.pocVolume);

					if (rect.Height > 13) {
						SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, tick.volume.ToString("F0"), textFormat, 250, textFormat.FontSize);
						RenderTarget.DrawTextLayout(new Vector2(0, rect.Y), textLayout, textFill, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					}
					RenderTarget.FillRectangle(rect, tick.isMainPoc ? pocFill : normalAreaFill);
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
		private Suri.VolumeProfileBig[] cacheVolumeProfileBig;
		public Suri.VolumeProfileBig VolumeProfileBig()
		{
			return VolumeProfileBig(Input);
		}

		public Suri.VolumeProfileBig VolumeProfileBig(ISeries<double> input)
		{
			if (cacheVolumeProfileBig != null)
				for (int idx = 0; idx < cacheVolumeProfileBig.Length; idx++)
					if (cacheVolumeProfileBig[idx] != null &&  cacheVolumeProfileBig[idx].EqualsInput(input))
						return cacheVolumeProfileBig[idx];
			return CacheIndicator<Suri.VolumeProfileBig>(new Suri.VolumeProfileBig(), input, ref cacheVolumeProfileBig);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.VolumeProfileBig VolumeProfileBig()
		{
			return indicator.VolumeProfileBig(Input);
		}

		public Indicators.Suri.VolumeProfileBig VolumeProfileBig(ISeries<double> input )
		{
			return indicator.VolumeProfileBig(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.VolumeProfileBig VolumeProfileBig()
		{
			return indicator.VolumeProfileBig(Input);
		}

		public Indicators.Suri.VolumeProfileBig VolumeProfileBig(ISeries<double> input )
		{
			return indicator.VolumeProfileBig(input);
		}
	}
}

#endregion
