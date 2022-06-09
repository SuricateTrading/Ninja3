#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Cbi;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
	public static class SuriStrings {
	
		/*public static string DisplayName(string prefix, Instrument instrument) {
			if (instrument == null) return prefix;
			return prefix + " (" + instrument.FullName + ")";
			
			string name = instrument.MasterInstrument.Name;
			Commodity? comm = GetComm(name);

			if (comm == null) {
				return prefix + " - " + instrument.FullName;
			}
			return prefix + " - " + data[comm.Value].longName + " (" + instrument.FullName + ")";
		}*/

		public static int? GetId(Instrument instrument) {
			try {
				return data.First(pair => pair.Value.shortName.Equals(instrument.MasterInstrument.Name)).Value.id;
			} catch (InvalidOperationException) {
				return null;
			}
		}
		
		public static Commodity? GetComm(string shortName) {
			try {
				return data.First(pair => pair.Value.shortName.Equals(shortName) || pair.Value.alternativeMarkets.Contains(shortName)).Key;
			} catch (InvalidOperationException) {
				return null;
			}
		}
		public static Commodity? GetComm(Instrument instrument) {
			return GetComm(instrument.MasterInstrument.Name);
		}
		public static Commodity? GetComm(int commId) {
			try {
				return data.First(pair => pair.Value.id == commId).Key;
			} catch (InvalidOperationException) {
				return null;
			}
		}
		
		public static string LongNameToShortName(string longName) {
			try {
				return data.First(pair => pair.Value.longName.Equals(longName)).Value.shortName;
			} catch (InvalidOperationException) {
				return null;
			}
		}
		
		public static readonly Dictionary<Commodity, CommodityData> data = new Dictionary<Commodity, CommodityData> {
			{Commodity.Gold,		new CommodityData(0,  "GC", new List<string> {"MGC"}, "Gold", 10)},
			{Commodity.Silver,		new CommodityData(1,  "SI", new List<string> {}, "Silber", 10)},
			{Commodity.Palladium,	new CommodityData(2,  "PA", new List<string> {}, "Palladium", 7)},
			{Commodity.Platinum,	new CommodityData(3,  "PL", new List<string> {}, "Platin", 7)},
			{Commodity.Copper,		new CommodityData(4,  "HG", new List<string> {}, "Kupfer", 20)},

			{Commodity.CrudeOil,	new CommodityData(7,  "CL", new List<string> {}, "Rohöl", 40)},
			//{Commodity.Ethanol,		new CommodityData(8,  "EH", new List<string> {}, "Ethanol", 0)},
			{Commodity.NaturalGas,	new CommodityData(9,  "NG", new List<string> {}, "Erdgas", 40)},
			{Commodity.BrentCrude,	new CommodityData(10, "B" , new List<string> {}, "Brent Öl", 36)},
			{Commodity.HeatingOil,	new CommodityData(11, "HO", new List<string> {}, "Heizöl", 30)},
			{Commodity.Gasoline,	new CommodityData(12, "RB", new List<string> {}, "Benzin", 50)},

			{Commodity.Corn,		new CommodityData(13, "ZC", new List<string> {}, "Mais", 13)},
			{Commodity.WheatZw,		new CommodityData(14, "ZW", new List<string> {}, "Weizen", 14)},
			{Commodity.Rice,		new CommodityData(16, "ZR", new List<string> {}, "Reis", 7)},
			{Commodity.Oats,		new CommodityData(17, "ZO", new List<string> {}, "Hafer", 7)},
			{Commodity.Soybeans,	new CommodityData(18, "ZS", new List<string> {}, "Sojabohnen", 16)},
			{Commodity.SoybeanMeal,	new CommodityData(19, "ZM", new List<string> {}, "Sojamehl", 14)},
			{Commodity.SoybeanOil,	new CommodityData(20, "ZL", new List<string> {}, "Sojaöl", 17)},
			{Commodity.Milk,		new CommodityData(21, "DC", new List<string> {}, "Milch", 17)},
			{Commodity.WheatKe,		new CommodityData(15, "KE", new List<string> {}, "Weizen", 14)},

			{Commodity.Coffee,		new CommodityData(22, "KC", new List<string> {}, "Kaffee", 6)},
			{Commodity.Cotton,		new CommodityData(23, "CT", new List<string> {}, "Baumwolle", 14)},
			{Commodity.Sugar,		new CommodityData(24, "SB", new List<string> {}, "Zucker", 8)},
			{Commodity.Cacao,		new CommodityData(25, "CC", new List<string> {}, "Kakao", 7)},
			{Commodity.OrangeJuice,	new CommodityData(26, "OJ", new List<string> {}, "Osaft", 6)},
			{Commodity.Lumber,		new CommodityData(27, "LB", new List<string> {}, "Bauholz", 6)},

			{Commodity.LiveCattle,	new CommodityData(28, "LE", new List<string> {}, "Lebendrind", 9)},
			{Commodity.FeederCattle,new CommodityData(29, "GF", new List<string> {}, "Mastrind", 8)},
			{Commodity.LiveHogs,	new CommodityData(30, "HE", new List<string> {}, "Schwein", 12)},

			{Commodity.Sp500,		new CommodityData(31, "ES", new List<string> {}, "S&P 500 Mini", 10)},
			{Commodity.Nikkei,		new CommodityData(32, "NKD", new List<string> {}, "Nikkei / USD", 10)},
			{Commodity.Nasdaq,		new CommodityData(33, "NQ", new List<string> {}, "Nasdaq Mini", 5)},
			{Commodity.Dow,			new CommodityData(34, "YM", new List<string> {}, "Dow Jones Mini", 4)},
			{Commodity.Vix,			new CommodityData(56, "VX", new List<string> {}, "CBOE Volatility Index", 4)},
/*
			{Commodity.U10Yn,		new CommodityData(35, "", "", "", 0)},
			{Commodity.UltraBond,	new CommodityData(36, "", "", "", 0)},
			{Commodity.UtrBond,		new CommodityData(37, "", "", "", 0)},
			{Commodity.Year30,		new CommodityData(38, "", "", "", 0)},
			{Commodity.Year10,		new CommodityData(39, "", "", "", 0)},
			{Commodity.Year5,		new CommodityData(40, "", "", "", 0)},
			{Commodity.Year2,		new CommodityData(41, "", "", "", 0)},
*/
			{Commodity.AustralianDollar,	new CommodityData(42, "6A", new List<string> {}, "Australischer Dollar", 10)},
			{Commodity.BritishPound,		new CommodityData(43, "6B", new List<string> {}, "Britisches Pfund", 10)},
			{Commodity.CanadianDollar,		new CommodityData(44, "6C", new List<string> {}, "Kanadischer Dollar", 8)},
			{Commodity.Euro,				new CommodityData(45, "6E", new List<string> {}, "Euro", 10)},
			{Commodity.Yen,					new CommodityData(46, "6J", new List<string> {}, "Japanischer Yen", 10)},
			{Commodity.BrazilianReal,		new CommodityData(47, "6L", new List<string> {}, "Brasilianisches Real", 10)},
			{Commodity.MexicanPeso,			new CommodityData(48, "6M", new List<string> {}, "Mexikanischer Peso", 10)},
			{Commodity.NewZealandDollar,	new CommodityData(49, "6N", new List<string> {}, "Neuseeland Dollar", 6)},
			{Commodity.RussianRuble,		new CommodityData(50, "6R", new List<string> {}, "Russischer Rubel", 10)},
			{Commodity.SwissFranc,			new CommodityData(51, "6S", new List<string> {}, "Schweizer Franken", 6)},
			{Commodity.SouthAfricanRand,	new CommodityData(57, "6Z", new List<string> {}, "Südafrikanischer Rand", 6)},
			{Commodity.UsDollar,			new CommodityData(52, "DX", new List<string> {}, "US Dollar", 3)},
			{Commodity.Eurodollar,			new CommodityData(53, "GE", new List<string> {}, "Euro-Dollar", 10)},
			{Commodity.BitcoinMicro,		new CommodityData(54, "BTC", new List<string> {}, "Bitcoin", 5)},
		};
		
	}

	public enum Commodity {
		Gold,
		Silver,
		Platinum,
		Palladium,
		Copper,
		//Iron,
		//Zinc,
	
		CrudeOil,
		//Ethanol,
		NaturalGas,
		BrentCrude,
		HeatingOil,
		Gasoline,
	
		Corn,
		WheatZw,
		Rice,
		Oats,
		Soybeans,
		SoybeanMeal,
		SoybeanOil,
		Milk,
		WheatKe,
	
		Coffee,
		Cotton,
		Sugar,
		Cacao,
		OrangeJuice,
		Lumber,
		//Canola,
	
		LiveCattle,
		FeederCattle,
		LiveHogs,
		
		Sp500,
		Nikkei,
		Nasdaq,
		Dow,
		Vix,
		/*
		U10Yn,
		UltraBond,
		UtrBond,
		Year30,
		Year10,
		Year5,
		Year2,
		*/
		AustralianDollar,
		BritishPound,
		CanadianDollar,
		Euro,
		Yen,
		BrazilianReal,
		MexicanPeso,
		NewZealandDollar,
		RussianRuble,
		SwissFranc,
		SouthAfricanRand,
		UsDollar,
		Eurodollar,
		BitcoinMicro,
	};

	public sealed class CommodityData {
		public readonly int id;
		public readonly string shortName;
		public readonly List<string> alternativeMarkets;
		public readonly string longName;
		public readonly int count;

		public CommodityData(int id, string shortName, List<string> alternativeMarkets, string longName, int count) {
			this.id = id;
			this.shortName = shortName;
			this.alternativeMarkets = alternativeMarkets;
			this.longName = longName;
			this.count = count;
		}
	}
}
