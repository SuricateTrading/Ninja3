#region Using declarations
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using Brush = SharpDX.Direct2D1.Brush;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class SuriSettings : Indicator {
		private Brush backgroundBrush;
		private bool isPrepared;
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description									= @"Einstellungen";
				Name										= "Einstellungen";
				IsOverlay									= true;
				DrawOnPricePanel							= true;
				drawBackground								= true;
				darkOrWhiteBackground						= BackgroundColor.Dunkler;
				opacityBackground							= 0.1;
			} else if (State == State.Configure) {
				isPrepared = false;
			}
		}
		public override string DisplayName { get { return "Einstellungen"; } }

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Zeichne Hintergrund", Order=0, GroupName="Hintergrund")]
		public bool drawBackground { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Heller oder dunkler", Order=1, GroupName="Hintergrund")]
		public BackgroundColor darkOrWhiteBackground { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Deckkraft", Order=2, GroupName="Hintergrund")]
		public double opacityBackground { get; set; }
		
		// This block just hides parameters from the UI
		[NinjaScriptProperty] [Browsable(false)] [Display (Name = "Input Series", GroupName= "Data Series" )]
		public string InputUI {get; set; }
		[NinjaScriptProperty] [Browsable(false)] [Display (Name = "Calculate", GroupName= "Setup" )]
		public string Calculate {get; set; }
		[NinjaScriptProperty] [Browsable(false)] [Display (Name = "Maximum bars look back", GroupName= "Setup" )]
		public string MaximumBarsLookBack {get; set; }
		[NinjaScriptProperty] [Browsable(false)] [Display (Name = "Auto scale", GroupName= "Visual" )]
		public string IsAutoScale {get; set; }
		[NinjaScriptProperty] [Browsable(false)] [Display (Name = "Displacement", GroupName= "Visual" )]
		public string Displacement {get; set; }
		[NinjaScriptProperty] [Browsable(false)] [Display (Name = "Display in Data Box", GroupName= "Visual" )]
		public string DisplayInDataBox {get; set; }
		[NinjaScriptProperty] [Browsable(false)] [Display (Name = "Panel", GroupName= "Visual" )]
		public string Panel {get; set; }
		[NinjaScriptProperty] [Browsable(false)] [Display (Name = "Price marker(s)", GroupName= "Visual" )]
		public string PaintPriceMarkers {get; set; }
		[NinjaScriptProperty] [Browsable(false)] [Display (Name = "Scale justification", GroupName= "Visual" )]
		public string ScaleJustification {get; set; }
		[NinjaScriptProperty] [Browsable(false)] [Display (Name = "Visible", GroupName= "Visual" )]
		public string IsVisible {get; set; }
		#endregion

		private static void DrawBackground(ChartControl chartControl, ChartPanel chartPanel, RenderTarget renderTarget) {
			SuriSettings suriSettings = null;
			foreach (var indicator in chartControl.Indicators) {
				if (indicator is SuriSettings) {
					suriSettings = indicator as SuriSettings;
					break;
				}
			}
			if (suriSettings == null || !suriSettings.drawBackground) return;

			if (!suriSettings.isPrepared) {
				var brush = suriSettings.darkOrWhiteBackground == BackgroundColor.Dunkler ? Brushes.Black.Clone() : Brushes.White.Clone();
				brush.Opacity = suriSettings.opacityBackground;
				suriSettings.backgroundBrush = brush.ToDxBrush(suriSettings.RenderTarget);
			}
			
			if (chartPanel.PanelIndex % 2 == 0) return;
			RectangleF rect = new RectangleF {
				X = 0,
				Y = chartPanel.Y,
				Width = 10000f,
				Height = 10000f,
			};
			renderTarget.FillRectangle(rect, suriSettings.backgroundBrush);
		}
	}

	public enum BackgroundColor {
		Heller,
		Dunkler,
	}
}

































//
