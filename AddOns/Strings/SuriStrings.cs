#region Using declarations
using System.Collections.Generic;
using System.Text.RegularExpressions;
#endregion

namespace NinjaTrader.NinjaScript.AddOns.Strings {}

public static class SuriStrings {
	
	public static string instrumentToName(string instrumentName) {
		string name = Regex.Replace(instrumentName, " .+", "");
		if (!TempS.ContainsKey(name)) return instrumentName;
		name = TempS[name];
		
		if(name != null)
			return name + " (" + instrumentName + ")";
		else
			return instrumentName;
	}

	public static int getId(string instrumentName) {
		switch (Regex.Replace(instrumentName, " .+", "")) {
			case "GC": return 0;
			case "SI": return 1;
			case "PA": return 2;
			case "PL": return 3;
			case "HG": return 4;
			case "TIO": case "TR": return 5;
			case "ZNC": case "ZA": return 6;
			
			case "CL": return 7;
			case "EH": case "FL": return 8;
			case "NG": return 9;
			case "BB": case "CB": return 10;
			case "HO": return 11;
			case "RB": return 12;
			
			case "ZC": return 13;
			case "ZW": return 14;
			case "ZR": return 15;
			case "ZO": return 16;
			case "ZS": return 17;
			case "ZM": return 18;
			case "ZL": return 19;
			case "DC": case "DL": return 20;
			case "KE": return 21;
			
			case "KC": return 22;
			case "CT": return 23;
			case "SB": return 24;
			case "CC": return 25;
			case "OJ": return 26;
			case "LB": case "LS": return 27;
			case "RS": return 55;
			
			case "LE": return 28;
			case "GF": return 29;
			case "HE": return 30;
			case "ES": return 31;
			case "NK": case "NY": return 32;
			case "NQ": return 33;
			case "YM": return 34;
			case "TN": return 35;
			case "UB": case "UD": return 36;
			case "Z3N": case "ZE": return 37;
			case "ZB": return 38;
			case "ZN": return 39;
			case "ZF": return 40;
			case "ZT": return 41;
			case "6A": case "A6": return 42;
			case "6B": case "B6": return 43;
			case "6C": case "D6": return 44;
			case "6E": case "E6": return 45;
			case "6J": case "J6": return 46;
			case "6L": case "L6": return 47;
			case "6M": case "M6": return 48;
			case "6N": case "N6": return 49;
			case "6R": case "R6": return 50;
			case "6S": case "S6": return 51;
			case "DX": return 52;
			case "GE": return 53;
			case "BA": return 54;
			default: return -1;
		}
	}
	
	public static readonly Dictionary<string, string> TempS = new Dictionary<string, string> {
		{"GC", "Gold"},
		{"SI", "Silber"},
		{"PL", "Platin"},
		{"PA", "Palladium"},
		{"HG", "Kupfer"},
		//{Coms.Iron, "Eisen"},
		//{Coms.Zinc, "Zink"},
		
		
		{"CL", "Rohöl"},
		{"EH", "Ethanol"},
		{"NG", "Erdgas"},
		//{Coms.BrentCrude, "Brent Öl"},
		//{Coms.HeatingOil, "Heizöl"},
		{"RB", "Benzin"},

		{"ZC", "Mais"},
		{"ZW", "Weizen"},
		{"KW", "Weizen"},
		{"ZR", "Reis"},
		{"ZO", "Hafer"},
		{"ZS", "Sojabohnen"},
		{"ZM", "Sojamehl"},
		{"ZL", "Sojaöl"},
		//{Coms.Milk, "Milch"},

		{"CC", "Kakao"},
		{"CT", "Baumwolle"},
		{"OJ", "OSaft"},
		{"KC", "Kaffee C"},
		{"SB", "Zucker #11"},
		{"LB", "Bauholz"},

		{"LE", "Lebendrind"},
		{"GF", "Mastrind"},
		{"HE", "Schwein"},
	};
	
	
	public static Dictionary<Commodity, string> ComsD = new Dictionary<Commodity, string> {
		{Commodity.Gold, "Gold"},
		{Commodity.Silver, "Silber"},
		{Commodity.Platinum, "Platin"},
		{Commodity.Palladium, "Palladium"},
		{Commodity.Copper, "Kupfer"},
		//{Coms.Iron, "Eisen"},
		//{Coms.Zinc, "Zink"},
		
		
		{Commodity.CrudeOil, "Rohöl"},
		{Commodity.Ethanol, "Ethanol"},
		{Commodity.NaturalGas, "Erdgas"},
		//{Coms.BrentCrude, "Brent Öl"},
		//{Coms.HeatingOil, "Heizöl"},
		{Commodity.Gasoline, "Benzin"},

		{Commodity.Corn, "Mais"},
		{Commodity.WheatZW, "Weizen"},
		{Commodity.WheatKW, "Weizen"},
		{Commodity.Rice, "Reis"},
		{Commodity.Oats, "Hafer"},
		{Commodity.Soybeans, "Sojabohnen"},
		{Commodity.SoybeanMeal, "Sojamehl"},
		{Commodity.SoybeanOil, "Sojaöl"},
		//{Coms.Milk, "Milch"},

		{Commodity.Cacao, "Kakao"},
		{Commodity.Cotton, "Baumwolle"},
		{Commodity.OrangeJuice, "OSaft"},
		{Commodity.Coffee, "Kaffee C"},
		{Commodity.Sugar, "Zucker #11"},
		{Commodity.Lumber, "Bauholz"},

		{Commodity.LiveCattle, "Lebendrind"},
		{Commodity.FeederCattle, "Mastrind"},
		{Commodity.LiveHogs, "Schwein"},
	};
	
