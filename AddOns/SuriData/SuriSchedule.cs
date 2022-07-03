using System;
using System.IO;
using System.Net;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using System.Threading;

namespace NinjaTrader.Custom.AddOns.SuriData {
    public static class SuriSchedule {
        private static Thread _thread;
        private static bool _isLoadingData;
        
        public static void ToggleVpBigSchedule() {
            if (_thread != null && _thread.IsAlive) {
                _thread.Abort();
                SuriCommon.SuriCommon.Print("Schedule beendet");
                return;
            }
            _thread = new Thread(() => {
                SuriCommon.SuriCommon.Print("Schedule startet");
                while (_thread != null) {
                    Thread.Sleep(1000 * 60);

                    if (!_isLoadingData && DateTime.Now.Hour == 4 &&  DateTime.Now.Minute < 5) {
                        _isLoadingData = true;
                        SuriCommon.SuriCommon.Print("Export startet");
                        SuriVpBigScripts.StoreVpBigToFile();
                    }
                    if (_isLoadingData && SuriVpBigScripts.lastExportFinnished) {
                        _isLoadingData = false;
                        UploadVpBigToServer();
                    }
                }
            });
            _thread.Start();
        }
        
        private static void UploadVpBigToServer() {
            using (var client = new WebClient()) {
                client.Credentials = new NetworkCredential("root", "G9!W0C!2j&");

                foreach (string filePath in Directory.GetFiles(@"C:\Users\Bo\Documents\NinjaTrader 8\db\suri\vpbig\main\")) {
                    string fileName = Path.GetFileName(filePath);
                    client.UploadFile("ftp://app.suricate-trading.de/ninja/" + fileName, WebRequestMethods.Ftp.UploadFile, filePath);
                }
            }
        }

    }
}
