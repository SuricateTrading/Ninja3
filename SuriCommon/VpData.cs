using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;

namespace NinjaTrader.Gui.NinjaScript.Indicators {
	public class VpIntraData {
		public List<VpBarData> barData = new List<VpBarData>();
		public bool isPrepared;

		public void Prepare(ChartScale chartScale, double tickSize, Instrument instrument, Action<object> print) {
			isPrepared = true;
			for (int i = barData.Count - 1; i >= 0; i--) {
				VpBarData vpBarData = barData.ElementAt(i);
				vpBarData.Prepare(chartScale, tickSize, instrument, print);
				CalculateNakedPoc(i);
			}
		}

		private void CalculateNakedPoc(int index) {
			if (index >= barData.Count - 1) return;
			double pocPrice = barData[index].PocTickData().price;
			for (int i = index + 1; i < barData.Count; i++) {
				if (barData[i].tickData.ContainsKey(pocPrice) && barData[i].tickData[pocPrice].volume != 0 ) {
					return;
				}
			}
			barData[index].PocTickData().isNakedPoc = true;
		}
	}

	public abstract class SingleVp {
		public bool isPrepared;
		public readonly SortedDictionary<double, VpTickData> tickData = new SortedDictionary<double, VpTickData>(new DoubleComparer());
		public int pocIndex;
		public double pocVolume = double.MinValue;
		public int vaStartIndex;
		public int vaEndIndex;
		public double totalVolume;
		public long totalBids;
		public long totalAsks;

		public VpTickData At(int index) { return tickData.ElementAt(index).Value; }
		public VpTickData PocTickData() { return tickData.ElementAt(pocIndex).Value; }

		public void AddTick(MarketDataEventArgs e) {
			isPrepared = false;
			if (e.MarketDataType == MarketDataType.Last) {
				if (!tickData.ContainsKey(e.Price)) {
					tickData.Add(e.Price, new VpTickData());
					tickData[e.Price].price = e.Price;
				}
				tickData[e.Price].volume += e.Volume;
				totalVolume += e.Volume;

				if (e.Price >= e.Ask) {
					tickData[e.Price].asks += e.Volume;
					totalAsks += e.Volume;
				} else if (e.Price <= e.Bid) {
					tickData[e.Price].bids += e.Volume;
					totalBids += e.Volume;
				}
			}
		}
		
		public void AddMinuteVolume(long volume, double open, double high, double low, double close, double tickSize, Action<object> print) {
			isPrepared = false;
			double volumePerTick = volume / (Math.Abs(high - low) / tickSize + 1);
			for (double price = low; price <= high; price = Math.Round(price+tickSize, 7)) {
				if (!tickData.ContainsKey(price)) {
					tickData.Add(price, new VpTickData());
					tickData[price].price = price;
				}
				tickData[price].volume += volumePerTick;
				totalVolume += volumePerTick;
			}
		}

