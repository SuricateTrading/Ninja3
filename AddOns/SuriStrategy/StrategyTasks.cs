using System;
using NinjaTrader.Data;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public static class StrategyTasks {

        public  static double GetWeekHigh     (int barIndex, Bars bars) { return GetWeekHighOrLow(barIndex, bars, true ); }
        public  static double GetWeekLow      (int barIndex, Bars bars) { return GetWeekHighOrLow(barIndex, bars, false); }
        private static double GetWeekHighOrLow(int barIndex, Bars bars, bool returnHigh) {
            double weekHigh = double.MinValue;
            double weekLow  = double.MaxValue;
            for (int i = barIndex; i >= 0; i--) {
                if (weekHigh < bars.GetHigh(i)) weekHigh = bars.GetHigh(i);
                if (weekLow  > bars.GetLow (i)) weekLow  = bars.GetLow (i);
                if (bars.GetTime(i).DayOfWeek == DayOfWeek.Monday || i > 0 && bars.GetTime(i).DayOfWeek < bars.GetTime(i-1).DayOfWeek ) break;
            }
            for (int i = barIndex; i < bars.Count; i++) {
                if (weekHigh < bars.GetHigh(i)) weekHigh = bars.GetHigh(i);
                if (weekLow  > bars.GetLow (i)) weekLow  = bars.GetLow (i);
                if (bars.GetTime(i).DayOfWeek == DayOfWeek.Friday || i < bars.Count-1 && bars.GetTime(i).DayOfWeek > bars.GetTime(i+1).DayOfWeek ) break;
            }
            return returnHigh ? weekHigh : weekLow;
        }

        public  static double GetLast10DaysHigh   (int barIndex, Bars bars) { return GetLastDaysHighOrLow(barIndex, bars, true ); }
        public  static double GetLast10DaysLow    (int barIndex, Bars bars) { return GetLastDaysHighOrLow(barIndex, bars, false); }
        private static double GetLastDaysHighOrLow(int barIndex, Bars bars, bool returnHigh, int days = 10) {
            double stopValue = returnHigh ? double.MinValue : double.MaxValue;
            for (int i = 0; i < days && barIndex - i >= 0; i++) {
                if (returnHigh  && stopValue < bars.GetHigh(barIndex - i)) stopValue = bars.GetHigh(barIndex - i);
                if (!returnHigh && stopValue > bars.GetLow(barIndex - i))  stopValue = bars.GetLow (barIndex - i);
            }
            return stopValue;
        }

        public delegate bool BarIndexCallback(int barIndex);
        /** Returns the index of when the given value would be filled, starting from startIndex. Breaks at the end of Bars, or by an optional stopCondition. Iff stopCondition returns true, then stop searching. */
        public static int? GetIndexOfValueFill(int startIndex, Bars bars, double value, BarIndexCallback stopCondition = null) {
            for (int i = startIndex; i < bars.Count && (stopCondition == null || !stopCondition(i)); i++) {
                if (value > bars.GetLow(i) && value < bars.GetHigh(i)) return i;
            }
            return null;
        }
        
        
        public static bool BarAction(int startIndex, Bars bars, BarIndexCallback callback) {
            for (int i = startIndex; i < bars.Count; i++) {
                if (callback(i)) return true;
            }
            return false;
        }
        
    }
}