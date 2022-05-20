using System;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Strategies;

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

        public static int? GetNextWeekIndex(int startIndex, Bars bars) {
            for (int i = startIndex + 1; i < bars.Count; i++) {
                if (bars.GetTime(i).DayOfWeek < bars.GetTime(i-1).DayOfWeek) return i;
            }
            return null;
        }

        public delegate bool BarIndexCallback(int barIndex);
        /** Returns the index of when the given value would be filled, starting from startIndex. Breaks at the end of Bars, or by an optional stopCondition. Iff stopCondition returns true, then stop searching. */
        public static int? GetIndexOfValueFill(int startIndex, Bars bars, double value, BarIndexCallback stopCondition = null) {
            for (int i = startIndex; i < bars.Count && (stopCondition == null || !stopCondition(i)); i++) {
                if (value >= bars.GetLow(i) && value <= bars.GetHigh(i)) return i;
            }
            return null;
        }

        /** Returns true iff given value is in bar range of given index, hence a limit order would be filled. Else returns false. */
        public static bool IsFilledAt(int index, double value, Bars bars) {
            return value >= bars.GetLow(index) && value <= bars.GetHigh(index);
        }

        public static SuriBarType GetBarType(Bars bars, int index, double tickSize) {
            double	prevCloseValue = bars.GetClose(index-1);
            double open = bars.GetOpen(index);
            double close = bars.GetClose(index);
            if (prevCloseValue > Math.Max(open, close) || prevCloseValue < Math.Min(open, close)) {
                open = prevCloseValue;
            }
            double high = Math.Max(bars.GetHigh(index), open);
            double low = Math.Min(bars.GetLow(index), open);
            double barSize = high - low + tickSize;
            double body = Math.Abs(open - close) + tickSize;
            double upperWick = high - Math.Max(open, close);
            double bottomWick = Math.Min(open, close) - low;

            double perc = 0.15; // => reversal bar percentage
            if (body / barSize > perc) return close > open ? SuriBarType.MegabarUp : SuriBarType.MegabarDown;
            if (upperWick  / body >= 0.5) return SuriBarType.ReversalBarBottom;
            if (bottomWick / body >= 0.5) return SuriBarType.ReversalBarTop;
            return upperWick > bottomWick ? SuriBarType.ReversalBarMiddleBottom : SuriBarType.ReversalBarMiddleTop;
        }

        public static bool BarGoesUp(Bars bars, int index) { return bars.GetClose(index) > bars.GetOpen(index); }
    }
}
