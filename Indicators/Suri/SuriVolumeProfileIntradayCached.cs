#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Gui.Chart;
using SharpDX;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Xml.Serialization;
using NinjaTrader.Core;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui.NinjaScript;
using Brush = System.Windows.Media.Brush;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public sealed class SuriVolumeProfileIntradayCached : Indicator {
		private readonly VpIntraData vpIntraData = new VpIntraData();
		private static readonly string dbPath = Globals.UserDataDir + @"db\suri\";
		private bool ready;
		
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
				Name										= "Volume Profile - Intraday Cached";
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
				ready = false;
				SimpleFont font = new SimpleFont { Size = textSize };
				textFormat					= font.ToDirectWriteTextFormat();
				textFormat.TextAlignment	= SharpDX.DirectWrite.TextAlignment.Leading;
				textFormat.WordWrapping		= SharpDX.DirectWrite.WordWrapping.NoWrap;
			} else if (State == State.DataLoaded) {
				Load();
			}
		}

		private void Load() {
			
		}
/*
		private void Load2() {
			ready = false;
			try {
				string[] lines = File.ReadAllLines(dbPath + @"\" + Instrument.MasterInstrument.Name + ".vpintra");

				DateTime firstBarDate = ChartBars.Bars.GetTime(0).Date;
				int startIndex = -1;
				for (int i = 0; i < lines.Length; i++) {
					string line = lines[i];
					if (line.IsNullOrEmpty()) continue;
					DateTime date = DateTime.Parse(line.Substring(0, 10)).Date;
					if (startIndex == -1) {
						if (date != firstBarDate) continue;
						startIndex = i;
					}

					int index = i - startIndex;
					if (index < 0) break;
					double low  = ChartBars.Bars.GetLow (index);
					vpIntraData.barData.Add(new VpBarData(TickSize, date.Date));
					string[] tabs = line.Split('\t');
					for (int j = 1; j < tabs.Length; j++) {
						if (tabs[j].IsNullOrEmpty()) continue;
						vpIntraData.barData.Last().AddCached(low + TickSize * (j-1), long.Parse(tabs[j]));
					}
					if (i-startIndex+1 >= ChartBars.Bars.Count) break;
				}
				vpIntraData.Prepare();
				ready = true;
			} catch (Exception e) {
				Print(e);
			}
		}
*/
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (!ready || SuriAddOn.license == License.None || Bars == null || Bars.Instrument == null || IsInHitTest) {
				return;
			}
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
					if (entry.Value.isLvn)				b = testing1Fill;
					else if (entry.Value.isHigh)		b = testing2Fill;
					else if (entry.Value.isMainPoc)		b = pocFill;
					else if (entry.Value.isInValueArea)	b = vaFill;
					else								b = normalAreaFill;
					RenderTarget.FillRectangle(rect, b);

					if (entry.Value.isLvn) {
						RenderTarget.DrawLine(
							new Vector2(rect.X + rect.Width + 10 + 100, rect.Y + rect.Height/2.0f),
							new Vector2(rect.X + rect.Width + 10 + 200, rect.Y + rect.Height/2.0f),
							testing1Fill, 1.5f
						);
					}

					// Draw SMA
					float distVolWidth = (float) ((maxWidth ?? barWidth) * entry.Value.distributedVolume / vpIntraData.barData[idx].pocVolume);
					if (previousDistVolWidth != null) {
						RenderTarget.DrawLine(
							new Vector2(rect.X + previousDistVolWidth.Value, rect.Y + rect.Height + rect.Height / 2f),
							new Vector2(rect.X + distVolWidth, rect.Y + rect.Height / 2f),
							smaFill
						);
					}
					previousDistVolWidth = distVolWidth;
					
					
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
							textLayout  = new SharpDX.DirectWrite.TextLayout(Globals.DirectWriteFactory, vpocText, textFormat, 250, textFormat.FontSize);
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
					textLayout  = new SharpDX.DirectWrite.TextLayout(Globals.DirectWriteFactory, str, textFormat, 250, textFormat.FontSize);
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
		private Suri.dev.SuriVolumeProfileIntradayCached[] cacheSuriVolumeProfileIntradayCached;
		public Suri.dev.SuriVolumeProfileIntradayCached SuriVolumeProfileIntradayCached()
		{
			return SuriVolumeProfileIntradayCached(Input);
		}

		public Suri.dev.SuriVolumeProfileIntradayCached SuriVolumeProfileIntradayCached(ISeries<double> input)
		{
			if (cacheSuriVolumeProfileIntradayCached != null)
				for (int idx = 0; idx < cacheSuriVolumeProfileIntradayCached.Length; idx++)
					if (cacheSuriVolumeProfileIntradayCached[idx] != null &&  cacheSuriVolumeProfileIntradayCached[idx].EqualsInput(input))
						return cacheSuriVolumeProfileIntradayCached[idx];
			return CacheIndicator<Suri.dev.SuriVolumeProfileIntradayCached>(new Suri.dev.SuriVolumeProfileIntradayCached(), input, ref cacheSuriVolumeProfileIntradayCached);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.SuriVolumeProfileIntradayCached SuriVolumeProfileIntradayCached()
		{
			return indicator.SuriVolumeProfileIntradayCached(Input);
		}

		public Indicators.Suri.dev.SuriVolumeProfileIntradayCached SuriVolumeProfileIntradayCached(ISeries<double> input )
		{
			return indicator.SuriVolumeProfileIntradayCached(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.SuriVolumeProfileIntradayCached SuriVolumeProfileIntradayCached()
		{
			return indicator.SuriVolumeProfileIntradayCached(Input);
		}

		public Indicators.Suri.dev.SuriVolumeProfileIntradayCached SuriVolumeProfileIntradayCached(ISeries<double> input )
		{
			return indicator.SuriVolumeProfileIntradayCached(input);
		}
	}
}

#endregion
