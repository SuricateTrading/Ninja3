using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using NinjaTrader.Custom.AddOns.SuriData;
using NinjaTrader.Data;

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
		public List<SuriVpBarData> barData;
		[JsonIgnore] public SortedDictionary<int, SuriVpBox> boxes;
		[JsonIgnore] public bool isPrepared;

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
		
	}

	public abstract class SuriSingleVp {
		[JsonIgnore] public bool isVpBig;
		public SortedList<int, SuriVpTickData> tickData;
		private SortedList<int, SuriVpTickData> clusters = new SortedList<int, SuriVpTickData>();
		[JsonIgnore] public bool isPrepared;
		public double tickSize;
		[JsonIgnore] public int tickCount;
		public int low;
		public int high;

		[JsonIgnore] public int pocIndex;
		[JsonIgnore] public double pocVolume;
		
		[JsonIgnore] public int vaHigh;
		[JsonIgnore] public int vaLow;
		[JsonIgnore] public double vaPercentage;
		
		public double totalVolume;
		/** Buy */
		public long totalBids;
		/** Sell */
		public long totalAsks;
		[JsonIgnore] public double highestDelta;
		[JsonIgnore]
		public long delta { get { return totalAsks - totalBids; } }

		protected SuriSingleVp(bool isVpBig, double tickSize) {
			this.isVpBig = isVpBig;
			this.tickSize = tickSize;
			pocVolume = double.MinValue;
			tickData = new SortedList<int, SuriVpTickData>();
			low = int.MaxValue;
			high = int.MinValue;
		}

		/** DO NOT USE! For Serialization only! */
		public SuriSingleVp() {
			pocVolume = double.MinValue;
			tickData = new SortedList<int, SuriVpTickData>();
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
			} else if (tick <= _bid) {
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
			if (this.high < tickHigh) this.high = tickHigh;
			if (this.low  > tickLow ) this.low  = tickLow;
			
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
					SuriCommon.Print("asdsd");
				}
				
				// poc
				if (pocVolume < tickData[low + i].volume) {
					pocVolume = tickData[low + i].volume;
					pocIndex = i;
				}

				if (!isVpBig) {
					highestDelta = Math.Max(Math.Abs(highestDelta), Math.Abs(tickData[low + i].asks - tickData[low + i].bids));
				}
			}
			PocTickData().isMainPoc = true;


			// distributed volume
			/*for (int i = 0; i < tickData.Count; i++) {
				double average = 0;
				for (int j = i - 2; j < i + 2; j++) {
					if (j < 0) continue;
					if (j >= tickData.Count) break;
					average += tickData[low + j].volume;
				}
				tickData[low + i].distributedVolume = average / 5.0;
			}*/

			CalcClusters();
			MergeClusters();

			if (!isVpBig) {
				CalculateVaueArea();

				// sub poc
				/*foreach (KeyValuePair<int, VpTickData> tick in tickData) {
					if (tick.Value.volume * 1.1 > pocVolume) tick.Value.isSubPoc = true;
				}*/

				//SetLvns();
			}
		}
		
		private void CalcClusters() {
			// params
			int initialSearchRange = 20;
			
			clusters.Add(low + pocIndex, PocTickData());
			
			// first find local pocs and lvns in a very rough way.
			for (int i = low + pocIndex + 1; i < low + tickData.Count; i++) {
				int? index = null;
				double value = double.MaxValue;
				int noNewFound = 0;
				
				// find local lvn
				for (; i < low + tickData.Count && noNewFound < initialSearchRange; i++) {
					if (value > tickData[i].volume) {
						value = tickData[i].volume;
						noNewFound = 0;
						index = i;
					} else {
						noNewFound++;
					}
				}
				if (index == null) {
					SuriCommon.Print("Achtung: Ein LVN konnte nicht gefunden werden!");
					break;
				}
				i = index.Value;
				tickData[i].isLvn = true;
				clusters.Add(i, tickData[i]);

				if (i == low + tickData.Count - 1) break; // break when reaching alltime high
				
				// find local poc
				index = null;
				value = double.MinValue;
				noNewFound = 0;
				for (; i < low + tickData.Count && noNewFound < initialSearchRange; i++) {
					if (value < tickData[i].volume) {
						value = tickData[i].volume;
						noNewFound = 0;
						index = i;
					} else {
						noNewFound++;
					}
				}
				if (index == null) break;
				i = index.Value;
				tickData[i].isSubPoc = true;
				clusters.Add(i, tickData[i]);
			}
		}

		/// Tries to merge insignificant clusters
		private void MergeClusters() {
			// params
			int clusterMinTickRange = 60; // "Die minimale Größe eines Clusters in Ticks."
			int minPocLvnDistanceTickRange = 20; // "Die minimale Entfernung eines POCs zu einem der LVNs in Ticks."
			int strength = 30; // "Mit welcher Stärke ein Cluster erkannt wird. Ein hoher Wert zeichnet nur sehr starke Cluster ein. Ein niedriger auch kleine Cluster. Der Wert ist in Prozent und gibt an, wie viel Prozent der POC mindestens vom LVN entfernt sein muss. Ein Wert von 100% heißt, dass der POC mindestens doppelt so hoch wie der höchste LVN des Clusters sein muss."

			for (int i = 0; i < clusters.Count; i++) {
				var tickData = clusters.Values[i];
				var tickRange = 0;
				if (tickRange < clusterMinTickRange) {
					// expand cluster
				}
			}
		}
		
		
		#region Intra Only Functions
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
		#endregion
	}
	
	

	public sealed class SuriVpBigData : SuriSingleVp { public SuriVpBigData(double tickSize) : base(true, tickSize) {} }

	public sealed class SuriVpBarData : SuriSingleVp {
		public DateTime dateTime;
		public SuriVpBarData(double tickSize, DateTime dateTime) : base(false, tickSize) {
			this.dateTime = dateTime;
		}
		/** DO NOT USE! For Serialization only! */
		public SuriVpBarData() {}
	}
	
	public sealed class SuriVpTickData {
		public int tick;
		public double volume;
		[JsonIgnore] public double distributedVolume;
		public long bids;
		public long asks;
		[JsonIgnore] public bool isMainPoc;
		[JsonIgnore] public bool isNakedPoc;
		[JsonIgnore] public bool isSubPoc;
		[JsonIgnore] public bool isInValueArea;
		[JsonIgnore] public bool isHigh;
		[JsonIgnore] public bool isLvn;
		public SuriVpTickData(int tick) {
			this.tick = tick;
		}
	}
}
