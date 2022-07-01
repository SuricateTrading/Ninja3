#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Cbi;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
	public static class SuriStrings {

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
			{Commodity.Gold,		new CommodityData(0,  "GC", new List<string> {"MGC"}, "Gold", 10, new List<int> {2, 4, 6, 8, 12})},
			{Commodity.Silver,		new CommodityData(1,  "SI", new List<string> {"QI"}, "Silber", 10, new List<int> {})},
			{Commodity.Palladium,	new CommodityData(2,  "PA", new List<string>(), "Palladium", 7, new List<int> {})},
			{Commodity.Platinum,	new CommodityData(3,  "PL", new List<string>(), "Platin", 7, new List<int> {})},
			{Commodity.Copper,		new CommodityData(4,  "HG", new List<string> {"MHG", "QC"}, "Kupfer", 20, new List<int> {})},

			{Commodity.CrudeOil,	new CommodityData(7,  "CL", new List<string> {"MCL", "QM"}, "Rohöl", 40, new List<int> {})},
			{Commodity.NaturalGas,	new CommodityData(9,  "NG", new List<string> {"QG"}, "Erdgas", 40, new List<int> {})},
			{Commodity.BrentCrude,	new CommodityData(10, "B" , new List<string> {}, "Brent Öl", 36, new List<int> {})},
			{Commodity.HeatingOil,	new CommodityData(11, "HO", new List<string> {}, "Heizöl", 30, new List<int> {})},
			{Commodity.Gasoline,	new CommodityData(12, "RB", new List<string> {}, "Benzin", 50, new List<int> {})},

			{Commodity.Corn,		new CommodityData(13, "ZC", new List<string> {"XC"}, "Mais", 13, new List<int> {})},
			{Commodity.WheatZw,		new CommodityData(14, "ZW", new List<string> {"XW"}, "Weizen", 14, new List<int> {})},
			{Commodity.Rice,		new CommodityData(16, "ZR", new List<string> {}, "Reis", 7, new List<int> {})},
			{Commodity.Oats,		new CommodityData(17, "ZO", new List<string> {}, "Hafer", 7, new List<int> {})},
			{Commodity.Soybeans,	new CommodityData(18, "ZS", new List<string> {"XK"}, "Sojabohnen", 16, new List<int> {})},
			{Commodity.SoybeanMeal,	new CommodityData(19, "ZM", new List<string> {}, "Sojamehl", 14, new List<int> {})},
			{Commodity.SoybeanOil,	new CommodityData(20, "ZL", new List<string> {}, "Sojaöl", 17, new List<int> {})},
			{Commodity.Milk,		new CommodityData(21, "DC", new List<string> {}, "Milch", 17, new List<int> {})},
			{Commodity.WheatKe,		new CommodityData(15, "KE", new List<string> {}, "Weizen", 14, new List<int> {})},

			{Commodity.Coffee,		new CommodityData(22, "KC", new List<string> {}, "Kaffee", 6, new List<int> {})},
			{Commodity.Cotton,		new CommodityData(23, "CT", new List<string> {}, "Baumwolle", 14, new List<int> {})},
			{Commodity.Sugar,		new CommodityData(24, "SB", new List<string> {}, "Zucker", 8, new List<int> {})},
			{Commodity.Cacao,		new CommodityData(25, "CC", new List<string> {}, "Kakao", 7, new List<int> {})},
			{Commodity.OrangeJuice,	new CommodityData(26, "OJ", new List<string> {}, "Osaft", 6, new List<int> {})},
			{Commodity.Lumber,		new CommodityData(27, "LB", new List<string> {}, "Bauholz", 6, new List<int> {})},

			{Commodity.LiveCattle,	new CommodityData(28, "LE", new List<string> {}, "Lebendrind", 9, new List<int> {})},
			{Commodity.FeederCattle,new CommodityData(29, "GF", new List<string> {}, "Mastrind", 8, new List<int> {})},
			{Commodity.LiveHogs,	new CommodityData(30, "HE", new List<string> {}, "Schwein", 12, new List<int> {})},

			{Commodity.Sp500,		new CommodityData(31, "ES", new List<string> {"MES"}, "S&P 500 Mini", 10, new List<int> {})},
			{Commodity.Nikkei,		new CommodityData(32, "NKD", new List<string> {}, "Nikkei / USD", 10, new List<int> {})},
			{Commodity.Nasdaq,		new CommodityData(33, "NQ", new List<string> {"VLQ", "MNQ"}, "Nasdaq Mini", 5, new List<int> {})},
			{Commodity.Dow,			new CommodityData(34, "YM", new List<string> {"MYM"}, "Dow Jones Mini", 4, new List<int> {})},
			{Commodity.Vix,			new CommodityData(56, "VX", new List<string> {"VXM"}, "CBOE Volatility Index", 4, new List<int> {})},
/*
			{Commodity.U10Yn,		new CommodityData(35, "", "", "", 0, new List<int> {})},
			{Commodity.UltraBond,	new CommodityData(36, "", "", "", 0, new List<int> {})},
			{Commodity.UtrBond,		new CommodityData(37, "", "", "", 0, new List<int> {})},
			{Commodity.Year30,		new CommodityData(38, "", "", "", 0, new List<int> {})},
			{Commodity.Year10,		new CommodityData(39, "", "", "", 0, new List<int> {})},
			{Commodity.Year5,		new CommodityData(40, "", "", "", 0, new List<int> {})},
			{Commodity.Year2,		new CommodityData(41, "", "", "", 0, new List<int> {})},
*/
			{Commodity.AustralianDollar,	new CommodityData(42, "6A", new List<string> {}, "Australischer Dollar", 10, new List<int> {})},
			{Commodity.BritishPound,		new CommodityData(43, "6B", new List<string> {}, "Britisches Pfund", 10, new List<int> {})},
			{Commodity.CanadianDollar,		new CommodityData(44, "6C", new List<string> {}, "Kanadischer Dollar", 8, new List<int> {})},
			{Commodity.Euro,				new CommodityData(45, "6E", new List<string> {"E7"}, "Euro", 10, new List<int> {})},
			{Commodity.Yen,					new CommodityData(46, "6J", new List<string> {"J7"}, "Japanischer Yen", 10, new List<int> {})},
			{Commodity.BrazilianReal,		new CommodityData(47, "6L", new List<string> {}, "Brasilianisches Real", 10, new List<int> {})},
			{Commodity.MexicanPeso,			new CommodityData(48, "6M", new List<string> {}, "Mexikanischer Peso", 10, new List<int> {})},
			{Commodity.NewZealandDollar,	new CommodityData(49, "6N", new List<string> {}, "Neuseeland Dollar", 6, new List<int> {})},
			{Commodity.RussianRuble,		new CommodityData(50, "6R", new List<string> {}, "Russischer Rubel", 10, new List<int> {})},
			{Commodity.SwissFranc,			new CommodityData(51, "6S", new List<string> {}, "Schweizer Franken", 6, new List<int> {})},
			{Commodity.SouthAfricanRand,	new CommodityData(57, "6Z", new List<string> {}, "Südafrikanischer Rand", 6, new List<int> {})},
			{Commodity.UsDollar,			new CommodityData(52, "DX", new List<string> {}, "US Dollar", 3, new List<int> {})},
			{Commodity.Eurodollar,			new CommodityData(53, "GE", new List<string> {}, "Euro-Dollar", 10, new List<int> {})},
			{Commodity.BitcoinMicro,		new CommodityData(54, "BTC", new List<string> {"MBT", "BIT"}, "Bitcoin", 5, new List<int> {})},
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
		public readonly List<int> months;

		public CommodityData(int id, string shortName, List<string> alternativeMarkets, string longName, int count, List<int> months) {
			this.id = id;
			this.shortName = shortName;
			this.alternativeMarkets = alternativeMarkets;
			this.longName = longName;
			this.count = count;
			this.months = months;
		}
	}
}
