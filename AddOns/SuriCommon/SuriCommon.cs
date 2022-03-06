
namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public abstract class SuriCommon {
        public static string version = "1.0.1";
        public static string mostRecentVersion = "";

        public static bool isUpToDate { get {
            return mostRecentVersion.Equals(version);
        }}
    }
}
