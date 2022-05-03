#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows.Data;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
using Line = NinjaTrader.Gui.Line;
#endregion


namespace NinjaTrader.NinjaScript.Indicators.Suri {
    public class SuriDrawingToolTile2 : DrawingToolTile {

        protected override void OnStateChange() {
            base.OnStateChange();
            if (State == State.SetDefaults) {
                Name							= "Zeichenwerkzeuge2";
                Description						= "";
                IsOverlay						= true;
                IsChartOnly						= true;
                DisplayInDataBox				= false;
                PaintPriceMarkers				= false;
                IsSuspendedWhileInactive		= true;
                SelectedTypes					= new XElement("SelectedTypes");
                foreach (Type type in new[] {
                             typeof(Line), typeof(PathTool), typeof(HorizontalLine),
                             typeof(VerticalLine), typeof(Rectangle), typeof(Text),
                             typeof(RectangleS), typeof(RectangleBarInfo), typeof(SuriRuler),
                }) {
                    if (type.FullName == null) continue;
                    XElement el = new XElement(type.FullName);
                    el.Add(new XAttribute("Assembly", "NinjaTrader.Custom"));
                    SelectedTypes.Add(el);
                }
                Left			= 400;
                Top				= 0;
                NumberOfRows	= 1;
            }
        }
    }
}
