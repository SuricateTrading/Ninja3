using System.Collections.Generic;

namespace NinjaTrader.Gui.NinjaScript {
    public class SuriChartData {
        public int commId;
        public string date;
        public List<SuriChartMonth> months;
    }
    public class SuriChartMonth {
        public int monthValue;
        public int year;
        public double settle;
        public double last;
        public long volume;
        public long openInterest;
    }
}
