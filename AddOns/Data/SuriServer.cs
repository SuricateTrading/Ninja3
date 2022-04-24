#region Using declarations
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using NinjaTrader.NinjaScript;

#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public static class SuriServer {
        
        private static string Post(string url, bool post) {
            //Code.Output.Process(url, PrintTo.OutputTab1);
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // todo: delete?
            
            var data = Encoding.ASCII.GetBytes("5YjNQrsvuJgoPCQs33cgcelvPCJ2");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (post) {
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
        
        public static List<TkData> GetTkData(int id, string oldDate, string newDate) {
            string url = "https://cloud2.suricate-trading.de:8443/tk/getTK?commId=" + id + "&oldDate=" + oldDate + "&newDate=" + newDate;
            string response = Post(url, true);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<TkData>>(response);
        }
        
        public static List<WasdeData> GetWasdeData(int id, bool isAmerica, string oldDate, string newDate) {
            string url = "https://cloud2.suricate-trading.de:8443/wasde/get?commId=" + id + "&isGlobal=" + !isAmerica + "&oldDate=" + oldDate + "&newDate=" + newDate;
            string response = Post(url, true);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<WasdeData>>(response);
        }
		
        public static List<DbCotData> GetCotData(int commId, DateTime oldDate, DateTime newDate, int? cotId = null) {
            string url = "https://cloud2.suricate-trading.de:8443/cot/get?commId=" + commId + "&oldDate=" + oldDate.Date.ToString("yyyy-MM-dd") + "&newDate=" + newDate.Date.ToString("yyyy-MM-dd");
            if (cotId != null) url += "&cotId=" + cotId;
            string response = Post(url, true);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<DbCotData>>(response);
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
                    case 0: suri.license = License.Basic; break;
                    case 2: suri.license = License.Premium; break;
                    case 99: suri.license = License.Dev; break;
                    default: suri.license = License.None; break;
                }
                return suri;
            } catch (Exception) {
                return new Suri { license = License.None };
            }
        }
    }
    
    public sealed class TkData {
        public DateTime Date {get; set;}
        public int TkState {get; set;}
        public double Delta {get; set;}
        public int Volume {get; set;}
        public int OpenInterest {get; set;}
    }
    
    public sealed class WasdeData {
        public DateTime Date {get; set;}	
        public bool IsGlobal {get; set;}
        public bool? IsProjection {get; set;}
        public int EndMarketYear {get; set;}
        public Dictionary<String, Double> Attributes {get; set;}
    }

    public sealed class DbCotData {
        public DateTime Date {get; set;}	
        
        public int OpenInterest {get; set;}
        public int NonCommercialsLong {get; set;}
        public int NonCommercialsShort {get; set;}
        public int NonCommercialsSpread {get; set;}
        public int CommercialsLong {get; set;}
        public int CommercialsShort {get; set;}
        public int NonReportablesLong {get; set;}
        public int NonReportablesShort {get; set;}
        
        public float? Cot1 {get; set;}
        public int? Cot2Min {get; set;}
        public int? Cot2Mid {get; set;}
        public int? Cot2Max {get; set;}

        public int CommercialsNetto() {
            return CommercialsLong - CommercialsShort;
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
