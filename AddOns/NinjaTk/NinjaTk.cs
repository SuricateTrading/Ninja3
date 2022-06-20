#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.Gui.NinjaScript {
    public static class NinjaTk {
        private static HttpListener _httpListener;
        private static Commodity? currentlyListening;
        private static List<BarsRequest> _barsRequests = new List<BarsRequest>();
        private static SuriChartData _currentChartData;

        public static void Start() {
            try {
                if (_httpListener != null && _httpListener.IsListening) {
                    Cleanup();
                    return;
                }
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add("http://localhost:5001/");
                _httpListener.Start();
                new Thread(ResponseThread).Start();
            } catch (Exception e) {
                SuriCommon.Print(e.ToString());
            }
        }

        private static bool ListenTo(Commodity commodity) {
            if (currentlyListening == commodity) return true;
            currentlyListening = commodity;
            Unsubscrbe();
            CommodityData commodityData = SuriStrings.data[commodity];
            
            _currentChartData = new SuriChartData {
                commId = commodityData.id,
                date = DateTime.Now.ToString("yyy-MM-dd"),
                months = new List<SuriChartMonth>()
            };
            
            Instrument instrument = SuriCommon.GetInstrument(commodityData);
            for (int i = 0; i < commodityData.count - 3; i++) {
                SuriCommon.Print(instrument.FullName);
                var barsRequest = new BarsRequest(instrument, 2) {
                    MergePolicy = MergePolicy.DoNotMerge,
                    BarsPeriod = new BarsPeriod {BarsPeriodType = BarsPeriodType.Day, Value = 1},
                    TradingHours = instrument.MasterInstrument.TradingHours,
                };
                barsRequest.Request((request, error, arg3) => {
                    try {
                        var tuple = SuriCommon.GetMonthAndYearFromInstrument(request.Instrument);
                        int month = tuple.Item1;
                        int year = tuple.Item2;

                        SuriChartMonth suriChartMonth = null;
                        foreach (var chartMonth in _currentChartData.months) {
                            if (chartMonth.monthValue == month && chartMonth.year == year) {
                                suriChartMonth = chartMonth;
                                break;
                            }
                        }
                        if (suriChartMonth == null) {
                            suriChartMonth = new SuriChartMonth();
                            _currentChartData.months.Add(suriChartMonth);
                        }

                        if (request.Bars.Count > 1) {
                            suriChartMonth.last = request.Bars.GetClose(0);
                            suriChartMonth.settle = request.Bars.GetClose(0);
                            suriChartMonth.volume = request.Bars.GetVolume(0);
                        } else {
                            SuriCommon.Print("Empty bars.");
                        }
                        
                        suriChartMonth.year = year;
                        suriChartMonth.monthValue = month;
                    } catch (Exception e) {
                        SuriCommon.Print(e.ToString());
                    }
                });
                // barsRequest.Update += OnBarUpdate;
                _barsRequests.Add(barsRequest);
                instrument = SuriCommon.GetNextInstrument(instrument);
                if (instrument == null) {
                    SuriCommon.Print("instrument was null");
                    return false;
                }
            }
            return true;
        }

        private static void OnBarUpdate(object sender, BarsUpdateEventArgs e) {
            Commodity? commodity = SuriStrings.GetComm(e.BarsSeries.Instrument);
            if (commodity == null) return;
            //_currentChartData[commodity].months
            //e.BarsSeries.GetTime()
        }
        
        private static void ResponseThread() {
            while (_httpListener != null && _httpListener.IsListening) {
                try {
                    HttpListenerContext context = _httpListener.GetContext();
                    context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                    context.Response.AddHeader("Access-Control-Allow-Credentials", "*");
                    context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With, Access-Control-Allow-Headers, Origin,Accept, Access-Control-Request-Method, Access-Control-Request-Headers");
                    context.Response.AddHeader("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT");
                    context.Response.AddHeader("Access-Control-Max-Age", "1728000");
                    context.Response.AddHeader("Content-Type", "application/json");
                    context.Response.AddHeader("X-Content-Type-Options", "nosniff");
                    context.Response.AddHeader("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");
                    context.Response.AddHeader("Pragma", "no-cache");
                    context.Response.AddHeader("Expires", "0");
                    context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000 ; includeSubDomains");
                    context.Response.AddHeader("X-Frame-Options", "DENY");
                    context.Response.AddHeader("Keep-Alive", "timeout=60");
                    context.Response.AddHeader("Connection", "keep-alive");

                    Commodity? commodity = null;
                    foreach (string key in context.Request.QueryString) {
                        string value = context.Request.QueryString[key];

                        if (key.Equals("commId")) {
                            int commId = int.Parse(value);
                            commodity = SuriStrings.GetComm(commId);
                        }
                    }
                    byte[] response;
                    if (commodity == null || !ListenTo(commodity.Value)) {
                        response = Encoding.UTF8.GetBytes("");
                    } else {
                        response = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_currentChartData));
                    }
                    
                    context.Response.OutputStream.Write(response, 0, response.Length);
                    context.Response.KeepAlive = false;
                    context.Response.Close();
                } catch (Exception e) {
                    SuriCommon.Print(e.ToString());
                }
            }
        }

        private static void Unsubscrbe() {
            foreach (var barsRequest in _barsRequests) {
                //barsRequest.Update -= OnBarUpdate;
                barsRequest.Dispose();
            }
            _barsRequests = new List<BarsRequest>();
            _currentChartData = null;
        }
        
        public static void Cleanup() {
            Unsubscrbe();
            if (_httpListener != null) _httpListener.Close();
            _httpListener = null;
        }
        
    }
}
