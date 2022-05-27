using System;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;

namespace NinjaTrader.Custom.AddOns.Data {
    public class TkDbRepo : GenericDbRepo<TkData> {
        private string dbPath1;
        private string urlT1;
        private string urlSuffix1;

        public TkDbRepo(Instrument instrument, DateTime start, DateTime end) : base(instrument, start, end) {}

        protected override string dbPath {
            get { return dbPath1; }
        }

        protected override string urlT {
            get { return urlT1; }
        }

        protected override string urlSuffix {
            get { return urlSuffix1; }
        }

        protected override DateTime GetDate(int index) {
            throw new NotImplementedException();
        }
    }
}
