using System;

namespace NinjaTrader.Custom.AddOns.SuriData {
    public static class DataTools {
        
        /// <param name="index">The index where to start calculating.</param>
        /// <param name="getValue">Returns the value of given index.</param>
        /// <param name="stop">Wether the calculation should stop after the given index.</param>
        /// <returns>Calculated Osci value. Returns null iff a value with getValue could not be returned before stop-function was triggered.</returns>
        public static double GetOsci(int index, Func<int, double> getValue, Func<int, bool> stop) {
            var min = double.MaxValue;
            var max = double.MinValue;
            for (int i = index; i >= 0; i--) {
                var v = getValue(i);
                if (min > v) min = v;
                if (max < v) max = v;
                if (stop(i)) break;
            }
            return 100.0 * (getValue(index) - min) / (max - min);
        }

        /// <returns>Returns a tuple containing the last min and max index. May be null if given index is zero.</returns>
        public static Tuple<int, int> GetMinMax(int index, int? lastMinIndex, int? lastMaxIndex, int years, Func<int, double> getValue, Func<int, DateTime> getDate) {
            double currentValue = getValue(index);
            DateTime currentDate = getDate(index);

            if (lastMinIndex == null || lastMaxIndex == null ||
                Math.Abs((getDate(lastMinIndex.Value) - currentDate).Days / 365.0) >= years ||
                Math.Abs((getDate(lastMaxIndex.Value) - currentDate).Days / 365.0) >= years
            ) {
                // the last max or min is too far away. Recalculate.
                double min = double.MaxValue;
                double max = double.MinValue;
                for (int i = index; i >= 0; i--) {
                    double value = getValue(i);
                    if (min > value) { min = value; lastMinIndex = i; }
                    if (max < value) { max = value; lastMaxIndex = i; }
                    if (Math.Abs((getDate(i) - currentDate).Days / 365.0) >= years) break;
                }
            } else {
                if (getValue(lastMinIndex.Value) > currentValue) { lastMinIndex = index; }
                if (getValue(lastMaxIndex.Value) < currentValue) { lastMaxIndex = index; }
            }

            if (lastMinIndex == null || lastMaxIndex == null) return null;
            return new Tuple<int, int>(lastMinIndex.Value, lastMaxIndex.Value);
        }
        
        
        
    }
}
