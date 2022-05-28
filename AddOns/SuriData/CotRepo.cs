using System;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.Data;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;

namespace NinjaTrader.Custom.AddOns.SuriData {
    public class CotRepo : GenericDbRepo<DbCotData> {
        public CotRepo(Instrument instrument, Bars bars) : base(instrument, bars) {}
        protected override string urlT { get { return @"cot/get"; } }
        protected override DateTime GetDate(int index) { return data[index].Date.Date; }
    }
}
