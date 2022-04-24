using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
	public sealed class SuriVpBox {
		public readonly int endIndex;
		//public List<VpBarData> data;
		public readonly int length;
		public readonly int boxHigh;
		public readonly int boxLow;

		public SuriVpBox(int length, int endIndex, int boxHigh, int boxLow) {
			this.length = length;
			this.endIndex = endIndex;
			this.boxHigh = boxHigh;
			this.boxLow = boxLow;
		}
	}
	
	public sealed class SuriVpIntraData {
		public List<SuriVpBarData> barData { get; set; }
		[JsonIgnore] public SortedDictionary<int, SuriVpBox> boxes { get; set; }
		[JsonIgnore] public bool isPrepared { get; set; }

		public SuriVpIntraData() {
			boxes = new SortedDictionary<int, SuriVpBox>();
			barData = new List<SuriVpBarData>();
		}
		
		public void Prepare() {
			if (isPrepared) return;
			isPrepared = true;
			int low = int.MaxValue;
			int high = int.MinValue;
			for (int i = barData.Count - 1; i >= 0; i--) {
				SuriVpBarData suriVpBarData = barData[i];
				suriVpBarData.Prepare();
				//CalculateNakedPoc(i);

				// naked poc:
				if (i != barData.Count -1 && (suriVpBarData.PocTickData().tick < low || suriVpBarData.PocTickData().tick > high)) {
					suriVpBarData.PocTickData().isNakedPoc = true;
				}
				low  = Math.Min(low,  suriVpBarData.low);
				high = Math.Max(high, suriVpBarData.high);
			}
			for (int i = 0; i < barData.Count; i++) {
				CalculateBox(i);
			}
		}
		
		private void CalculateBox(int index) {
			int boxHigh = barData[index].vaHigh;
			int boxLow  = barData[index].vaLow;
			List<SuriVpBarData> bars = new List<SuriVpBarData>();
			bars.Add(barData[index]);
			for (int i = index - 1; i >= 0; i--) {
				SuriVpBarData bar = barData[i];
				// check if value areas overlap
				if (bar.vaHigh < boxHigh && bar.vaHigh > boxLow || bar.vaLow > boxLow && bar.vaLow < boxHigh || bar.vaHigh > boxHigh && bar.vaLow < boxLow) {
					bars.Add(bar);
					boxHigh = Math.Min(bar.vaHigh, boxHigh);
					boxLow = Math.Max(bar.vaLow, boxLow);
				} else {
					break; // todo: es darf ausreißer geben
				}
			}
			if (bars.Count >= 3) {
				
				
				
				/*foreach (VpBarData vpBarData1 in bars) {
					foreach (VpBarData vpBarData2 in bars) {
						if (vpBarData1 == vpBarData2) continue;
						
					}
				}*/
				
				
				boxes[index] = new SuriVpBox(bars.Count, index, boxHigh, boxLow);
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

	public abstract class SuriSingleVp {
		[JsonIgnore] public bool isVpBig { get; set; }
		public SortedDictionary<int, SuriVpTickData> tickData { get; set; }
		[JsonIgnore] public bool isPrepared { get; set; }
		public double tickSize { get; set; }
		[JsonIgnore] public int tickCount { get; set; }
		public int low { get; set; }
		public int high { get; set; }

		[JsonIgnore] public int pocIndex { get; set; }
		[JsonIgnore] public double pocVolume { get; set; }
		
		[JsonIgnore] public int vaHigh { get; set; }
		[JsonIgnore] public int vaLow { get; set; }
		[JsonIgnore] public double vaPercentage { get; set; }
		
		public double totalVolume { get; set; }
		/** Buy */
		public long totalBids { get; set; }
		/** Sell */
		public long totalAsks { get; set; }
		[JsonIgnore] public double highestDelta { get; set; }
		[JsonIgnore]
		public long delta { get { return totalAsks - totalBids; } }

		protected SuriSingleVp(bool isVpBig, double tickSize) {
			this.isVpBig = isVpBig;
			this.tickSize = tickSize;
			pocVolume = double.MinValue;
			tickData = new SortedDictionary<int, SuriVpTickData>();
			low = int.MaxValue;
			high = int.MinValue;
		}

		/** DO NOT USE! For Serialization only! */
		public SuriSingleVp() {
			pocVolume = double.MinValue;
			tickData = new SortedDictionary<int, SuriVpTickData>();
			low = int.MaxValue;
			high = int.MinValue;
		}

		public SuriVpTickData At(int index) { return tickData[low + index]; }
		public SuriVpTickData PocTickData() { return tickData[low + pocIndex]; }
		public int PriceToTick(double price) { return (int) Math.Round(price / tickSize); }
		
		public void AddTick(MarketDataEventArgs e) {
			if (e.MarketDataType != MarketDataType.Last) return;
			AddTick(e.Time, e.Price, e.Volume, e.Ask, e.Bid);
		}
		public void AddTick(DateTime d, double price, long volume, double ask, double bid) {
			isPrepared = false;
			//Code.Output.Process(d + " " + price + " " + volume + " " + ask + " " + bid, PrintTo.OutputTab1);
			
			int tick = PriceToTick(price);
			int _bid  = PriceToTick(bid);
			int _ask  = PriceToTick(ask);
			
			if (!tickData.ContainsKey(tick)) {
				tickData[tick] = new SuriVpTickData(tick);
			}
			tickData[tick].volume += volume;
			totalVolume += volume;
			
			if (tick >= _ask) {
				tickData[tick].asks += volume;
				totalAsks += volume;
			} /*else*/ if (tick <= _bid) {
				tickData[tick].bids += volume;
				totalBids += volume;
			}
			
			if (tick > high) high = tick;
			if (tick < low)  low  = tick;
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
					tickData.Add(price, new SuriVpTickData(price));
				}
				tickData[price].volume += volumePerTick;
				totalVolume += volumePerTick;
			}
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

		public void AddMissingValues() {
			for (int i = 0; i < tickData.Count; i++) {
				if (!tickData.ContainsKey(low + i)) {
					tickData[low + i] = new SuriVpTickData(low + i);
				}
			}
		}
		
		public void Prepare() {
			isPrepared = true;
			tickCount = high - low;
			
			for (int i = 0; i < tickData.Count; i++) {
				// add missing values with a volume of zero
				if (!tickData.ContainsKey(low + i)) {
					tickData[low + i] = new SuriVpTickData(low + i);
				}
				
				// poc
				if (pocVolume < tickData[low + i].volume) {
					pocVolume = tickData[low + i].volume;
					pocIndex = i;
				}
			}
			PocTickData().isMainPoc = true;

			if (!isVpBig) {
				CalculateVaueArea();
				
				
				
				// distributed volume
				// must not be executed in the for-loop above, because entries may be missing.
				/*if (!isVpBig && SuriAddOn.license == License.Dev && false) {
					if (i == 0) {
						entry										.distributedVolume += entry.volume / 2.0;
						if (i < tickData.Count - 1) At(i + 1)	.distributedVolume += entry.volume / 2.0;
					} else if (i == tickData.Count - 1) {
						if (i > 0) At(i - 1)					.distributedVolume += entry.volume / 2.0;
						entry										.distributedVolume += entry.volume / 2.0;
					} else {
						At(i - 1)			.distributedVolume += entry.volume / 3.0;
						entry					.distributedVolume += entry.volume / 3.0;
						At(i + 1)			.distributedVolume += entry.volume / 3.0;
					}
					
					highestDelta = Math.Max(Math.Abs(highestDelta), Math.Abs(entry.asks - entry.bids));
				}*/
			
				/*foreach (KeyValuePair<int, VpTickData> tick in tickData) {
					// sub poc
					if (tick.Value.volume * 1.1 > pocVolume) tick.Value.isSubPoc = true;
				}*/

				//SetLvns();
			}
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
				
				double high1Volume = At(high1.Value).distributedVolume;
				double high2Volume = At(high2.Value).distributedVolume;
				double lowVolume   = At(low1 .Value).distributedVolume;
				if (lowVolume > Math.Min(high1Volume, high2Volume) * 0.4) {
					At(low1.Value).isLvn = false;
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
				SuriVpTickData tick = At(i);
				if (highestValue < tick.distributedVolume) {
					highestValue = tick.distributedVolume;
					highIndex = i;
					ticksWithNoHigherValue = 0;
				} else {
					ticksWithNoHigherValue++;
				}

				if (ticksWithNoHigherValue == lookAhead - 1) {
					double averageAround = AverageAround(highIndex, Math.Max(4, (int) Math.Round(tickCount * 0.1, 0)) );
					if (	//  * * * * * * * high criterias * * * * * * * * * * * * * * * * * * * * * * 
							averageAround > totalVolume * 0.1 ||
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
				SuriVpTickData tick = At(i);
				if (lowestValue > tick.distributedVolume) {
					lowestValue = tick.distributedVolume;
					lowIndex = i;
					ticksWithNoLowerValue = 0;
				} else {
					ticksWithNoLowerValue++;
				}

				if (ticksWithNoLowerValue == lookAhead - 1) {
					double averageAroundLow = AverageAround(lowIndex, Math.Max(4, (int) Math.Round(tickCount * 0.1, 0)) );
					if (	//  * * * * * * * low criterias * * * * * * * * * * * * * * * * * * * * * * 
							// averageAroundLow < At(pocIndex).distributedVolume * 0.35 &&
					        At(lowIndex).distributedVolume < At(pocIndex).distributedVolume * 0.3
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

	public sealed class SuriVpBigData : SuriSingleVp { public SuriVpBigData(double tickSize) : base(true, tickSize) {} }

	public sealed class SuriVpBarData : SuriSingleVp {
		public DateTime dateTime { get; set; }
		public SuriVpBarData(double tickSize, DateTime dateTime) : base(false, tickSize) {
			this.dateTime = dateTime;
		}
		/** DO NOT USE! For Serialization only! */
		public SuriVpBarData() {}
	}
	
	public sealed class SuriVpTickData {
		public int tick { get; set; }
		public double volume { get; set; }
		[JsonIgnore] public double distributedVolume { get; set; }
		public long bids { get; set; }
		public long asks { get; set; }
		[JsonIgnore] public bool isMainPoc { get; set; }
		[JsonIgnore] public bool isNakedPoc { get; set; }
		[JsonIgnore] public bool isSubPoc { get; set; }
		[JsonIgnore] public bool isInValueArea { get; set; }
		[JsonIgnore] public bool isHigh { get; set; }
		[JsonIgnore] public bool isLvn { get; set; }
		public SuriVpTickData(int tick) {
			this.tick = tick;
		}
	}
}
