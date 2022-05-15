using System;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.Indicators;

namespace NinjaTrader.Custom.AddOns.SuriCommon {

    /*public interface IStrategyInterface {
        TradePosition? tradePosition { get; set; }
        /// A Signal indicates that we search for a market entry
        bool? isSignal { get; set; }
        /// Ready to enter the market
        bool? isEntry { get; set; }
        /// Exit the market now
        bool? isExit { get; set; }
        bool? isStop { get; set; }
        /// Set or change a stop
        double? stop { get; set; }
        double? entry { get; set; }
    }*/

    public enum TradePosition {
        Long,
        Middle,
        Short,
    }

    public abstract class StrategyIndicator : Indicator {
        abstract public TradePosition tradePosition { get; }

        private bool? _isSignal;
        /// A signal to start waiting for possible trades
        [XmlIgnore]
        [Browsable(false)]
        virtual public bool? isSignal { get { return _isSignal; } set { _isSignal = value; } }

        /// The Date when the very first signal happened
        public DateTime? firstSignalDate;
        /// Try to enter. Entering may fail due to too high stop or other criteria.
        public bool? isEntry;
        /// The Bar when an exit should happen.
        public int? exitBar = null;
        abstract public double? stop { get; set;}
        //public double? stop;
        /// The price to enter with a stop market order
        public double? entry;
        /// A value between 0 and 100 indicating the strength of the long / short position
        public double? positionStrength;

        public void Reset() {
            isSignal = null;
            isEntry = null;
            positionStrength = null;
        }



        public String GetEntryName(TradePosition? t = null) {
            if (t==null) t = tradePosition;
            return Name + " " + t + " In";
        }
    }

    public abstract class StrategyIndicator2 : Indicator {
        public abstract TradePosition GetTradePosition();
        /// A signal to start waiting for possible trades
        public abstract bool IsSignal();
        /// The Date when the signal happened. Expects to be called ONLY while IsSignal = true !
        public abstract DateTime? FirstSignalDate();
        /// Try to enter. Entering may fail due to too high stop or other criteria.
        public abstract bool? IsEntry();
        public abstract bool ShouldExit(TradePosition tradePosition);
        public abstract double GetStopValue();
        // The price to enter with a stop market order
        // public abstract double? GetEntryValue();
        /// A value between 0 and 100 indicating the strength of the long / short position
        public abstract double? GetPositionStrength();

        public abstract bool IsLong();
        public abstract bool IsShort();
        public abstract bool IsInLongHalf();
        public abstract bool IsInShortHalf();

        public String GetEntryName(TradePosition? t = null) {
            if (t==null) t = GetTradePosition();
            return Name + " " + t + " In";
        }
    }

}
