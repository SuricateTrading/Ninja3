using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.Custom.AddOns.SuriCommon.strategies {
    public class JsonStrategy : StrategyInterface {
        public override void UpdateIndicators() {}
        protected override string name { get { return "JsonStrategy"; } }
        protected override int startBarIndex { get { return 0; } }
        
        public JsonStrategy(Bars bars, Instrument instrument) : base(bars, instrument) {
            signals.AddRange(JsonConvert.DeserializeObject<List<SuriSignal>>(JSON));
        }
        
        protected override bool IsEntry(int signalIndex) { return false; }
        protected override SuriSignal PrepareSignal(int signalIndex) { return null; }
        protected override bool SetAndCheckInitialStoploss(SuriSignal signal) { return true; }
        protected override void SetExit(SuriSignal signal) {}

        private const string JSON = @"
			[
				{
					""isLong"": false,
					""orderType"": 1,
					""limitPrice"": 0.0,
					""stopPrice"": 0.0,

					""entryIndex"": 5,
					""stops"": {
						""-1"": 140.9,
						""6"": 150,
					},
					""exitIndex"": 10,
				},
				{
					""isLong"": true,
					""orderType"": 1,
					""limitPrice"": 0.0,
					""stopPrice"": 0.0,

					""entryIndex"": 10,
					""stops"": {
						""-1"": 130.9,
						""4"": 130,
					},
					""exitIndex"": 20,
				}
			]
		";

        // orderType: Limit = 0, Market = 1, MIT = 2, StopLimit = 3, StopMarket = 4
        
    }
}
