#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.NinjaScript;
using System.IO;
using System.Linq;
using MathNet.Numerics.Statistics;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using NinjaTrader.Data;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class DevCorrelation {
	    private static readonly string filePath = Globals.UserDataDir + @"mining\correlation\";
	    private StreamWriter pearsonWriter;
	    private StreamWriter spearmanWriter;
	    private StreamWriter correlationMatchingWriter;
	    private StreamWriter meanWriter;
	    private readonly List<Commodity> commodities = Enum.GetValues(typeof(Commodity)).Cast<Commodity>().ToList();
	    private readonly Dictionary<Commodity, BarsRequest> data = new Dictionary<Commodity, BarsRequest>();

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
		    pearsonWriter = File.CreateText(filePath + @"pearson.txt");
		    spearmanWriter = File.CreateText(filePath + @"spearman.txt");
		    correlationMatchingWriter = File.CreateText(filePath + @"correlationMatching.txt");
		    meanWriter = File.CreateText(filePath + @"mean.txt");
		    
		    // write header
		    WriteAll("Commodity\t");
		    foreach (var pair1 in data) WriteAll(pair1.Key + "\t");
		    
		    foreach (var pair1 in data) {
			    WriteAll("\n" + pair1.Key + "\t");
			    foreach (var pair2 in data) {
				    Bars bars1 = pair1.Value.Bars;
				    Bars bars2 = pair2.Value.Bars;
				    if (pair1.Key == pair2.Key) {
					    WriteAll("100\t");
					    continue;
				    }

				    List<double> bars1Matches = new List<double>();
				    List<double> bars2Matches = new List<double>();
				    Tuple<int, int> t = new Tuple<int, int>(-1, -1);
				    while ((t = SynchronizeIndex(new Tuple<int, int>(t.Item1 + 1, t.Item2 + 1), bars1, bars2)) != null) {
					    bars1Matches.Add(bars1.GetClose(t.Item1));
					    bars2Matches.Add(bars2.GetClose(t.Item2));
				    }
				    if (bars1Matches.Count < bars1.Count * 0.75) {
					    Code.Output.Process("ERROR zu viel fehlende Tage bei: " + pair1.Key + " " + pair2.Key + " " + (100 * bars1Matches.Count / ((double)bars1.Count)), PrintTo.OutputTab1);
					    WriteAll("\t");
					    continue;
				    }

				    double pearson  = Correlation.Pearson (bars1Matches, bars2Matches) * 100;
				    double spearman = Correlation.Spearman(bars1Matches, bars2Matches) * 100;
				    pearsonWriter .Write(pearson .ToString("F2") + "\t");
				    spearmanWriter.Write(spearman.ToString("F2") + "\t");
				    correlationMatchingWriter.Write((100 * Math.Min(pearson+1, spearman+1) / Math.Max(pearson+1, spearman+1)).ToString("F2") + "\t");
				    meanWriter.Write(((pearson + spearman)/2.0).ToString("F2") + "\t");
			    }
		    }
		    
		    pearsonWriter.Close();
		    spearmanWriter.Close();
		    correlationMatchingWriter.Close();
		    meanWriter.Close();
		    Code.Output.Process("End calculation", PrintTo.OutputTab1);
	    }

	    private void WriteAll(string text) {
		    pearsonWriter.Write(text);
		    spearmanWriter.Write(text);
		    correlationMatchingWriter.Write(text);
		    meanWriter.Write(text);
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
	    
    }
}
