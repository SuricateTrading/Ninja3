#region Using declarations
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public static class SuriServer {
        
        private static string Post(string url, bool post) {
            //Code.Output.Process(url, PrintTo.OutputTab1);
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (post) {
                var data = Encoding.ASCII.GetBytes(SuriAddOn.suri.NtLicense);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream()) {
                    stream.Write(data, 0, data.Length);
                }
            }
            
            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }
        
        public static List<WasdeData> GetWasdeData(int id, bool isAmerica, string oldDate, string newDate) {
            string url = "https://cloud2.suricate-trading.de:8443/wasde/get?commId=" + id + "&isGlobal=" + !isAmerica + "&oldDate=" + oldDate + "&newDate=" + newDate;
            string response = Post(url, true);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<WasdeData>>(response);
        }
		
        /** urlT must be 'cot/get' for example */
        public static List<T> GetGenericData<T>(int commId, DateTime oldDate, DateTime newDate, string urlT, string urlSuffix = null) {
            string url = "https://cloud2.suricate-trading.de:8443/" + urlT + "?commId=" + commId + "&oldDate=" + oldDate.Date.ToString("yyyy-MM-dd") + "&newDate=" + newDate.Date.ToString("yyyy-MM-dd");
            //Code.Output.Process(url, PrintTo.OutputTab1);
            if (urlSuffix != null) url += urlSuffix;
            string response = Post(url, true);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<T>>(response);
        }
        
        /** urlT must be 'cot/get' for example */
        public static bool ChangeMachineId(string oldMachineId) {
            string url = "https://cloud2.suricate-trading.de:8443/suriguard/change?" + "oldLicense=" + oldMachineId + "&newLicense=" + Cbi.License.MachineId;
            string response = Post(url, true);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<bool>(response);
        }
		
        public static Suri GetSuri(string license) {
            try {
                string url = "https://cloud2.suricate-trading.de:8443/suriguard/nt?license=" + license;
                string response = Post(url, false);
                var serializer = new JavaScriptSerializer();
                Suri suri = serializer.Deserialize<Suri>(response);
                if (suri == null) {
                    return new Suri { license = License.None };
                }
                DateTime dateTime = DateTime.Parse(suri.Until);
                if(dateTime < DateTime.Now) {
                    suri.license = License.None;
                    return suri;
                }
                switch (suri.LicenseType) {
                    case 0:  suri.license = License.Basic; break;
                    case 2:  suri.license = License.Premium; break;
                    case 99: suri.license = License.Dev; break;
                    default: suri.license = License.None; break;
                }
                return suri;
            } catch (Exception) {
                return new Suri { license = License.None };
            }
        }
    }

    public sealed class WasdeData {
        public DateTime Date {get; set;}	
        public bool IsGlobal {get; set;}
        public bool? IsProjection {get; set;}
        public int EndMarketYear {get; set;}
        public Dictionary<String, Double> Attributes {get; set;}
    }

    public sealed class DbCotData {
        public DateTime date {get; set;}
        //public DateTime releaseDate {get; set;}
        
        public int openInterest {get; set;}
        public int nonCommercialsLong {get; set;}
        public int nonCommercialsShort {get; set;}
        public int nonCommercialsSpread {get; set;}
        public int commercialsLong {get; set;}
        public int commercialsShort {get; set;}
        public int nonReportablesLong {get; set;}
        public int nonReportablesShort {get; set;}
        
        public double? Cot1 {get; set;}
        public int? Cot2Min {get; set;}
        public int? Cot2Mid {get; set;}
        public int? Cot2Max {get; set;}

        public int CommercialsNetto() {
            return commercialsLong - commercialsShort;
        }
    }

    public sealed class Suri {
        public string NtLicense {get; set;}
        public string AppLicense {get; set;}
        public string Name {get; set;}	
        public int LicenseType {get; set;}
        public string Until {get; set;}
        public License license {get; set;}
        public bool Vp {get; set;}
    }

    public enum License {
        None,
        Basic,
        Premium,
        Dev
    }
    
}
