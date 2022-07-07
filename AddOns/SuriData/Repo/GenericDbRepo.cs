using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.Custom.AddOns.Data {
    public abstract class GenericDbRepo<T> {
        private readonly string dbPath = Globals.UserDataDir + @"db\suri\" + typeof(T) + @"\";
        private string GetPath(int commId, int year) { return dbPath + commId + "_" + year + ".json"; }
        private static readonly Dictionary<Commodity, Mutex> state = new Dictionary<Commodity, Mutex>();
        protected static void Print(string s) { Code.Output.Process(s, PrintTo.OutputTab1); }
        
        /** urlT must be 'cot/get' for example */
        protected abstract string urlT { get; }
        /** Optional url suffix to add new html parameters. */
        protected virtual string urlSuffix { get { return null; } }
        protected abstract DateTime GetDate(int index);
        protected virtual void OnDataLoaded() {}
        protected virtual void OnPartialDataLoaded(List<T> partialData) {}
        protected virtual bool reverseList { get { return false; } }

        public List<T> data = new List<T>();
        public bool IsEmpty() { return data.IsNullOrEmpty(); }
        protected Commodity? commodity;

        /** Used to map each Bars-index to an index of the data-list. */
        protected readonly List<int> dataIndices = new List<int>();
        protected readonly Bars bars;

        static GenericDbRepo() { foreach (var commodity in Enum.GetValues(typeof(Commodity)).Cast<Commodity>()) state.Add(commodity, new Mutex()); }
        
        protected GenericDbRepo(Instrument instrument, Bars bars, DateTime? start = null) {
            this.bars = bars;
            if (start == null) start = bars.GetTime(0).AddDays(-14);
            DateTime end = bars.LastBarTime.Date;
            Directory.CreateDirectory(dbPath);
            commodity = SuriStrings.GetComm(instrument);
            if (commodity != null && start.Value.Year > 1900 && end.Year > 1900) {
                InitData(start.Value, end);
            }
        }
        
        /** May throw IndexOutOfRangeException */
        public T Get(int barIndex) { return data[dataIndices[barIndex]]; }

        /** Returns the index of the data-field of the given barIndex. */
        public int DataIndexOf(int barIndex) { return dataIndices[barIndex]; }
        
        /** Gets data from local disk. If local data is outdated, then download from database and store to local disk. */
        private void InitData(DateTime start, DateTime end) {
            if (commodity == null) return;
            state[commodity.Value].WaitOne();
            data = new List<T>();
            try {
                for (int year = start.Year; year <= end.Year; year++) {
                    int commId = SuriStrings.data[commodity.Value].id;
                    string path = GetPath(commId, year);
                    TimeSpan fileAge = DateTime.Now - File.GetCreationTime(path);
                    if (!File.Exists(path) || fileAge.TotalDays >= 10 || year == DateTime.Now.Year && fileAge.TotalHours >= 8 ) {
                        // load file
                        List<T> part = SuriServer.GetGenericData<T>(commId, DateTime.Parse(year + "-01-01"), DateTime.Parse(year + "-12-31"), urlT, urlSuffix);
                        if (reverseList) part.Reverse();
                        OnPartialDataLoaded(part);
                        File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(part));
                    }
                    data.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path)));
                }
                SetIndices();
                OnDataLoaded();
            } finally {
                state[commodity.Value].ReleaseMutex();
            }
        }

        private void SetIndices() {
            // get first index
            int dataIndex = 0;
            DateTime startDate = bars.GetTime(0).Date;
            for (; dataIndex < data.Count && startDate > GetDate(dataIndex).Date; dataIndex++) {}
            dataIndex--;

            // map all indices
            for (int i = 0; i < bars.Count; i++) {
                while (dataIndex < data.Count - 1 && bars.GetTime(i).Date >= GetDate(dataIndex + 1).Date) {
                    dataIndex++;
                }
                dataIndices.Add(dataIndex);
            }
        }

    }
}
