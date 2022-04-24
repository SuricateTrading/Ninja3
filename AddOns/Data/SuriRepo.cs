#region Using declarations
using System;
using System.Globalization;
using System.Linq;
using NinjaTrader.Core;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon.Vp {
    public class SuriRepo {
        public static readonly string dbPath = Globals.UserDataDir + @"db\suri\";

        public static Instrument GetInstrument(CommodityData commodity) {
            return Instrument.GetInstrument(commodity.shortName + Instrument.GetInstrument(commodity.shortName+" ##-##").MasterInstrument.GetNextExpiry(DateTime.Now).ToString(" MM-yy"));
        }
        public static Instrument GetInstrument(int index) {
            try {
                return GetInstrument(SuriStrings.data.ElementAt(index).Value);
            } catch (Exception) { return null; }
        }
        
        public static int Week(DateTime time) {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }
        
    }
}