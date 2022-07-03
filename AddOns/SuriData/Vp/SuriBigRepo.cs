#region Using declarations
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon.Vp {
    public class SuriBigRepo {
        
	    static SuriBigRepo() {
		    Directory.CreateDirectory(SuriCommon.dbPath + @"vpbig/");
		    Directory.CreateDirectory(SuriCommon.dbPath + @"vpbigdev/");
	    }
	    
		public static SuriVpBigData GetVpBig(Instrument instrument, bool dev = false, DateTime? date = null) {
			int? year = null, week = null;
			if (date != null) {
				year = date.Value.Year;
				week = SuriCommon.Week(date.Value);
			}
			
			string fileName = GetVpBigFilePath(instrument, dev, year, week);
			if (!File.Exists(fileName) || DateTime.Now.DayOfYear != File.GetCreationTime(fileName).DayOfYear) {
				// update vp
				string serverFile = @"https://app.suricate-trading.de/ninja/vpbig" + (dev ? "dev/" : "/") + instrument.MasterInstrument.Name + ".vpbig";
				try {
					using (WebClient webClient = new WebClient()) {
						webClient.DownloadFile(serverFile, fileName);
					}
				} catch (Exception) {
					return null;
				}
			}
			string json = File.ReadAllText(fileName);
			var serializer = new JavaScriptSerializer();
			SuriVpBigDataSerialized s = serializer.Deserialize<SuriVpBigDataSerialized>(json);
			
			SuriVpBigData suriVp = ToVpBig(s);
			suriVp.Prepare();
			return suriVp;
		}

		public static string GetVpBigFilePath(Instrument instrument, bool dev = false, int? year = null, int? week = null) {
			if (year == null) {
				string folder = SuriCommon.dbPath + @"vpbig" + (dev ? @"dev" : "") + @"\main\";
				Directory.CreateDirectory(folder);
				return folder + instrument.MasterInstrument.Name + ".vpbig";
			} else {
				string folder = SuriCommon.dbPath + @"vpbig" + (dev ? @"dev\" : @"\") + instrument.MasterInstrument.Name + @"\";
				Directory.CreateDirectory(folder);
				return folder + year + "_" + week + ".vpbig";
			}
		}

		
        public static SuriVpBigData ToVpBig(SuriVpBigDataSerialized suriVpBigDataSerialized) {
            SuriVpBigData suriVpBigData = new SuriVpBigData(suriVpBigDataSerialized.tickSize) {
                low = suriVpBigDataSerialized.low,
                high = suriVpBigDataSerialized.high,
                totalVolume = suriVpBigDataSerialized.totalVolume
            };
            int tick = suriVpBigData.low;
            foreach (var volume in suriVpBigDataSerialized.tickData) {
                suriVpBigData.tickData[tick] = new SuriVpTickData(tick);
                suriVpBigData.tickData[tick].volume = volume;
                tick++;
            }
            return suriVpBigData;
        }
        public static SuriVpBigDataSerialized FromVpBig(SuriVpBigData suriVpBigData) {
            SuriVpBigDataSerialized suriVpBigDataSerialized = new SuriVpBigDataSerialized {
                low = suriVpBigData.low,
                high = suriVpBigData.high,
                totalVolume = suriVpBigData.totalVolume,
                tickSize = suriVpBigData.tickSize,
                tickData = new List<long>()
            };
            foreach (var pair in suriVpBigData.tickData) {
                suriVpBigDataSerialized.tickData.Add((long) Math.Round(pair.Value.volume));
            }
            return suriVpBigDataSerialized;
        }
    }
    
    public sealed class SuriVpBigDataSerialized {
        public List<long> tickData;
        public int high;
        public int low;
        public double totalVolume;
        public double tickSize;
        public DateTime date;
    }
    
}