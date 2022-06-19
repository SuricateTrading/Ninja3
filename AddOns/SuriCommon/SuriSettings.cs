using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using NinjaTrader.Core;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public sealed class SuriSettings {
        public static SuriSettings Get;
        
        [JsonProperty("toolbar_maerkte")]
        public List<string> toolbarMarketNames;
        
        public static void InitializeSettings() {
            string fieldName = Globals.UserDataDir + @"suri\einstellungen.json";
            if (!File.Exists(fieldName)) {
                using (WebClient webClient = new WebClient()) {
                    webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/einstellungen.json", fieldName);
                }
            }
            Get = JsonConvert.DeserializeObject<SuriSettings>(File.ReadAllText(fieldName));
        }
    }
}
