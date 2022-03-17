#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Cbi;
#endregion

namespace NinjaTrader.Custom.AddOns.SuriCommon {
	public static class SuriStrings {
	
		public static string DisplayName(string prefix, Instrument instrument) {
			if (instrument == null) return prefix;
			
			string name = instrument.MasterInstrument.Name;
			Commodity? comm = GetComm(name);

			if (comm == null) {
				return prefix + " - " + instrument.FullName;
			}
			return prefix + " - " + data[comm.Value].longName + " (" + instrument.FullName + ")";
		}

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
		
		public static string LongNameToShortName(string longName) {
			try {
				return data.First(pair => pair.Value.longName.Equals(longName)).Value.shortName;
			} catch (InvalidOperationException) {
				return null;
			}
		}
		
		public static readonly Dictionary<Commodity, CommodityData> data = new Dictionary<Commodity, CommodityData> {
			{Commodity.Gold,		new CommodityData(0,  "GC", "", "Gold")},
			{Commodity.Silver,		new CommodityData(1,  "SI", "", "Silber")},
			{Commodity.Palladium,	new CommodityData(2,  "PA", "", "Palladium")},
			{Commodity.Platinum,	new CommodityData(3,  "PL", "", "Platin")},
			{Commodity.Copper,		new CommodityData(4,  "HG", "", "Kupfer")},

			{Commodity.CrudeOil,	new CommodityData(7,  "CL", "", "Rohöl")},
			{Commodity.Ethanol,		new CommodityData(8,  "EH", "", "Ethanol")},
			{Commodity.NaturalGas,	new CommodityData(9,  "NG", "", "Erdgas")},
			{Commodity.BrentCrude,	new CommodityData(10, "B", "BB", "Brent Öl")},
			{Commodity.HeatingOil,	new CommodityData(11, "HO", "", "Heizöl")},
			{Commodity.Gasoline,	new CommodityData(12, "RB", "", "Benzin")},

			{Commodity.Corn,		new CommodityData(13, "ZC", "", "Mais")},
			{Commodity.WheatZw,		new CommodityData(14, "ZW", "", "Weizen")},
			{Commodity.Rice,		new CommodityData(16, "ZR", "", "Reis")},
			{Commodity.Oats,		new CommodityData(17, "ZO", "", "Hafer")},
			{Commodity.Soybeans,	new CommodityData(18, "ZS", "", "Sojabohnen")},
			{Commodity.SoybeanMeal,	new CommodityData(19, "ZM", "", "Sojamehl")},
			{Commodity.SoybeanOil,	new CommodityData(20, "ZL", "", "Sojaöl")},
			{Commodity.Milk,		new CommodityData(21, "DC", "", "Milch")},
			{Commodity.WheatKe,		new CommodityData(15, "KE", "", "Weizen")},

			{Commodity.Coffee,		new CommodityData(22, "KC", "", "Kaffee")},
			{Commodity.Cotton,		new CommodityData(23, "CT", "", "Baumwolle")},
			{Commodity.Sugar,		new CommodityData(24, "SB", "", "Zucker")},
			{Commodity.Cacao,		new CommodityData(25, "CC", "", "Kakao")},
			{Commodity.OrangeJuice,	new CommodityData(26, "OJ", "", "Osaft")},
			{Commodity.Lumber,		new CommodityData(27, "LB", "", "Bauholz")},

			{Commodity.LiveCattle,	new CommodityData(28, "LE", "", "Lebendrind")},
			{Commodity.FeederCattle,new CommodityData(29, "GF", "", "Mastrind")},
			{Commodity.LiveHogs,	new CommodityData(30, "HE", "", "Schwein")},
/*
			{Commodity.Sp500,		new CommodityData(31, "", "", "")},
			{Commodity.Nikkei,		new CommodityData(32, "", "", "")},
			{Commodity.Nasdaq,		new CommodityData(33, "", "", "")},
			{Commodity.Dow,			new CommodityData(34, "", "", "")},

			{Commodity.U10Yn,		new CommodityData(35, "", "", "")},
			{Commodity.UltraBond,	new CommodityData(36, "", "", "")},
			{Commodity.UtrBond,		new CommodityData(37, "", "", "")},
			{Commodity.Year30,		new CommodityData(38, "", "", "")},
			{Commodity.Year10,		new CommodityData(39, "", "", "")},
			{Commodity.Year5,		new CommodityData(40, "", "", "")},
			{Commodity.Year2,		new CommodityData(41, "", "", "")},
*/
			{Commodity.AustralianDollar,	new CommodityData(42, "6A", "", "Australischer Dollar")},
			{Commodity.BritishPound,		new CommodityData(43, "6B", "", "Britisches Pfund")},
			{Commodity.CanadianDollar,		new CommodityData(44, "6C", "", "Kanadischer Dollar")},
			{Commodity.Euro,				new CommodityData(45, "6E", "", "Euro")},
			{Commodity.Yen,					new CommodityData(46, "6J", "", "Japanischer Yen")},
			{Commodity.BrazilianReal,		new CommodityData(47, "6L", "", "Brasilianisches Real")},
			{Commodity.MexicanPeso,			new CommodityData(48, "6M", "", "Mexikanischer Peso")},
			{Commodity.NewZealandDollar,	new CommodityData(49, "6N", "", "Neuseeland Dollar")},
			{Commodity.RussianRuble,		new CommodityData(50, "6R", "", "Russischer Rubel")},
			{Commodity.SwissFranc,			new CommodityData(51, "6S", "", "Schweizer Franken")},
			{Commodity.UsDollar,			new CommodityData(52, "DX", "", "US Dollar")},
			{Commodity.Eurodollar,			new CommodityData(53, "GE", "", "Euro-Dollar")},
			{Commodity.BitcoinMicro,		new CommodityData(54, "BA", "", "Bitcoin")},
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
		Ethanol,
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
		/*
		Sp500,
		Nikkei,
		Nasdaq,
		Dow,
		
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
		UsDollar,
		Eurodollar,
		BitcoinMicro,
	};

	public sealed class CommodityData {
		public readonly int id;
		public readonly string shortName;
		public readonly string suriShortName;
		public readonly string longName;

		public CommodityData(int id, string shortName, string suriShortName, string longName) {
			this.id = id;
			this.shortName = shortName;
			this.suriShortName = suriShortName;
			this.longName = longName;
		}
	}
}
