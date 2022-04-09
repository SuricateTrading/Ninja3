#region Using declarations
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using NinjaTrader.Core;
using Instrument = NinjaTrader.Cbi.Instrument;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public class SuriVpSerialization {
	    public static readonly string dbPath = Globals.UserDataDir + @"db\suri\";

	    public static Instrument GetInstrument(CommodityData commodity) {
		    return Instrument.GetInstrument(commodity.shortName + Instrument.GetInstrument(commodity.shortName+" ##-##").MasterInstrument.GetNextExpiry(DateTime.Now).ToString(" MM-yy"));
	    }
	    
		public static SuriVpBigData GetVpBig(Instrument instrument, DateTime? date = null) {
			int? year = null, week = null;
			if (date != null) {
				year = date.Value.Year;
				week = Week(date.Value);
			}
			
			string fileName = GetVpBigFilePath(instrument, year, week);
			bool updateVpFile = true;
			if (File.Exists(fileName)) {
				TimeSpan timeSpan = DateTime.Now - File.GetCreationTime(fileName);
				if (timeSpan.TotalDays <= 2 || timeSpan.TotalDays <= 1 && DateTime.Now.DayOfWeek == DayOfWeek.Saturday) {
					updateVpFile = false;
				}
			}
			if (updateVpFile) {
				string serverFile = @"https://app.suricate-trading.de/ninja/vpbig/" + instrument.MasterInstrument.Name + ".vpbig";
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

		public static string GetVpBigFilePath(Instrument instrument, int? year = null, int? week = null) {
			if (year == null) {
				string folder = dbPath + @"vpbig\main\";
				Directory.CreateDirectory(folder);
				return folder + instrument.MasterInstrument.Name + ".vpbig";
			} else {
				string folder = dbPath + @"vpbig\" + instrument.MasterInstrument.Name + @"\";
				Directory.CreateDirectory(folder);
				return folder + year + "_" + week + ".vpbig";
			}
		}

		public static int Week(DateTime time) {
			return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
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
