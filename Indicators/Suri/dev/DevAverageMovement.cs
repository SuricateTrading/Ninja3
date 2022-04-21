#region Using declarations

using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class AverageMovement : Indicator {
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tage", Order=1, GroupName="Parameter")]
		public int days
		{ get; set; }

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Name						= "Average Movement";
				Description					= "Berechnet die durchschnittliche Bewegung der letzten X Bars.";
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

				days										= 30;
			} else if (State == State.Configure) {
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, Name);
			}
		}
		
		protected override void OnBarUpdate() {
			if (CurrentBar <= days) return;
			Value[0] = 0;
			for (int barsAgo = 0; barsAgo < days-1; barsAgo++) {
				Value[0] += Math.Abs(Close[barsAgo+1] - Close[barsAgo]);
			}
			Value[0] /= days;
		}

	}
}
