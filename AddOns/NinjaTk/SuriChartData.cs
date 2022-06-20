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
        public double settle = 0.0;
        public double last = 0.0;
        public long volume = 0;
        public long openInterest = 0;
    }
}
