using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Gui.Tools;

namespace NinjaTrader.Custom.AddOns.Data {
    public abstract class GenericDbRepo<T> {
        private string GetPath(int commId, int year) { return dbPath + commId + "_" + year + "." + typeof(T); }
        private static readonly Dictionary<Commodity, Mutex> state = new Dictionary<Commodity, Mutex>();
        
        protected abstract string dbPath { get; }
        protected abstract string urlT { get; }
        protected abstract string urlSuffix { get; }
        protected abstract DateTime GetDate(int index);
        
        public List<T> data;
        public int nextIndex;
        public bool hasStarted;

        static GenericDbRepo() {
            foreach (var commodity in Enum.GetValues(typeof(Commodity)).Cast<Commodity>()) {
                state.Add(commodity, new Mutex());
            }
        }
        public GenericDbRepo(Instrument instrument, DateTime start, DateTime end) {
            Directory.CreateDirectory(dbPath);
            Commodity? commodity = SuriStrings.GetComm(instrument);
            if (commodity != null && start.Year > 1900 && end.Year > 1900) {
                data = GetData(commodity.Value, start, end);
            } else {
                data = new List<T>();
            }
        }
        
        /** Gets data from local disk. If local data is outdated, then download from database and store to local disk. */
        private List<T> GetData(Commodity commodity, DateTime start, DateTime end) {
            state[commodity].WaitOne();
            var data = new List<T>();
            try {
                for (int year = start.Year; year <= end.Year; year++) {
                    int commId = SuriStrings.data[commodity].id;
                    string path = GetPath(commId, year);
                    TimeSpan fileAge = DateTime.Now - File.GetCreationTime(path);
                    if (!File.Exists(path) || fileAge.TotalDays >= 10 || year == DateTime.Now.Year && fileAge.TotalHours >= 8 ) {
                        // load file
                        List<T> part = SuriServer.GetGenericData<T>(commId, DateTime.Parse(year + "-01-01"), DateTime.Parse(year + "-12-31"), urlT, urlSuffix);
                        File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(part));
                    }
                    data.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path)));
                }
            } finally {
                state[commodity].ReleaseMutex();
            }
            return data;
        }

        /// Updates the *nextIndex* to the given dateTime which must come after or equal the current *nextIndex*.
        public int? Update(DateTime dateTime) {
            if (data.IsNullOrEmpty()) return null;
            for (int i = nextIndex; i < data.Count; i++) {
                if (GetDate(i).Date.Equals(dateTime.Date)) {
                    nextIndex = i;
                    hasStarted = true;
                    return nextIndex;
                }
                if (hasStarted && GetDate(i).Date > dateTime.Date) {
                    return nextIndex;
                }
            }
            return null;
        }
    }
}
