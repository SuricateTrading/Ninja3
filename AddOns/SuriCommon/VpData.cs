using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Data;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
	public sealed class VpIntraData {
		public readonly List<VpBarData> barData = new List<VpBarData>();
		public bool isPrepared;

		public void Prepare() {
			isPrepared = true;
			for (int i = barData.Count - 1; i >= 0; i--) {
				VpBarData vpBarData = barData.ElementAt(i);
				vpBarData.Prepare();
				CalculateNakedPoc(i);
			}
		}

		private void CalculateNakedPoc(int index) {
			if (index >= barData.Count - 1) return;
			int pocTick = barData[index].PocTickData().tick;
			for (int i = index + 1; i < barData.Count; i++) {
				if (barData[i].tickData.ContainsKey(pocTick) && barData[i].tickData[pocTick].volume != 0 ) {
					return;
				}
			}
			barData[index].PocTickData().isNakedPoc = true;
		}
	}

	public abstract class SingleVp {
		public readonly SortedDictionary<int, VpTickData> tickData = new SortedDictionary<int, VpTickData>();
		public bool isPrepared;
		private readonly double tickSize;
		private int tickCount;
		private int low = int.MaxValue;
		private int high = int.MinValue;
		
		private int pocIndex;
		public double pocVolume = double.MinValue;
		
		private int vaHigh;
		private int vaLow;
		public double vaPercentage;
		
		public double totalVolume;
		public long totalBids;
		public long totalAsks;

		protected SingleVp(double tickSize) { this.tickSize = tickSize; }

		private VpTickData At(int index) { return tickData.ElementAt(index).Value; }
		public VpTickData PocTickData() { return tickData.ElementAt(pocIndex).Value; }
		private int PriceToTick(double price) { return (int) Math.Round(price / tickSize); }
		
		public void AddTick(MarketDataEventArgs e) {
			isPrepared = false;
			
			if (e.MarketDataType == MarketDataType.Last) {
				int tick = PriceToTick(e.Price);
				int bid  = PriceToTick(e.Bid);
				int ask  = PriceToTick(e.Ask);
				
				if (!tickData.ContainsKey(tick)) {
					tickData.Add(tick, new VpTickData(tick));
				}
				tickData[tick].volume += e.Volume;
				totalVolume += e.Volume;

				if (tick >= ask) {
					tickData[tick].asks += e.Volume;
					totalAsks += e.Volume;
				} else if (tick <= bid) {
					tickData[tick].bids += e.Volume;
					totalBids += e.Volume;
				}
				
				if (tick > high) high = tick;
				if (tick < low)  low  = tick;
			}
		}
		
		public void AddMinuteVolume(long volume, double high, double low) {
			isPrepared = false;
			
			int tickHigh = PriceToTick(high);
			int tickLow  = PriceToTick(low);
			if (tickHigh > this.high) this.high = tickHigh;
			if (tickLow  < this.low ) this.low  = tickLow;
			
			double volumePerTick = volume / (tickHigh - tickLow + 1.0);
			for (int price = tickLow; price <= tickHigh; price++) {
				if (!tickData.ContainsKey(price)) {
					tickData.Add(price, new VpTickData(price));
				}
				tickData[price].volume += volumePerTick;
				totalVolume += volumePerTick;
			}
		}

		public void AddCached(double price, long volume) {
			isPrepared = false;
			
			int tick = PriceToTick(price);
			
			if (!tickData.ContainsKey(tick)) {
				tickData.Add(tick, new VpTickData(tick));
			}
			tickData[tick].volume += volume;
			totalVolume += volume;
			
			if (tick > high) high = tick;
			if (tick < low)  low  = tick;
		}

		private void CalculateVaueArea(bool checkPairs = false) {
			PocTickData().isInValueArea = true;
			double vaVolume = totalVolume * 0.682 - pocVolume;
			vaHigh = PocTickData().tick;
			vaLow = PocTickData().tick;
			int indexAbove = pocIndex + 1;
			int indexBelow = pocIndex - 1;
			
			int i = 0; // this is only used to break the loop if it doesn't terminate. It hasn't happened yet, but just in case...
			while (vaVolume > 0) {
				i++;
				if (i > 300) {
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
						vaHigh = At(indexAbove).tick;
						indexAbove++;
						if (up2) {
							At(indexAbove).isInValueArea = true;
							vaVolume -= At(indexAbove).volume;
							vaHigh = At(indexAbove).tick;
							indexAbove++;
						}
					} else if (down1 && totalAbove < totalBelow) {
						At(indexBelow).isInValueArea = true;
						vaVolume -= At(indexBelow).volume;
						vaLow = At(indexBelow).tick;
						indexBelow--;
						if (down2) {
							At(indexBelow).isInValueArea = true;
							vaVolume -= At(indexBelow).volume;
							vaLow = At(indexBelow).tick;
							indexBelow--;
						}
					} else {
						if (At(indexAbove).volume > At(indexBelow).volume) {
							At(indexAbove).isInValueArea = true;
							vaVolume -= At(indexAbove).volume;
							vaHigh = At(indexAbove).tick;
							indexAbove++;
						} else if (At(indexAbove).volume < At(indexBelow).volume) {
							At(indexBelow).isInValueArea = true;
							vaVolume -= At(indexBelow).volume;
							vaLow = At(indexBelow).tick;
							indexBelow--;
						} else {
							At(indexAbove).isInValueArea = true;
							At(indexBelow).isInValueArea = true;
							vaVolume -= At(indexAbove).volume;
							vaVolume -= At(indexBelow).volume;
							vaHigh = At(indexAbove).tick;
							vaLow = At(indexBelow).tick;
							indexAbove++;
							indexBelow--;
						}
					}
				} else {
					if (!down1 || up1 && At(indexAbove).volume > At(indexBelow).volume) {
						At(indexAbove).isInValueArea = true;
						vaVolume -= At(indexAbove).volume;
						vaHigh = At(indexAbove).tick;
						indexAbove++;
					} else if (!up1 || At(indexAbove).volume < At(indexBelow).volume) {
						At(indexBelow).isInValueArea = true;
						vaVolume -= At(indexBelow).volume;
						vaLow = At(indexBelow).tick;
						indexBelow--;
					} else {
						At(indexAbove).isInValueArea = true;
						At(indexBelow).isInValueArea = true;
						vaVolume -= At(indexAbove).volume;
						vaVolume -= At(indexBelow).volume;
						vaHigh = At(indexAbove).tick;
						vaLow = At(indexBelow).tick;
						indexAbove++;
						indexBelow--;
					}
				}
			}
			
			vaPercentage = Math.Round(100 * (vaHigh - vaLow + 1.0) / (high - low + 1.0), 1);
		}
		
		public void Prepare() {
			isPrepared = true;
			tickCount = high - low;
			
			for (int i = 0; i < tickData.Count; i++) {
				KeyValuePair<int, VpTickData> entry = tickData.ElementAt(i);
				
				// add missing values with a volume of zero
				if (i > 0) {
					int curr = tickData.ElementAt(i).Key;
					int prev = tickData.ElementAt(i-1).Key;
					if (Math.Abs(curr - prev) >= 2) {
						tickData.Add(prev + 1, new VpTickData(prev + 1));
						i--;
						continue;
					}
				}
				
				// poc
				if (pocVolume < entry.Value.volume) {
					pocVolume = entry.Value.volume;
					pocIndex = i;
				}
				
				// distributed volume
				if (i == 0) {
					entry.Value									.distributedVolume += entry.Value.volume / 2.0;
					if (i < tickData.Count - 1) At(i + 1)	.distributedVolume += entry.Value.volume / 2.0;
				} else if (i == tickData.Count - 1) {
					if (i > 0) At(i - 1)					.distributedVolume += entry.Value.volume / 2.0;
					entry.Value									.distributedVolume += entry.Value.volume / 2.0;
				} else {
					At(i - 1)	.distributedVolume += entry.Value.volume / 3.0;
					entry.Value		.distributedVolume += entry.Value.volume / 3.0;
					At(i + 1)	.distributedVolume += entry.Value.volume / 3.0;
				}
				
			}
			PocTickData().isMainPoc = true;
			
			CalculateVaueArea();
			
			foreach (KeyValuePair<int, VpTickData> tick in tickData) {
				// sub poc
				if (tick.Value.volume * 1.1 > pocVolume) tick.Value.isSubPoc = true;
			}

			SetLvns();
		}

		
		private void SetLvns(int start = 0) {
			for (int i = start; i < tickData.Count; i++) {
				int? high1, high2, low1;
				high1 = GetHigh(i);
				if (high1 == null) return;

				low1 = GetLow(high1.Value);
				if (low1 == null) {
					i = high1.Value + 1; // todo: das funktioneirt, ist aber extrem ineffizient!
					continue;
				}
				
				high2 = GetHigh(low1.Value);
				if (high2 == null) {
					At(low1.Value).isLvn = false;
					return;
				}

				i = high2.Value;
			}
		}

		private int? GetHigh(int start) {
			int lookAhead = Math.Max(1, (int) Math.Round(tickCount * 0.1, 0));
			
			int ticksWithNoHigherValue = 0;
			double highestValue = double.MinValue;
			int highIndex = 0;
			for (int i = start; i < tickData.Count; i++) {
				VpTickData tick = At(i);
				if (highestValue < tick.distributedVolume) {
					highestValue = tick.distributedVolume;
					highIndex = i;
					ticksWithNoHigherValue = 0;
				} else {
					ticksWithNoHigherValue++;
				}

				if (ticksWithNoHigherValue == lookAhead - 1) {
					double average = AverageAround(highIndex, Math.Max(4, (int) Math.Round(tickCount * 0.1, 0)) );
					if (	average > totalVolume * 0.1 ||
					        Math.Abs(highIndex - pocIndex) < 4 ||
					        At(highIndex).distributedVolume * 3 > pocVolume
					   ) {
						At(highIndex).isHigh = true;
						return highIndex;
					}
				}
			}
			return null;
		}

		private int? GetLow(int start) {
			int lookAhead = Math.Max(1, (int) Math.Round(tickCount * 0.1, 0));
			
			int ticksWithNoLowerValue = 0;
			double lowestValue = double.MaxValue;
			int lowIndex = 0;
			for (int i = start; i < tickData.Count; i++) {
				VpTickData tick = At(i);
				if (lowestValue > tick.distributedVolume) {
					lowestValue = tick.distributedVolume;
					lowIndex = i;
					ticksWithNoLowerValue = 0;
				} else {
					ticksWithNoLowerValue++;
				}

				if (ticksWithNoLowerValue == lookAhead - 1) {
					double average = AverageAround(lowIndex, Math.Max(4, (int) Math.Round(tickCount * 0.1, 0)) );
					if (	average < totalVolume * 0.01 &&
					        At(lowIndex).distributedVolume < pocVolume / 10.0
					) {
						At(lowIndex).isLvn = true;
						return lowIndex;
					}
				}
			}
			return null;
		}

		private double AverageAround(int index, int range) {
			double value = 0;
			double count = 0;
			for (int i = index - (int) Math.Floor(range/2.0); i >= 0 && i < tickData.Count && i < index + (int) Math.Ceiling(range/2.0); i++) {
				count++;
				value += At(i).volume;
			}
			return value / count;
		}
		
	}

	public sealed class VpBigData : SingleVp { public VpBigData(double tickSize) : base(tickSize) {} }

	public sealed class VpBarData : SingleVp {
		public DateTime dateTime;
		public VpBarData(double tickSize, DateTime dateTime) : base(tickSize) {
			this.dateTime = dateTime;
		}
	}
	
	public sealed class VpTickData {
		public int tick;
		public double volume;
		public double distributedVolume;
		public long bids;
		public long asks;
		public bool isMainPoc;
		public bool isNakedPoc;
		public bool isSubPoc;
		public bool isInValueArea;
		public bool isHigh;
		public bool isLvn;
		public VpTickData(int tick) {
			this.tick = tick;
		}
	}
}
