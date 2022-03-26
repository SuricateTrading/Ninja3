﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NinjaTrader.Data;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
	public sealed class VpBox {
		public readonly int endIndex;
		//public List<VpBarData> data;
		public readonly int length;
		public readonly int boxHigh;
		public readonly int boxLow;

		public VpBox(int length, int endIndex, int boxHigh, int boxLow) {
			this.length = length;
			this.endIndex = endIndex;
			this.boxHigh = boxHigh;
			this.boxLow = boxLow;
		}
	}
	
	public sealed class VpIntraData {
		public readonly List<VpBarData> barData = new List<VpBarData>();
		public readonly SortedDictionary<int, VpBox> boxes = new SortedDictionary<int, VpBox>();
		public bool isPrepared;

		public void Prepare() {
			if (isPrepared) return;
			isPrepared = true;
			for (int i = barData.Count - 1; i >= 0; i--) {
				barData[i].Prepare();
				CalculateNakedPoc(i);
			}
			for (int i = 0; i < barData.Count; i++) {
				CalculateBox(i);
			}
		}
		
		private void CalculateBox(int index) {
			int boxHigh = barData[index].vaHigh;
			int boxLow  = barData[index].vaLow;
			List<VpBarData> bars = new List<VpBarData> { barData[index] };
			for (int i = index - 1; i >= 0; i--) {
				VpBarData bar = barData[i];
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
				
				
				boxes[index] = new VpBox(bars.Count, index, boxHigh, boxLow);
			}
		}
		
		private void CalculateNakedPoc(int index) {
			if (index >= barData.Count - 1) return;
			int pocTick = barData[index].PocTickData().tick;
			for (int i = index + 1; i < barData.Count; i++) {
				if (barData[i].Contains(pocTick) && barData[i].tickData[pocTick].volume != 0 ) {
					return;
				}
			}
			barData[index].PocTickData().isNakedPoc = true;
		}
	}

	public abstract class SingleVp {
		public bool isVpBig;
		public readonly List<VpTickData> tickData = new List<VpTickData>();
		public bool isPrepared;
		public readonly double tickSize;
		public int tickCount;
		public int low = int.MaxValue;
		public int high = int.MinValue;
		
		public int pocIndex;
		public double pocVolume = double.MinValue;
		
		public int vaHigh;
		public int vaLow;
		public double vaPercentage;
		
		public double totalVolume;
		/** Buy */
		public long totalBids;
		/** Sell */
		public long totalAsks;
		public double highestDelta;
		public long delta { get { return totalAsks - totalBids; } }

		protected SingleVp(bool isVpBig, double tickSize) {
			this.isVpBig = isVpBig;
			this.tickSize = tickSize;
		}

		public VpTickData At(int index) { return tickData[low + index]; }
		public VpTickData PocTickData() { return At(pocIndex); }
		public int PriceToTick(double price) { return (int) Math.Round(price / tickSize); }

		/** Use only AFTER Prepared was called! */
		public bool Contains(int tick) { return tick >= low || tick <= high; }
		
		public void AddTick(MarketDataEventArgs e) {
			isPrepared = false;
			
			if (e.MarketDataType == MarketDataType.Last) {
				int tick = PriceToTick(e.Price);
				int bid  = PriceToTick(e.Bid);
				int ask  = PriceToTick(e.Ask);
				
				if (tickData.Last().tick != tick) {
					tickData.Add(new VpTickData(tick));
				}
				VpTickData last = tickData.Last();
				last.volume += e.Volume;
				totalVolume += e.Volume;

				if (tick >= ask) {
					last.asks += e.Volume;
					totalAsks += e.Volume;
				} /*else*/ if (tick <= bid) {
					last.bids += e.Volume;
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
				if (tickData.Last().tick != price) {
					tickData.Add(new VpTickData(price));
				}
				tickData.Last().volume += volumePerTick;
				totalVolume += volumePerTick;
			}
		}

		public void AddCached(double price, long volume, double ask, double bid) {
			isPrepared = false;
			
			int tick = PriceToTick(price);
			int _bid  = PriceToTick(bid);
			int _ask  = PriceToTick(ask);
			
			if (tickData.Last().tick != tick) {
				tickData.Add(new VpTickData(tick));
			}
			tickData.Last().volume += volume;
			totalVolume += volume;
			
			if (tick >= _ask) {
				tickData.Last().asks += volume;
				totalAsks += volume;
			} /*else*/ if (tick <= _bid) {
				tickData.Last().bids += volume;
				totalBids += volume;
			}
			
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
				// add missing values with a volume of zero
				if (i > 0) {
					int curr = At(i).tick;
					int prev = At(i-1).tick;
					if (Math.Abs(curr - prev) >= 2) {
						tickData.Insert(i, new VpTickData(prev + 1));
						i--;
						continue;
					}
				}
				
				// poc
				if (pocVolume < At(i).volume) {
					pocVolume = At(i).volume;
					pocIndex = i;
				}
				
				// distributed volume
				if (!isVpBig && SuriAddOn.license == License.Dev) {
					if (i == 0) {
						At(i)										.distributedVolume += At(i).volume / 2.0;
						if (i < tickData.Count - 1) At(i + 1)	.distributedVolume += At(i).volume / 2.0;
					} else if (i == tickData.Count - 1) {
						if (i > 0) At(i-1)						.distributedVolume += At(i).volume / 2.0;
						At(i)										.distributedVolume += At(i).volume / 2.0;
					} else {
						At(i-1)			.distributedVolume += At(i).volume / 3.0;
						At(i)					.distributedVolume += At(i).volume / 3.0;
						At(i + 1)			.distributedVolume += At(i).volume / 3.0;
					}
					
					highestDelta = Math.Max(Math.Abs(highestDelta), Math.Abs(At(i).asks - At(i).bids));
				}
			}
			
			PocTickData().isMainPoc = true;

			if (!isVpBig) {
				CalculateVaueArea();
			
				/*foreach (KeyValuePair<int, VpTickData> tick in tickData) {
					// sub poc
					if (tick.Value.volume * 1.1 > pocVolume) tick.Value.isSubPoc = true;
				}*/

				//SetLvns();
			}
		}

		/*
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
				
				double high1Volume = tickData.ElementAt(high1.Value).Value.distributedVolume;
				double high2Volume = tickData.ElementAt(high2.Value).Value.distributedVolume;
				double lowVolume   = tickData.ElementAt(low1 .Value).Value.distributedVolume;
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
				VpTickData tick = At(i);
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
				VpTickData tick = At(i);
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
		*/
	}

	public sealed class VpBigData : SingleVp { public VpBigData(double tickSize) : base(true, tickSize) {} }

	public sealed class VpBarData : SingleVp {
		public DateTime dateTime;
		public VpBarData(double tickSize, DateTime dateTime) : base(false, tickSize) {
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
