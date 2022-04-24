using System;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Tools;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class SuriCotHelper {
        public List<DbCotData> dbCotData;
        public int nextIndex;
        public bool hasStarted;

        public SuriCotHelper(Instrument instrument, DateTime start, DateTime end) {
            Commodity? commodity = SuriStrings.GetComm(instrument);
            if (commodity != null && start.Year > 1900 && end.Year > 1900) {
                dbCotData = SuriCotRepo.GetCotData(commodity.Value, start, end);
            } else {
                dbCotData = new List<DbCotData>();
            }
        }
        
        /// Updates the *nextIndex* to the given dateTime which must come after or equal the current *nextIndex*.
        public int? Update(DateTime dateTime) {
            if (dbCotData.IsNullOrEmpty()) return null;
            for (int i = nextIndex; i < dbCotData.Count; i++) {
                if (dbCotData[i].Date.Date.Equals(dateTime.Date)) {
                    nextIndex = i;
                    hasStarted = true;
                    return nextIndex;
                }
                if (hasStarted && dbCotData[i].Date.Date > dateTime.Date) {
                    return nextIndex;
                }
            }
            return null;
        }

    }
}