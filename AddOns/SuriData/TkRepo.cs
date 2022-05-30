using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;

namespace NinjaTrader.Custom.AddOns.Data {
    public sealed class TkRepo : GenericDbRepo<TkData> {
        public TkRepo(Instrument instrument, Bars bars) : base(instrument, bars) {}
        protected override string urlT { get { return @"future/getTK4"; } }
        protected override DateTime GetDate(int index) { return data[index].date; }
        protected override bool reverseList { get { return true; } }
        
        protected override void OnDataLoaded() {
            if (commodity == null) return;
            foreach (var tkData in data) {
                foreach (var month in tkData.months) {
	                
                    double price = month.GetPrice();
                    if (month.count > SuriStrings.data[commodity.Value].count) break;
                    if (tkData.highestVolume < month.volume) {
                        tkData.highestVolume = month.volume;
                        tkData.highestVolIndex = month.count;
                        tkData.mainPrice = price;
                    }
                    if (tkData.highestPrice < price) tkData.highestPrice = price;
                    if (tkData.lowestPrice  > price) tkData.lowestPrice  = price;
                    tkData.volume += month.volume;
                    tkData.openInterest += month.openInterest;
                }

                int i = 0;
                foreach (var month in tkData.months) {

	                if (i > SuriStrings.data[commodity.Value].count - 1) break; // todo
	                
                    double price = month.GetPrice();
                    if (month.count < tkData.highestVolIndex && month.volume * 10 < tkData.highestVolume) {
                        month.isIgnored = true;
                    } else {
                        tkData.validMonths++;
                        if (tkData.lowestValidPrice  > price) tkData.lowestValidPrice  = price;
                        if (tkData.highestValidPrice < price) tkData.highestValidPrice = price;
                    }
                    i++;
                }

                tkData.delta = tkData.highestPrice - tkData.lowestPrice;
                //tkData.delta = tkData.highestValidPrice - tkData.lowestValidPrice;

                tkData.tkState = GetTkState(tkData);
            }
        }
        
        
        
		private TkState GetTkState(TkData tkData) {
			bool isContango = true; 
			bool isBackwardation = true;
			double contangoDeviation = 0.0;
			double backwardationDeviation = 0.0;
			double maxDeviation = tkData.delta * tkData.validMonths / 180.0;

			for (int i = 1; i < tkData.months.Count; i++) {
				var month = tkData.months[i];
				if (month.isIgnored) continue;
				var prevMonth = GetPrevMonth(tkData, i-1);
				if (prevMonth == null) continue;

				double price     = month    .GetPrice();
				double prevPrice = prevMonth.GetPrice();
				
				if (isContango && price < prevPrice) {
					contangoDeviation += Math.Abs(price - prevPrice);
					if (contangoDeviation > maxDeviation) isContango = false;
				}
				if (isBackwardation && price > prevPrice) {
					backwardationDeviation += Math.Abs(price - prevPrice);
					if (backwardationDeviation > maxDeviation) isBackwardation = false;
				}
				if (!isContango && !isBackwardation) break;
			}
			
			if (isBackwardation && isContango) {
				return contangoDeviation < backwardationDeviation ? TkState.Contango : TkState.Backwardation;
			}
			if (isContango) return TkState.Contango;
			if (isBackwardation) return TkState.Backwardation;

			TkMonth first = tkData.months.First(month => month.volume >= tkData.highestVolume * 0.1);
			double firstPrice = first.GetPrice();
			double lastPrice = tkData.months.Last().GetPrice();
			if (Math.Abs(firstPrice - tkData.lowestPrice)  < 0.00000001 && Math.Abs(lastPrice - tkData.highestPrice) < 0.00000001) return TkState.FirstLowestAndLastHighest;
			if (Math.Abs(firstPrice - tkData.highestPrice) < 0.00000001 && Math.Abs(lastPrice - tkData.lowestPrice)  < 0.00000001) return TkState.FirstHighestAndLastLowest;
			// todo: FirstThreeContango
			if (firstPrice < lastPrice) return TkState.FirstLowerThanLast;
			if (firstPrice > lastPrice) return TkState.FirstHigherThanLast;
			return TkState.None;
		}
		
		private TkMonth GetPrevMonth(TkData tkData, int startIndex) {
			for (int i = startIndex; i >= 0; i--) {
				if (!tkData.months[i].isIgnored) return tkData.months[i];
			}
			return null;
		}
        
    }
}


public sealed class TkData {
    public DateTime date;
    [JsonIgnore] public TkState tkState;
    //public double delta;
    //public int volume;
    //public int openInterest;
    public List<TkMonth> months;

    [JsonIgnore] public double mainPrice;
    [JsonIgnore] public long highestVolume;
    [JsonIgnore] public int highestVolIndex;
    [JsonIgnore] public double highestPrice = double.MinValue;
    [JsonIgnore] public double lowestPrice = double.MaxValue;
    [JsonIgnore] public double delta;
    [JsonIgnore] public double highestValidPrice = double.MinValue;
    [JsonIgnore] public double lowestValidPrice = double.MaxValue;
    [JsonIgnore] public int validMonths;
    [JsonIgnore] public long volume;
    [JsonIgnore] public long openInterest;
}

public sealed class TkMonth {
    public int monthValue;
    public int year;
    public double open;
    public double high;
    public double low;
    public double last;
    public double? settle;
    public long volume;
    public long openInterest;
    public int count;

    [JsonIgnore] public bool isIgnored;
    public double GetPrice() { return settle ?? last; }
}
public enum TkState {
	Backwardation,
	FirstHighestAndLastLowest,
	FirstHigherThanLast,
	None,
	FirstLowerThanLast,
	FirstLowestAndLastHighest,
	Contango,
	FirstThreeContango,
}
public static class TkStateExtensions {
	public static bool? IsAnyBackwardation(this TkState tkState) {
		switch (tkState) {
			case TkState.Backwardation: return true;
			case TkState.FirstHighestAndLastLowest: return true;
			case TkState.FirstHigherThanLast: return true;
			case TkState.None: return null;
			case TkState.FirstLowerThanLast: return false;
			case TkState.FirstLowestAndLastHighest: return false;
			case TkState.Contango: return false;
			case TkState.FirstThreeContango: return false;
		}
		return null;
	}
	public static bool? IsAnyContango(this TkState tkState) { return !tkState.IsAnyBackwardation(); }
	public static bool IsBackwardationToContango(this TkState tkState, TkState prevTkState) { return prevTkState.IsAnyBackwardation() == true && tkState.IsAnyContango() == true; }
	public static bool IsContangoToBackwardation(this TkState tkState, TkState prevTkState) { return prevTkState.IsAnyContango() == true && tkState.IsAnyBackwardation() == true; }
}
