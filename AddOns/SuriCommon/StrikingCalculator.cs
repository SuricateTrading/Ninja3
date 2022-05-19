using NinjaTrader.Data;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public static class StrikingCalculator {
        // todo: es sollten mindestens 7-10 Bars zwischen altem und neuem Tief (Hoch) sein. Adi: "ich schaue mir keine kleinen Trendbewegungen an sondern nur Große Trends."

        private static double Get(bool high, int i, Bars bars) { return high ? bars.GetHigh(i) : bars.GetLow(i); }
        
        public static StrikingSpotData FindStrikingSpot(bool high, Bars bars, int index, int? initialIndex = null) {
            if (initialIndex == null) initialIndex = index;
            StrikingSpotData s = new StrikingSpotData();
            double value;

            s.Set3(index, high ? double.MaxValue : double.MinValue);
            for (int i = 0; i < 5; i++) {
                value = Get(!high, index, bars);
                if (high ? value < s.p3Value : value > s.p3Value) {
                    s.p3Value = value;
                    s.p3Bar = index;
                    i = 0;
                }
                index--;
            }
            index = s.p3Bar - 1;
            
            for (int i = 0; i < 5; i++) {
                value = Get(!high, index, bars);
                if (i == 0) s.Set1(index, value);
                
                if (high ? value < s.p1Value : value > s.p1Value) {
                    s.p1Value = value;
                    s.p1Bar = index;
                    i=0;
                }
                index--;

                if (i==4) {
                    /* explenation for searching a striking low:
                        1. find the highest local high (p1)
                        2. check if there are enough bars between p1 and p3 which have a lower high than p1
                        3. if not enough bars, then reset search and continue, starting with p1
                    */ 
                    int belowCounter = 0;
                    //int barsToCheck = (s.p1Bar - s.p3Bar); 
                    for (int j = s.p1Bar+1; j < s.p3Bar; j++) {
                        value = Get(!high, j, bars);
                        if (high ? value > s.p1Value : value < s.p1Value) {
                            belowCounter++;
                        }
                    }
                    if (belowCounter <= 3 /* || belowCounter / (double)(s.p1Bar - s.p3Bar) < 0.5*/) {
                        // less than 50% of the bars are lower
                        i = 0;
                        index += 4;
                        s.p1Value = high ? double.MaxValue : double.MinValue;
                    }
                }

                if (i == 4 && (high ? s.p1Value < s.p3Value : s.p1Value > s.p3Value)) {
                    return FindStrikingSpot(high, bars, s.p1Bar, initialIndex);
                }
            }
            
            s.p2Value = high ? double.MinValue : double.MaxValue;
            for (int i = s.p1Bar + 1; i <= s.p3Bar; i++) {
                value = Get(high, i, bars);
                if (high ? value > s.p2Value : value < s.p2Value) {
                    s.Set2(i, value);
                }
            }
            value = Get(high, initialIndex.Value, bars);
            if (high ? s.p2Value < value : s.p2Value > value) {
                return FindStrikingSpot(high, bars, s.p1Bar, initialIndex);
            }
            
            /*if (s.p1Bar == s.p2Bar) {
                // special case: iff p1 and p2 is the same bar and p1 has neighboured lower lows, we have to search again
                for (int i = 1; i < 4; i++) {
                    value = Get(!high, index - i, bars);
                    if (high ? value > s.p1Value : value < s.p1Value) {
                        return FindStrikingSpot(high, bars, s.p1Bar, initialIndex);
                    }
                }
            }*/

            return s;
        }
    }
}

public sealed class StrikingSpotData {
    public int p1Bar;
    public int p2Bar;
    public int p3Bar;
    
    public double p1Value;
    public double p2Value;
    public double p3Value;

    public void Set1(int barsAgo, double value) { p1Bar = barsAgo; p1Value = value; }
    public void Set2(int barsAgo, double value) { p2Bar = barsAgo; p2Value = value; }
    public void Set3(int barsAgo, double value) { p3Bar = barsAgo; p3Value = value; }
}
