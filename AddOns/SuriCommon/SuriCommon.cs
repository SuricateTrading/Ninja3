using System;
using System.Globalization;
using System.Linq;
using NinjaTrader.Data;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using TextAlignment = System.Windows.TextAlignment;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public abstract class SuriCommon {
        public static string version = "1.3.2";
        public static string mostRecentVersion = "";

        private static Random _random = new Random();
        public static int random { get { return _random.Next(1, 1000000); } }
        
        public static bool isUpToDate { get {
            return mostRecentVersion.Equals(version);
        }}
        
        public static int Week(DateTime time) {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        /** Returns true iff the given date is the end of the trading week. This is usually friday, but may be another day if there was a holiday. */
        public static bool IsEndOfTradingWeek(SessionIterator sessionIterator, DateTime date) {
            int currentWeek = Week(date);
            do {
                date = date.AddDays(1);
                if (sessionIterator.IsTradingDayDefined(date)) {
                    return currentWeek != Week(date);
                }
            } while (true);
        }
        
        
        public static void DrawText(NinjaScriptBase ninjaScriptBase, string tag, string text, int barsAgo, double y, int offset) {
            Draw.Text(ninjaScriptBase, tag + " " + random, false, text, barsAgo, y, offset, Brushes.LightGray, new SimpleFont(), TextAlignment.Center, null, null, 0);
        }
		
        public static double PriceToCurrency(Instrument instrument, double price) {
            double yDiffPrice	= Math.Abs(instrument.MasterInstrument.RoundToTickSize(price));
            double yDiffTicks	= yDiffPrice / instrument.MasterInstrument.TickSize;
            double currency		= yDiffTicks * instrument.MasterInstrument.TickSize * instrument.MasterInstrument.PointValue;
            if (instrument.MasterInstrument.InstrumentType == InstrumentType.Forex) {
                currency *= Account.All[0].ForexLotSize;
            }
            return currency;
        }


        public static void NoValidLicenseError(RenderTarget renderTarget, ChartControl chartControl, ChartPanel chartPanel) {
            SimpleFont font = new SimpleFont { Size = 15 };
            TextFormat textFormat		= font.ToDirectWriteTextFormat();
            textFormat.TextAlignment	= SharpDX.DirectWrite.TextAlignment.Leading;
            textFormat.WordWrapping		= WordWrapping.NoWrap;
            renderTarget.DrawText("Deine Lizenz ist abgelaufen oder Deine Maschinen ID hat sich verändert.", textFormat, new RectangleF(10, chartPanel.Y + 30, 100, 100), chartControl.Properties.ChartText.ToDxBrush(renderTarget));
        }
        
        
        public static readonly string dbPath = Globals.UserDataDir + @"db\suri\";

        public static Instrument GetInstrument(CommodityData commodity) {
            return Instrument.GetInstrument(commodity.shortName + Instrument.GetInstrument(commodity.shortName+" ##-##").MasterInstrument.GetNextExpiry(DateTime.Now).ToString(" MM-yy"));
        }
        public static Instrument GetInstrument(int index) {
            try {
                return GetInstrument(SuriStrings.data.ElementAt(index).Value);
            } catch (Exception) { return null; }
        }
        
        
    }
}