		public void CalculateVaueArea(bool checkPairs = false) {
			PocTickData().isInValueArea = true;
			double vaVolume = totalVolume * 0.682 - pocVolume;
			int indexAbove = pocIndex + 1;
			int indexBelow = pocIndex - 1;
			int i = 0; // this is only used to break the loop if it doesn't terminate. It hasn't happened yet, but just in case...
			while (vaVolume > 0) {
				i++;
				if (i > 300) {
					//print("error loop" + indexAbove + " " + indexBelow);
					break;
				}

				bool up1 = indexAbove < tickData.Count;
				bool up2 = indexAbove < tickData.Count -1;
				bool down1 = indexBelow >= 0;
				bool down2 = indexBelow >= 1;
				
				if (checkPairs) {
					double totalAbove = double.MinValue;
					if (up1) {
						totalAbove = At(indexAbove).volume;
						if (up2) totalAbove += At(indexAbove+1).volume;
					}
					double totalBelow = double.MinValue;
					if (down1) {
						totalBelow = At(indexBelow).volume;
						if (down2) totalBelow += At(indexBelow-1).volume;
					}

					if (up1 && totalAbove > totalBelow) {
						At(indexAbove).isInValueArea = true;
						vaVolume -= At(indexAbove).volume;
						indexAbove++;
						if (up2) {
							At(indexAbove).isInValueArea = true;
							vaVolume -= At(indexAbove).volume;
							indexAbove++;
						}
					} else if (down1 && totalAbove < totalBelow) {
						At(indexBelow).isInValueArea = true;
						vaVolume -= At(indexBelow).volume;
						indexBelow--;
						if (down2) {
							At(indexBelow).isInValueArea = true;
							vaVolume -= At(indexBelow).volume;
							indexBelow--;
						}
					} else {
						if (At(indexAbove).volume > At(indexBelow).volume) {
							At(indexAbove).isInValueArea = true;
							vaVolume -= At(indexAbove).volume;
							indexAbove++;
						} else if (At(indexAbove).volume < At(indexBelow).volume) {
							At(indexBelow).isInValueArea = true;
							vaVolume -= At(indexBelow).volume;
							indexBelow--;
						} else {
							At(indexAbove).isInValueArea = true;
							At(indexBelow).isInValueArea = true;
							vaVolume -= At(indexAbove).volume;
							vaVolume -= At(indexBelow).volume;
							indexAbove++;
							indexBelow--;
						}
					}
				} else {
					if (!down1 || up1 && At(indexAbove).volume > At(indexBelow).volume) {
						At(indexAbove).isInValueArea = true;
						vaVolume -= At(indexAbove).volume;
						indexAbove++;
					} else if (!up1 || At(indexAbove).volume < At(indexBelow).volume) {
						At(indexBelow).isInValueArea = true;
						vaVolume -= At(indexBelow).volume;
						indexBelow--;
					} else {
						At(indexAbove).isInValueArea = true;
						At(indexBelow).isInValueArea = true;
						vaVolume -= At(indexAbove).volume;
						vaVolume -= At(indexBelow).volume;
						indexAbove++;
						indexBelow--;
					}
				}
			}
		}
		
		public void Prepare(ChartScale chartScale, double tickSize, Instrument instrument, Action<object> print) {
			isPrepared = true;
			for (int i = 0; i < tickData.Count; i++) {
				KeyValuePair<double, VpTickData> entry = tickData.ElementAt(i);
				
				// add missing values with a volume of zero
				if (i > 0) {
					double curr = tickData.ElementAt(i).Key;
					double prev = tickData.ElementAt(i-1).Key;
					if (Math.Abs(curr - prev) > tickSize * 1.2) {
						tickData.Add(prev + tickSize, new VpTickData());
						i--;
						continue;
					}
				}
				
				// poc
				if (pocVolume < entry.Value.volume) {
					pocVolume = entry.Value.volume;
					pocIndex = i;
				}
			}
			PocTickData().isMainPoc = true;
			
			// check if we missed a leading or trailing value.
			// todo: kann es passieren, dass der high oder low tick kein volumen hatte?
			/*double highTicks = instrument.MasterInstrument.RoundToTickSize(chartScale.GetFirstChartBars().Bars.GetHigh(currentBar)) / tickSize;
			double lowTicks = instrument.MasterInstrument.RoundToTickSize(chartScale.GetFirstChartBars().Bars.GetLow(currentBar)) / tickSize;
			int ticks = (int)Math.Round(highTicks - lowTicks);
			if (tickData.Count != ticks + 1) {
				print("ERROR: tick count and data count was not the same on bar " + currentBar + ". " + ticks + " " + vpBarData.tickData.Count);
			}*/
			
			// sub poc
			foreach (KeyValuePair<double, VpTickData> tick in tickData) {
				if (tick.Value.volume * 1.1 > pocVolume) {
					tick.Value.isSubPoc = true;
				}
			}
			
			CalculateVaueArea();
			
			
		}
	}

	public class VpBigData : SingleVp {}
	public class VpBarData : SingleVp {}
	
	public class VpTickData {
		public double volume;
		public double price;
		public long bids;
		public long asks;
		public bool isMainPoc;
		public bool isNakedPoc;
		public bool isSubPoc;
		public bool isInValueArea;
	}
	
	public class DoubleComparer : IComparer<double> {
		public int Compare(double x, double y) {
			if (Math.Abs(x - y) < 0.0000001) return 0;
			if (x < y) return -1;
			if (x > y) return 1;
			return 0;
		}
	}
}