	public static Dictionary<Commodity, string> ComsDShort = new Dictionary<Commodity, string> {
		{Commodity.Gold, "GC"},
		{Commodity.Silver, "SI"},
		{Commodity.Platinum, "PL"},
		{Commodity.Palladium, "PA"},
		{Commodity.Copper, "HG"},
		//{Coms.Iron, "TIO"},
		//{Coms.Zinc, "ZNC"},
		
		
		{Commodity.CrudeOil, "CL"},
		{Commodity.Ethanol, "EH"},
		{Commodity.NaturalGas, "NG"},
		//{Coms.BrentCrude, "BB"},
		//{Coms.HeatingOil, "HO"},
		{Commodity.Gasoline, "RB"},

		{Commodity.Corn, "ZC"},
		{Commodity.WheatZW, "ZW"},
		{Commodity.WheatKW, "KW"},
		{Commodity.Rice, "ZR"},
		{Commodity.Oats, "ZO"},
		{Commodity.Soybeans, "ZS"},
		{Commodity.SoybeanMeal, "ZM"},
		{Commodity.SoybeanOil, "ZL"},
		//{Coms.Milk, "DC"},

		{Commodity.Cacao, "CC"},
		{Commodity.Cotton, "CT"},
		{Commodity.OrangeJuice, "OJ"},
		{Commodity.Coffee, "KC"},
		{Commodity.Sugar, "SB"},
		{Commodity.Lumber, "LB"},

		{Commodity.LiveCattle, "LE"},
		{Commodity.FeederCattle, "GF"},
		{Commodity.LiveHogs, "HE"},
	};

}

/// <summary>
/// Commodities
/// </summary>
public enum Commodity {
	Gold,
	Silver,
	Platinum,
	Palladium,
	Copper,
	//Iron,
	//Zinc,
	
	CrudeOil,//Rohöl
	Ethanol,
	NaturalGas,//Erdgas
	BrentCrude,//Brent Öl
	HeatingOil,//Heizöl
	Gasoline,//Benzin
	
	Corn,//Mais
	WheatZW,//Weizen
	WheatKW,//Weizen
	Rice,//Reis
	Oats,//Hafer
	Soybeans,//Sojabohnen
	SoybeanMeal,//Sojamehl
	SoybeanOil,//Sojaöl
	Milk,//Milch
	Canola,
	
	Cacao,//Kakao
	Cotton,//Baumwolle
	OrangeJuice,//OSaft
	Coffee,//Kaffee C
	Sugar,//Zucker #11
	Lumber,//Bauholz
	
	LiveCattle,//Lebendrind
	FeederCattle,//Mastrind
	LiveHogs,//Schwein
	
	//SP500,
	//Nkkei,
	//NASDAQ,
	//Dow,
};
