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
            Code.Output.Process(url, PrintTo.OutputTab1);
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
        
        public static List<WasdeData> GetWasdeData(int id, string oldDate, string newDate) {
            string url = "https://cloud2.suricate-trading.de:8443/wasde/get?commId=" + id + "&isGlobal=false&oldDate=" + oldDate + "&newDate=" + newDate;
            string response = Post(url, true);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<WasdeData>>(response);
        }
		
        public static List<DbCotData> GetCotData(int commId, string oldDate, string newDate, int cotId) {
            string url = "https://cloud2.suricate-trading.de:8443/cot/get?commId=" + commId + "&oldDate=" + oldDate + "&newDate=" + newDate + "&cotId=" + cotId;
            string response = Post(url, true);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<DbCotData>>(response);
        }
		
        public static License GetSuri(string license) {
            try {
                string url = "https://cloud2.suricate-trading.de:8443/suriguard/nt?license=" + license;
                string response = Post(url, false);
                var serializer = new JavaScriptSerializer();
                Suri suri = serializer.Deserialize<Suri>(response);
                DateTime dateTime = DateTime.Parse(suri.Until);
                if(dateTime < DateTime.Now) return License.None;
                switch (suri.LicenseType) {
                    case 0: return License.Basic;
                    case 1: return License.PremiumCot;
                    case 2: return License.PremiumVp;
                    case 99: return License.Dev;
                }
                return License.None;
            } catch (Exception) {
                return License.None;
            }
        }
    }
    
    public sealed class TkData {
        public DateTime Date {get; set;}	
        public int TkState {get; set;}
        public double Delta {get; set;}
    }
    
    public sealed class WasdeData {
        public DateTime Date {get; set;}	
        public bool IsGlobal {get; set;}
        public bool? IsProjection {get; set;}
        public int EndMarketYear {get; set;}
        public Dictionary<String, Double> Attributes {get; set;}

        public double endingStocks { get { return Attributes["Ending Stocks"]; } }
    }

    public sealed class DbCotData {
        public string Date {get; set;}	
        public int OpenInterest {get; set;}
    }

    public sealed class Suri {
        public string NtLicense {get; set;}
        public string AppLicense {get; set;}
        public string Name {get; set;}	
        public int LicenseType {get; set;}
        public string Until {get; set;}	
    }

    public enum License {
        None,
        Basic,
        PremiumCot,
        PremiumVp,
        Dev
    }
    
}