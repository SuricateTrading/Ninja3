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
				return data.First(pair => pair.Value.shortName.Equals(shortName)).Key;
			} catch (InvalidOperationException) {
				return null;
			}
		}
		public static Commodity? GetComm(Instrument instrument) {
			return GetComm(instrument.MasterInstrument.Name);
		}
		
		public static string LongNameToShortName(string longName) {
			try {
				return data.First(pair => pair.Value.longName.Equals(longName)).Value.shortName;
			} catch (InvalidOperationException) {
				return null;
			}
		}
		
		public static readonly Dictionary<Commodity, CommodityData> data = new Dictionary<Commodity, CommodityData> {
			{Commodity.Gold,		new CommodityData(0,  "GC", "", "Gold", 10)},
			{Commodity.Silver,		new CommodityData(1,  "SI", "", "Silber", 10)},
			{Commodity.Palladium,	new CommodityData(2,  "PA", "", "Palladium", 7)},
			{Commodity.Platinum,	new CommodityData(3,  "PL", "", "Platin", 7)},
			{Commodity.Copper,		new CommodityData(4,  "HG", "", "Kupfer", 20)},

			{Commodity.CrudeOil,	new CommodityData(7,  "CL", "", "Rohöl", 40)},
			//{Commodity.Ethanol,		new CommodityData(8,  "EH", "", "Ethanol", 0)},
			{Commodity.NaturalGas,	new CommodityData(9,  "NG", "", "Erdgas", 40)},
			{Commodity.BrentCrude,	new CommodityData(10, "B", "BB", "Brent Öl", 36)},
			{Commodity.HeatingOil,	new CommodityData(11, "HO", "", "Heizöl", 30)},
			{Commodity.Gasoline,	new CommodityData(12, "RB", "", "Benzin", 50)},

			{Commodity.Corn,		new CommodityData(13, "ZC", "", "Mais", 13)},
			{Commodity.WheatZw,		new CommodityData(14, "ZW", "", "Weizen", 14)},
			{Commodity.Rice,		new CommodityData(16, "ZR", "", "Reis", 7)},
			{Commodity.Oats,		new CommodityData(17, "ZO", "", "Hafer", 7)},
			{Commodity.Soybeans,	new CommodityData(18, "ZS", "", "Sojabohnen", 16)},
			{Commodity.SoybeanMeal,	new CommodityData(19, "ZM", "", "Sojamehl", 14)},
			{Commodity.SoybeanOil,	new CommodityData(20, "ZL", "", "Sojaöl", 17)},
			{Commodity.Milk,		new CommodityData(21, "DC", "", "Milch", 17)},
			{Commodity.WheatKe,		new CommodityData(15, "KE", "", "Weizen", 14)},

			{Commodity.Coffee,		new CommodityData(22, "KC", "", "Kaffee", 6)},
			{Commodity.Cotton,		new CommodityData(23, "CT", "", "Baumwolle", 14)},
			{Commodity.Sugar,		new CommodityData(24, "SB", "", "Zucker", 8)},
			{Commodity.Cacao,		new CommodityData(25, "CC", "", "Kakao", 7)},
			{Commodity.OrangeJuice,	new CommodityData(26, "OJ", "", "Osaft", 6)},
			{Commodity.Lumber,		new CommodityData(27, "LB", "", "Bauholz", 6)},

			{Commodity.LiveCattle,	new CommodityData(28, "LE", "", "Lebendrind", 9)},
			{Commodity.FeederCattle,new CommodityData(29, "GF", "", "Mastrind", 8)},
			{Commodity.LiveHogs,	new CommodityData(30, "HE", "", "Schwein", 12)},

			{Commodity.Sp500,		new CommodityData(31, "ES", "", "S&P 500 Mini", 10)},
			{Commodity.Nikkei,		new CommodityData(32, "NKD", "", "Nikkei / USD", 10)},
			{Commodity.Nasdaq,		new CommodityData(33, "NQ", "", "Nasdaq Mini", 5)},
			{Commodity.Dow,			new CommodityData(34, "YM", "", "Dow Jones Mini", 4)},
			{Commodity.Vix,			new CommodityData(56, "VX", "", "CBOE Volatility Index", 4)},
/*
			{Commodity.U10Yn,		new CommodityData(35, "", "", "", 0)},
			{Commodity.UltraBond,	new CommodityData(36, "", "", "", 0)},
			{Commodity.UtrBond,		new CommodityData(37, "", "", "", 0)},
			{Commodity.Year30,		new CommodityData(38, "", "", "", 0)},
			{Commodity.Year10,		new CommodityData(39, "", "", "", 0)},
			{Commodity.Year5,		new CommodityData(40, "", "", "", 0)},
			{Commodity.Year2,		new CommodityData(41, "", "", "", 0)},
*/
			{Commodity.AustralianDollar,	new CommodityData(42, "6A", "", "Australischer Dollar", 10)},
			{Commodity.BritishPound,		new CommodityData(43, "6B", "", "Britisches Pfund", 10)},
			{Commodity.CanadianDollar,		new CommodityData(44, "6C", "", "Kanadischer Dollar", 8)},
			{Commodity.Euro,				new CommodityData(45, "6E", "", "Euro", 10)},
			{Commodity.Yen,					new CommodityData(46, "6J", "", "Japanischer Yen", 10)},
			{Commodity.BrazilianReal,		new CommodityData(47, "6L", "", "Brasilianisches Real", 10)},
			{Commodity.MexicanPeso,			new CommodityData(48, "6M", "", "Mexikanischer Peso", 10)},
			{Commodity.NewZealandDollar,	new CommodityData(49, "6N", "", "Neuseeland Dollar", 6)},
			{Commodity.RussianRuble,		new CommodityData(50, "6R", "", "Russischer Rubel", 10)},
			{Commodity.SwissFranc,			new CommodityData(51, "6S", "", "Schweizer Franken", 6)},
			{Commodity.SouthAfricanRand,	new CommodityData(57, "6Z", "", "Südafrikanischer Rand", 6)},
			{Commodity.UsDollar,			new CommodityData(52, "DX", "", "US Dollar", 3)},
			{Commodity.Eurodollar,			new CommodityData(53, "GE", "", "Euro-Dollar", 10)},
			{Commodity.BitcoinMicro,		new CommodityData(54, "BTC", "", "Bitcoin", 5)},
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
		public readonly string suriShortName;
		public readonly string longName;
		public readonly int count;

		public CommodityData(int id, string shortName, string suriShortName, string longName, int count) {
			this.id = id;
			this.shortName = shortName;
			this.suriShortName = suriShortName;
			this.longName = longName;
			this.count = count;
		}
	}
}
