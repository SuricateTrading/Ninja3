#region Using declarations
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
#endregion

namespace NinjaTrader.Gui.NinjaScript {
    public class SuriServer {
		
        public static List<TkData> GetTkData(int id, string oldDate, string newDate) {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // todo: delete?
            string url = "https://localhost:8443/tk/getTK?commId=" + id + "&oldDate=" + oldDate + "&newDate=" + newDate;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            
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
            string url = "https://localhost:8443/cot/get?commId=" + commId + "&oldDate=" + oldDate + "&newDate=" + newDate + "&cotId=" + cotId;
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
		
        public static List<TkDeltaData> GetTkDelta(int commId, string oldDate, string newDate) {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // todo: delete?
            string url = "https://localhost:8443/aggregated/getTkDelta?commId=" + commId + "&oldDate=" + oldDate + "&newDate=" + newDate;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            
            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream)) {
                string s = reader.ReadToEnd();
                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<List<TkDeltaData>>(s);
            }
        }
		
		
    }
}

public class TkData {
    public string Date {get; set;}	
    public int TkState {get; set;}
}

public class DbCotData {
    public string Date {get; set;}	
    public int OpenInterest {get; set;}
}

public class TkDeltaData {
    public string Date {get; set;}	
    public double Delta {get; set;}	
    public double DeltaPercent {get; set;}
}
