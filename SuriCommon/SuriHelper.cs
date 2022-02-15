using System;

namespace NinjaTrader.Gui.NinjaScript {
    public abstract class SuriHelper {
        private static Random _random = new Random();
        public static int random { get { return _random.Next(1, 1000000); } }
    }
}