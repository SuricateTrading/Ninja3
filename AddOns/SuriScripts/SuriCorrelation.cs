#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.NinjaScript;
using System.IO;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using NinjaTrader.Data;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class DevCorrelation {
	    private static readonly string filePath = Globals.UserDataDir + @"mining\";
	    private static readonly string fileName = filePath + @"correlation.txt";
	    private readonly List<Commodity> commodities = Enum.GetValues(typeof(Commodity)).Cast<Commodity>().ToList();
	    private readonly Dictionary<Commodity, BarsRequest> data = new Dictionary<Commodity, BarsRequest>();
	    private readonly Dictionary<Commodity, double> medians = new Dictionary<Commodity, double>();
	    private readonly Dictionary<Commodity, List<double>> sortedCloses = new Dictionary<Commodity, List<double>>();

	    private StreamWriter correlationCoefficientWriter;

	    public void LoadData(int index = 0) {
		    if (index == 0) Code.Output.Process("Start Loading", PrintTo.OutputTab1);
		    if (index >= commodities.Count) {
			    Calculate();
			    return;
		    }
		    Commodity commodity = commodities[index];
		    try {
			    Instrument instrument = SuriRepo.GetInstrument(SuriStrings.data[commodity]);
			    DateTime from = DateTime.Now.AddYears(-3).Date;
			    DateTime to = DateTime.Now.AddDays(-1).Date;
			    //Code.Output.Process("Loading " + commodity + " from " + from + " to " + to, PrintTo.OutputTab1);
			    
			    new BarsRequest(instrument, from, to) {
				    MergePolicy = MergePolicy.MergeBackAdjusted,
				    BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Minute, Value = 1440},
				    TradingHours = instrument.MasterInstrument.TradingHours,
			    }.Request((bars, errorCode, errorMessage) => {
				    if (errorCode != ErrorCode.NoError) {
					    Code.Output.Process("Error: " + errorCode, PrintTo.OutputTab1);
					    return;
				    }
				    data.Add(commodity, bars);
				    LoadData(++index);
			    });
		    } catch (Exception e) {
			    Code.Output.Process("Error in " + commodity + " " + e, PrintTo.OutputTab1);
		    }
	    }

	    private void Calculate() {
		    Code.Output.Process("Start calculation", PrintTo.OutputTab1);
		    Directory.CreateDirectory(filePath);
		    correlationCoefficientWriter = File.CreateText(fileName);
		    
		    // write header
		    WriteAll("Commodity\t");
		    foreach (var pair1 in data) WriteAll(pair1.Key + "\t");

		    foreach (var pair1 in data) {
			    List<double> closeValues = new List<double>();
			    for (int i = 0; i < pair1.Value.Bars.Count; i++) {
				    closeValues.Add(pair1.Value.Bars.GetClose(i));
			    }
			    closeValues.Sort();
			    sortedCloses.Add(pair1.Key, closeValues);
			    double median = closeValues[(closeValues.Count + 1) / 2];
			    if (closeValues.Count % 2 == 0) median = (median + closeValues[closeValues.Count / 2]) / 2;
			    medians.Add(pair1.Key, median);
		    }

		    foreach (var pair1 in data) {
			    WriteAll("\n" + pair1.Key + "\t");
			    foreach (var pair2 in data) {
				    Bars bars1 = pair1.Value.Bars;
				    Bars bars2 = pair2.Value.Bars;
				    if (pair1.Key == pair2.Key) {
					    WriteAll("100\t");
					    continue;
				    }

				    int matchingEntries = 0;
				    Tuple<int, int> t = new Tuple<int, int>(-1, -1);
				    while ((t = SynchronizeIndex(new Tuple<int, int>(t.Item1 + 1, t.Item2 + 1), bars1, bars2)) != null) {
					    matchingEntries++;
				    }
				    if (matchingEntries < bars1.Count * 0.75) {
					    Code.Output.Process("ERROR zu viel fehlende Tage bei: " + pair1.Key + " " + pair2.Key + " " + (100 * matchingEntries / ((double)bars1.Count)), PrintTo.OutputTab1);
					    WriteAll("\t");
					    continue;
				    }

				    //double corrCo = 100 * CorrelationCoefficient(bars1, bars2, matchingEntries);
				    //correlationCoefficientWriter.Write(corrCo.ToString("F2") + "\t");
				    
				    double spearman = 100 * SpearmanCorrelation(pair1.Key, pair2.Key, bars1, bars2, matchingEntries);
				    correlationCoefficientWriter.Write(spearman.ToString("F2") + "\t");
			    }
		    }
		    
		    correlationCoefficientWriter.Close();
		    Code.Output.Process("End calculation", PrintTo.OutputTab1);
	    }

	    private void WriteAll(string text) {
		    correlationCoefficientWriter.Write(text);
	    }

	    /** Synchronizes the index of 2 Bars so that the index for each bar points to the exact same date. */
	    private Tuple<int, int> SynchronizeIndex(Tuple<int, int> index, Bars bars1, Bars bars2) {
		    if (index.Item1 >= bars1.Count || index.Item2 >= bars2.Count) return null;
		    if (bars1.GetTime(index.Item1).Date == bars2.GetTime(index.Item2).Date) return index;

		    int i1 = index.Item1;
		    int i2 = index.Item2;
		    while (bars1.GetTime(i1).Date != bars2.GetTime(i2).Date) {
			    while (bars1.GetTime(i1).Date < bars2.GetTime(i2).Date) {
				    i1++;
				    if (i1 >= bars1.Count) return null;
			    }
			    while (bars1.GetTime(i1).Date > bars2.GetTime(i2).Date) {
				    i2++;
				    if (i2 >= bars2.Count) return null;
			    }
		    }
		    return new Tuple<int, int>(i1, i2);
	    }
	    
	    private double CorrelationCoefficient(Bars bars1, Bars bars2, int n) {
		    Tuple<int, int> t;

		    t = new Tuple<int, int>(-1, -1);
		    double average1 = 0.0;
		    double average2 = 0.0;
		    while ((t = SynchronizeIndex(new Tuple<int, int>(t.Item1 + 1, t.Item2 + 1), bars1, bars2)) != null) {
			    average1 += bars1.GetClose(t.Item1);
			    average2 += bars2.GetClose(t.Item2);
		    }
		    average1 /= n;
		    average2 /= n;

		    t = new Tuple<int, int>(-1, -1);
		    double standardDeviation1 = 0.0;
		    double standardDeviation2 = 0.0;
		    while ((t = SynchronizeIndex(new Tuple<int, int>(t.Item1 + 1, t.Item2 + 1), bars1, bars2)) != null) {
			    standardDeviation1 += Math.Pow(bars1.GetClose(t.Item1) - average1, 2);
			    standardDeviation2 += Math.Pow(bars2.GetClose(t.Item2) - average2, 2);
		    }
		    standardDeviation1 = Math.Sqrt(standardDeviation1 / n);
		    standardDeviation2 = Math.Sqrt(standardDeviation2 / n);

		    t = new Tuple<int, int>(-1, -1);
		    double covariance = 0.0;
		    while ((t = SynchronizeIndex(new Tuple<int, int>(t.Item1 + 1, t.Item2 + 1), bars1, bars2)) != null) {
			    covariance += (bars1.GetClose(t.Item1) - average1) * (bars2.GetClose(t.Item2) - average2);
		    }
		    covariance /= n;

		    double correlationCoefficient = covariance / (standardDeviation1 * standardDeviation2);
		    return correlationCoefficient;
	    }
	    
	    private double SpearmanCorrelation(Commodity commodity1, Commodity commodity2, Bars bars1, Bars bars2, int n) {
		    Tuple<int, int> t;
		    
		    double v = 0;
		    t = new Tuple<int, int>(-1, -1);
		    while ((t = SynchronizeIndex(new Tuple<int, int>(t.Item1 + 1, t.Item2 + 1), bars1, bars2)) != null) {
			    int index1 = sortedCloses[commodity1].BinarySearch(bars1.GetClose(t.Item1));
			    int index2 = sortedCloses[commodity2].BinarySearch(bars2.GetClose(t.Item2));
			    v += (index1 - index2) * (index1 - index2);
		    }
		    return 1 - (6 * v / ((n - 1) * n * (n + 1)));
	    }
	    
    }
}
