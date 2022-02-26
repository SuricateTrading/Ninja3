#region Using declarations
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public sealed class SuriServer {
        public static List<TkData> GetTkData(int id, string oldDate, string newDate) {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // todo: delete?
            string url = "https://cloud2.suricate-trading.de:8443/tk/getTK?commId=" + id + "&oldDate=" + oldDate + "&newDate=" + newDate;
            
            var data = Encoding.ASCII.GetBytes("5YjNQrsvuJgoPCQs33cgcelvPCJ2");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream()) {
                stream.Write(data, 0, data.Length);
            }
            
            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream)) {
                string s = reader.ReadToEnd();
                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<List<TkData>>(s);
            }
        }
		
        public static List<DbCotData> GetCotData(int commId, string oldDate, string newDate, int cotId) {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // todo: delete?
            string url = "https://cloud2.suricate-trading.de:8443/cot/get?commId=" + commId + "&oldDate=" + oldDate + "&newDate=" + newDate + "&cotId=" + cotId;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            
            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream)) {
                string s = reader.ReadToEnd();
                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<List<DbCotData>>(s);
            }
        }
		
        public static License GetSuri(string license) {
            try {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                string url = "https://cloud2.suricate-trading.de:8443/suriguard/nt?license=" + license;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                
                using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream)) {
                    string s = reader.ReadToEnd();
                        var serializer = new JavaScriptSerializer();
                        Suri suri = serializer.Deserialize<Suri>(s);
                        DateTime dateTime = DateTime.Parse(suri.Until);
                        if(dateTime < DateTime.Now) return License.None;
                        
                        switch (suri.LicenseType) {
                            case 0: return License.Basic;
                            case 1: return License.PremiumCot;
                            case 2: return License.PremiumVp;
                            case 99: return License.Dev;
                        }
                        return License.None;
                }
            } catch (Exception) {
                return License.None;
            }
        }
    }
    
    public sealed class TkData {
        public string Date {get; set;}	
        public int TkState {get; set;}
        public double Delta {get; set;}
        public double DeltaPercent {get; set;}
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
