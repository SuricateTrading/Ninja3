using System;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.Data;
using NinjaTrader.Data;

namespace NinjaTrader.Custom.AddOns.SuriData {
    public class NgasRepo : GenericDbRepo<NgasData> {
        public NgasRepo(Instrument instrument, Bars bars) : base(instrument, bars) {}
        protected override string urlT { get { return @"ngas/get"; } }
        protected override DateTime GetDate(int index) { return data[index].date.Date; }
        protected override bool reverseList { get { return true; } }
    }
    public class NgasData {
        public DateTime date;
        public int total;
    }
}
