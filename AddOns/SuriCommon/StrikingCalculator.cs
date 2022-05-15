using NinjaTrader.Gui.Chart;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public sealed class StrikingCalculator {
        private readonly ChartScale chartScale;
        private readonly int currentBar;
        
        private double High(int barsAgo) { return chartScale.GetFirstChartBars().Bars.GetHigh(currentBar - barsAgo); }
        private double Low(int barsAgo)  { return chartScale.GetFirstChartBars().Bars.GetLow(currentBar - barsAgo); }
        public double Close(int barsAgo)  { return chartScale.GetFirstChartBars().Bars.GetClose(currentBar - barsAgo); }
        
        public StrikingCalculator(ChartScale chartScale) {
            this.chartScale = chartScale;
            currentBar = chartScale.GetFirstChartBars().Bars.Count - 1;
        }
        
        // todo: es sollten mindestens 7-10 Bars zwischen altem und neuem Tief (Hoch) sein. Adi: "ich schaue mir keine kleinen Trendbewegungen an sondern nur Große Trends."

        public StrikingSpotData FindStrikingLow(int barsAgo, int? initialBarsAgo = null) {
            if (initialBarsAgo == null) initialBarsAgo = barsAgo;
            StrikingSpotData s = new StrikingSpotData();

            s.Set3(barsAgo, double.MinValue);
            for (int i = 0; i < 5; i++) {
                if (High(barsAgo) > s.p3Value) {
                    s.p3Value = High(barsAgo);
                    s.p3Bar = barsAgo;
                    i = 0;
                }
                barsAgo++;
            }
            barsAgo = s.p3Bar + 1;
            
            for (int i = 0; i < 5; i++) {
                if (i == 0) {
                    s.Set1(barsAgo, High(barsAgo));
                }
                
                if (High(barsAgo) > s.p1Value) {
                    s.p1Value = High(barsAgo);
                    s.p1Bar = barsAgo;
                    i=0;
                }
                barsAgo++;

                if (i==4) {
                    int belowCounter = 0;
                    //int barsToCheck = (s.p1Bar - s.p3Bar); 
                    for (int j = s.p1Bar+1; j > s.p3Bar; j--) {
                        if (High(j) < s.p1Value) {
                            belowCounter++;
                        }
                    }
                    if (belowCounter <= 3 /* || belowCounter / (double)(s.p1Bar - s.p3Bar) < 0.5*/) {
                        // less than 50% of the bars are lower
                        i = 0;
                        barsAgo -= 4;
                        s.p1Value = double.MinValue;
                    }
                }

                if (i == 4 && s.p1Value > s.p3Value) {
                    return FindStrikingLow(s.p1Bar, initialBarsAgo);
                }
            }
            
            s.p2Value = double.MaxValue;
            for (int i = s.p3Bar; i < s.p1Bar; i++) {
                if (Low(i) < s.p2Value) {
                    s.Set2(i, Low(i));
                }
            }
            if (s.p2Value > Low(initialBarsAgo.Value)) {
                return FindStrikingLow(s.p1Bar, initialBarsAgo);
            }
            
            return s;
        }
        
        public StrikingSpotData FindStrikingHigh(int barsAgo, int? initialBarsAgo = null) {
            if (initialBarsAgo == null) initialBarsAgo = barsAgo;
            StrikingSpotData s = new StrikingSpotData();

            s.Set3(barsAgo, double.MaxValue);
            for (int i = 0; i < 5; i++) {
                if (Low(barsAgo) < s.p3Value) {
                    s.p3Value = Low(barsAgo);
                    s.p3Bar = barsAgo;
                    i = 0;
                }
                barsAgo++;
            }
            barsAgo = s.p3Bar + 1;
            
            for (int i = 0; i < 5; i++) {
                if (i == 0) {
                    s.Set1(barsAgo, Low(barsAgo));
                }
                
                if (Low(barsAgo) < s.p1Value) {
                    s.p1Value = Low(barsAgo);
                    s.p1Bar = barsAgo;
                    i=0;
                }
                barsAgo++;

                if (i==4) {
                    int belowCounter = 0;
                    //int barsToCheck = (s.p1Bar - s.p3Bar); 
                    for (int j = s.p1Bar+1; j > s.p3Bar; j--) {
                        if (Low(j) > s.p1Value) {
                            belowCounter++;
                        }
                    }
                    if (belowCounter <= 3 /* || belowCounter / (double)(s.p1Bar - s.p3Bar) < 0.5*/) {
                        // less than 50% of the bars are lower
                        i = 0;
                        barsAgo -= 4;
                        s.p1Value = double.MaxValue;
                    }
                }

                if (i == 4 && s.p1Value < s.p3Value) {
                    return FindStrikingHigh(s.p1Bar, initialBarsAgo);
                }
            }
            
            s.p2Value = double.MinValue;
            for (int i = s.p3Bar; i < s.p1Bar; i++) {
                if (High(i) > s.p2Value) {
                    s.Set2(i, High(i));
                }
            }
            if (s.p2Value < High(initialBarsAgo.Value)) {
                return FindStrikingHigh(s.p1Bar, initialBarsAgo);
            }
            
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
