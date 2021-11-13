#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui.Tools;
#endregion

namespace NinjaTrader.NinjaScript.AddOns.Strings {}

public static class SuriStrings {
	public static Dictionary<Coms, string> ComsD = new Dictionary<Coms, string> {
		{Coms.Gold, "Gold"},
		{Coms.Silver, "Silber"},
		{Coms.Platinum, "Platin"},
		{Coms.Palladium, "Palladium"},
		{Coms.Copper, "Kupfer"},
		//{Coms.Iron, "Eisen"},
		//{Coms.Zinc, "Zink"},
		
		
		{Coms.CrudeOil, "Rohöl"},
		{Coms.Ethanol, "Ethanol"},
		{Coms.NaturalGas, "Erdgas"},
		//{Coms.BrentCrude, "Brent Öl"},
		//{Coms.HeatingOil, "Heizöl"},
		{Coms.Gasoline, "Benzin"},

		{Coms.Corn, "Mais"},
		{Coms.WheatZW, "Weizen"},
		{Coms.WheatKW, "Weizen"},
		{Coms.Rice, "Reis"},
		{Coms.Oats, "Hafer"},
		{Coms.Soybeans, "Sojabohnen"},
		{Coms.SoybeanMeal, "Sojamehl"},
		{Coms.SoybeanOil, "Sojaöl"},
		//{Coms.Milk, "Milch"},

		{Coms.Cacao, "Kakao"},
		{Coms.Cotton, "Baumwolle"},
		{Coms.OrangeJuice, "OSaft"},
		{Coms.Coffee, "Kaffee C"},
		{Coms.Sugar, "Zucker #11"},
		{Coms.Lumber, "Bauholz"},

		{Coms.LiveCattle, "Lebendrind"},
		{Coms.FeederCattle, "Mastrind"},
		{Coms.LiveHogs, "Schwein"},
	};
	
	public static Dictionary<Coms, string> ComsDShort = new Dictionary<Coms, string> {
		{Coms.Gold, "GC"},
		{Coms.Silver, "SI"},
		{Coms.Platinum, "PL"},
		{Coms.Palladium, "PA"},
		{Coms.Copper, "HG"},
		//{Coms.Iron, "TIO"},
		//{Coms.Zinc, "ZNC"},
		
		
		{Coms.CrudeOil, "CL"},
		{Coms.Ethanol, "EH"},
		{Coms.NaturalGas, "NG"},
		//{Coms.BrentCrude, "BB"},
		//{Coms.HeatingOil, "HO"},
		{Coms.Gasoline, "RB"},

		{Coms.Corn, "ZC"},
		{Coms.WheatZW, "ZW"},
		{Coms.WheatKW, "KW"},
		{Coms.Rice, "ZR"},
		{Coms.Oats, "ZO"},
		{Coms.Soybeans, "ZS"},
		{Coms.SoybeanMeal, "ZM"},
		{Coms.SoybeanOil, "ZL"},
		//{Coms.Milk, "DC"},

		{Coms.Cacao, "CC"},
		{Coms.Cotton, "CT"},
		{Coms.OrangeJuice, "OJ"},
		{Coms.Coffee, "KC"},
		{Coms.Sugar, "SB"},
		{Coms.Lumber, "LB"},

		{Coms.LiveCattle, "LE"},
		{Coms.FeederCattle, "GF"},
		{Coms.LiveHogs, "HE"},
	};

}

/// <summary>
/// Commodities
/// </summary>
public enum Coms {
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
	//BrentCrude,//Brent Öl
	//HeatingOil,//Heizöl
	Gasoline,//Benzin
	
	Corn,//Mais
	WheatZW,//Weizen
	WheatKW,//Weizen
	Rice,//Reis
	Oats,//Hafer
	Soybeans,//Sojabohnen
	SoybeanMeal,//Sojamehl
	SoybeanOil,//Sojaöl
	//Milk,//Milch
	
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
