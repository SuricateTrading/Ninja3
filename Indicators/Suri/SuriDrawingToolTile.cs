#region Using declarations
using System;
using System.Xml.Linq;
using NinjaTrader.NinjaScript.DrawingTools;
using Line = NinjaTrader.Gui.Line;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
    public class SuriDrawingToolTile : DrawingToolTile {
        protected override void OnStateChange() {
            base.OnStateChange();
            if (State == State.SetDefaults) {
                Name							= "Zeichenwerkzeuge";
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
