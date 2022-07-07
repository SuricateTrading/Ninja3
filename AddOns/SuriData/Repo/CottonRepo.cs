using System;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.Data;
using NinjaTrader.Data;

namespace NinjaTrader.Custom.AddOns.SuriData {
    public class CottonRepo : GenericDbRepo<CottonData> {
        public CottonRepo(Instrument instrument, Bars bars, bool isDelayed = false) : base(instrument, bars) {}
        protected override string urlT { get { return @"cotton/get"; } }
        protected override DateTime GetDate(int index) { return data[index].date.Date; }
        protected override bool reverseList { get { return true; } }
    }
    public class CottonData {
        public DateTime date;
        public int sales;
        public int purchases;
        public int openInterest;
    }
}
