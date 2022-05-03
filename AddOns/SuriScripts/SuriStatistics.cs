#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.NinjaScript;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using NinjaTrader.Data;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class DevStatistics {
	    private static readonly string filePath = Globals.UserDataDir + @"mining\";
	    private static readonly string fileName = filePath + @"statistics.json";
	    private readonly List<Commodity> commodities = Enum.GetValues(typeof(Commodity)).Cast<Commodity>().ToList();
	    private readonly Dictionary<Commodity, Data> data = new Dictionary<Commodity, Data>();


	    public void Start() {
		    Code.Output.Process("Start Loading", PrintTo.OutputTab1);
		    foreach (var commodity in commodities) data[commodity] = new Data();
		    LoadData();
	    }

	    private void LoadData(int index = 0) {
		    if (index >= commodities.Count) {
			    Code.Output.Process("Start to load tick data", PrintTo.OutputTab1);
			    LoadTickData();
			    return;
		    }
		    try {
			    Commodity commodity = commodities[index];
			    Instrument instrument = SuriRepo.GetInstrument(SuriStrings.data[commodity]);
			    DateTime from = DateTime.Now.AddDays(-30).Date;
			    DateTime to = DateTime.Now.AddDays(-1).Date;
			    
			    new BarsRequest(instrument, from, to) {
				    MergePolicy = MergePolicy.MergeBackAdjusted,
				    BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Minute, Value = 1440},
				    TradingHours = instrument.MasterInstrument.TradingHours,
			    }.Request((bars, errorCode, errorMessage) => {
				    if (errorCode != ErrorCode.NoError) {
					    Code.Output.Process("Error: " + errorCode, PrintTo.OutputTab1);
					    return;
				    }

				    data[commodity].commodityId = SuriStrings.GetId(instrument).Value;
				    SessionIterator sessionIterator = new SessionIterator(bars.Bars);
				    sessionIterator.GetNextSession(bars.Bars.GetTime(0), true);
				    if (commodity == Commodity.OrangeJuice) {
					    Code.Output.Process("1232323: " + errorCode, PrintTo.OutputTab1);
				    }
				    data[commodity].tradingHours = sessionIterator.ActualSessionBegin.ToString("t") + " - " + sessionIterator.ActualSessionEnd.ToString("t");
				    data[commodity].ninjaName = instrument.MasterInstrument.Name;
				    data[commodity].exchange = String.Join(", ", instrument.MasterInstrument.Exchanges.ToArray());
				    try {
					    data[commodity].maintenanceMargin = Risk.Get("NinjaTrader Brokerage Default").ByMasterInstrument[instrument.MasterInstrument].MaintenanceMargin;
					    data[commodity].initialMargin = Risk.Get("NinjaTrader Brokerage Default").ByMasterInstrument[instrument.MasterInstrument].InitialMargin;
				    } catch (Exception) {/**/}
				    
				    data[commodity].tickSize = instrument.MasterInstrument.TickSize;
				    data[commodity].pointValue = instrument.MasterInstrument.PointValue;
				    data[commodity].tickValue = instrument.MasterInstrument.PointValue * instrument.MasterInstrument.TickSize;

				    // average
				    double mean = 0;
				    for (int i = 0; i < bars.Bars.Count; i++) {
					    data[commodity].volume += bars.Bars.GetVolume(i);
					    data[commodity].barSize += bars.Bars.GetHigh(i) - bars.Bars.GetLow(i);
					    if (i > 0) {
						    data[commodity].gap += Math.Abs(bars.Bars.GetClose(i - 1) - bars.Bars.GetOpen(i));
					    }
					    mean += bars.Bars.GetClose(i);
				    }
				    mean /= bars.Bars.Count;

				    double standardDeviation = 0;
				    for (int i = 0; i < bars.Bars.Count; i++) {
					    standardDeviation += Math.Pow(bars.Bars.GetClose(i) - mean, 2) / bars.Bars.Count;
				    }
				    standardDeviation = Math.Sqrt(standardDeviation);
				    
				    data[commodity].mean = instrument.MasterInstrument.RoundToTickSize(mean);
				    data[commodity].volatility = instrument.MasterInstrument.RoundToTickSize(standardDeviation);
				    data[commodity].volume = (long) Math.Round(data[commodity].volume / (double) bars.Bars.Count);
				    data[commodity].barSize = instrument.MasterInstrument.RoundToTickSize(data[commodity].barSize / bars.Bars.Count);
				    data[commodity].gap = instrument.MasterInstrument.RoundToTickSize(data[commodity].gap / (bars.Bars.Count - 1));
				    
				    LoadData(++index);
			    });
		    } catch (Exception e) {
			    Code.Output.Process("Error in " + index + " " + e, PrintTo.OutputTab1);
		    }
	    }

	    private void LoadTickData(int index = 0) {
		    if (index >= commodities.Count) {
			    Write();
			    return;
		    }
		    try {
			    Commodity commodity = commodities[index];
			    Instrument instrument = SuriRepo.GetInstrument(SuriStrings.data[commodity]);
			    DateTime from = DateTime.Now.AddDays(-7).Date;
			    DateTime to = DateTime.Now.AddDays(-1).Date;
			    
			    new BarsRequest(instrument, from, to) {
				    MergePolicy = MergePolicy.MergeBackAdjusted,
				    BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Tick, Value = 1},
				    TradingHours = instrument.MasterInstrument.TradingHours,
			    }.Request((bars, errorCode, errorMessage) => {
				    if (errorCode != ErrorCode.NoError) {
					    Code.Output.Process("Error: " + errorCode, PrintTo.OutputTab1);
					    return;
				    }
				    for (int i = 0; i < bars.Bars.Count; i++) {
					    data[commodity].slippage += Math.Abs(bars.Bars.GetBid(i) - bars.Bars.GetAsk(i));
				    }
				    data[commodity].slippage /= bars.Bars.Count;
				    LoadTickData(++index);
			    });
		    } catch (Exception e) {
			    Code.Output.Process("Error in " + index + " " + e, PrintTo.OutputTab1);
		    }
	    }

	    private void Write() {
		    File.WriteAllText(fileName, Newtonsoft.Json.JsonConvert.SerializeObject(data));
		    Code.Output.Process("Done", PrintTo.OutputTab1);
	    }

	    private class Data {
		    public int commodityId;
		    public string tradingHours;
		    public double maintenanceMargin;
		    public double initialMargin;
		    public double tickSize;
		    public double tickValue;
		    public double pointValue;
		    public string ninjaName;
		    public string exchange;
		    
		    // average values:
		    public double barSize;
		    public double slippage;
		    public long volume;
		    public double volatility;
		    public double mean;
		    public double gap;
	    }
	    
    }
    
}
