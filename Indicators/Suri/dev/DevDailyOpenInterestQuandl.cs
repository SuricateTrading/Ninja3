#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion
using System.Globalization;
using System.IO;
using System.Net;


namespace NinjaTrader.NinjaScript.Indicators.Suri.dev
{
	public class DevDailyOpenInterestQuandl : Indicator
	{
		
		private string SymConverted ="";
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "DevDailyOpenInterestQuandl";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				QuandlAPIKey=  "Your QuandlAPIKey";
				AddPlot(Brushes.Orange, "OpenInterest");
			}
			else if(State == State.Historical)
			{						
				
		
				//Warning to only run this on Daily chart.
				if (BarsPeriod.BarsPeriodType != BarsPeriodType.Day)  //Putting a warning up if this is not applied to a Weekly Chart.
				{
					Draw.TextFixed(this, "NinjaScriptInfo2", "WARNING: ONLY USE THIS INDICATOR ON Daily Chart", TextPosition.Center);
					Log("WARNING: ONLY USE THIS INDICATOR ON Daily CHARTS", LogLevel.Error);
				//	return;
				}				

			}		
			else if (State == State.Configure)
			{
				
				
				
			} 
			else if (State== State.DataLoaded)
			{
										
				string  SymNewSchool = 	  (string)Instrument.MasterInstrument.Name;						
							
				string xSym = ReturnOldSchoolSym(SymNewSchool);
	 
				//	Print(xSym);
				SymConverted = xSym;		
						
				
				COTMain(xSym);
				
				
					
			}
		}

		private bool doOnce=false;
		protected override void OnBarUpdate()
		{
				
			if(CurrentBar<Bars.Count-2) return;	
				
			if(doOnce==false)
			{
				doOnce=true;
				//	string  xSymTemp = 	  (string)Instrument.MasterInstrument.Name;	
				//	string xSym = xSymTemp;
				string FileName = SymConverted+".csv";
				string FolderSavePath = NinjaTrader.Core.Globals.UserDataDir +"APData\\"+"COT\\" +"OI\\" ; 
			
			
				MyCotDataList =	MakeListOfCOTData(FolderSavePath+ FileName);
					
						
				if(MyCotDataList !=null)
					SetPlotWithCOTBars(MyCotDataList, Bars.Count);		
				else
					Draw.TextFixed(this, "this", "No Data Available", TextPosition.TopRight);				
				
			}
		}
		
		public void COTMain(string xSym)
		{

			CreateCheckFolder();

			string FileName = xSym+".csv";
			string FolderSavePath = NinjaTrader.Core.Globals.UserDataDir +"APData\\"+"COT\\" +"OI\\" ; 
			
			string filePath =FolderSavePath+	FileName ; 
			
			DateTime lastRunTime = ReadLastRunTimeFromFile();
			
			//Assuming OI is released at 5PM MST.
			TimeSpan reportTimeOfDay = new TimeSpan(17,00,00);
	
			//Three ways to trigger new data download.
			if(!File.Exists(filePath) //If file doesn't exist, meaning data aint there.
				|| 	 (DateTime.Now.Date >= lastRunTime.AddDays(1).Date)		//If Last pull was yesterday.
				||	(DateTime.Now.TimeOfDay > reportTimeOfDay && DateTime.Now.Date != lastRunTime.Date))//if wasn't pulled today and the time is after 5.		
			{
			
					
				SaveLastRuntimeToTxt();
				
				List<Url_Sym_Obj> urlList =  MakeURLList(QuandlAPIKey);
				
				foreach(Url_Sym_Obj s in urlList)					
					DownloadDataQuandl(s.xUrl, FolderSavePath + s.xSym + ".csv");
			}

			
		}
		public static void DownloadDataQuandl(string url, string FilePath)
		{
			string s=url;
		//	string x=NinjaTrader.Core.Globals.UserDataDir + FilePath;	
			
			try
	        {      
				//Delete file if its already there.
				 if (File.Exists(FilePath))
					 File.Delete(FilePath);
				
				
				using (WebClient webClient = new WebClient())  //If this is failing, delete the .zip folder in folder.
				{
					
					
				   webClient.DownloadFile(url, FilePath);
				}
		     }
            catch (Exception err)
            {
                throw new Exception("Sorry there was an error with downloading(): " + err.Message);
            }
		}
		
		
		
		private void SetPlotWithCOTBars(List<COTData_Obj> cotDataList, int barCount)
		{
			int numberOfPlotsToSet = Math.Min(barCount, cotDataList.Count);

			
			for(int i=0; i < numberOfPlotsToSet-2; i++)		
				Values[0][i]=  cotDataList[i].OI;


		}
		
		public class Url_Sym_Obj//Create Custom Object, an object with 2 objects inside..  //An object, with 2 objects inside.
		{
			public string xUrl {get; set;}
			public string xSym {get; set;}
	

		}
		private static List<Url_Sym_Obj> MakeURLList(string myKey)
		{
			
			List<Url_Sym_Obj> UrlSymString_List= new List<Url_Sym_Obj>();			
			
				
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_ES1.csv?api_key="+myKey, 
					xSym = "ES"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_TU1.csv?api_key="+myKey, 
					xSym = "TU"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_FV1.csv?api_key="+myKey, 
					xSym = "FV"				
			});

			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_TY1.csv?api_key="+myKey, 
					xSym = "TY"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_US1.csv?api_key="+myKey, 
					xSym = "US"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_UL1.csv?api_key="+myKey, 
					xSym = "UL"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_S1.csv?api_key="+myKey, 
					xSym = "S"				
			});

			
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_C1.csv?api_key="+myKey, 
					xSym = "C"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_W1.csv?api_key="+myKey, 
					xSym = "W"				
			});
		
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_SM1.csv?api_key="+myKey, 
					xSym = "SM"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_BO1.csv?api_key="+myKey, 
					xSym = "BO"				
			});

			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_RR1.csv?api_key="+myKey, 
					xSym = "RR"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_O1.csv?api_key="+myKey, 
					xSym = "O"				
			});
		
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_LC1.csv?api_key="+myKey, 
					xSym = "LC"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_LN1.csv?api_key="+myKey, 
					xSym = "LN"				
			});
			
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_EC1.csv?api_key="+myKey, 
					xSym = "EC"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_BP1.csv?api_key="+myKey, 
					xSym = "BP"				
			});
		
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_JY1.csv?api_key="+myKey, 
					xSym = "JY"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_CD1.csv?api_key="+myKey, 
					xSym = "CD"				
			});
	
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_MP1.csv?api_key="+myKey, 
					xSym = "MP"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_AD1.csv?api_key="+myKey, 
					xSym = "AD"				
			});
		
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_SF1.csv?api_key="+myKey, 
					xSym = "SF"				
			});
			UrlSymString_List.Add(new Url_Sym_Obj			
			{
					xUrl =	"https://www.quandl.com/api/v3/datasets/CHRIS/CME_NE1.csv?api_key="+myKey, 
					xSym = "NE"				
			});
			
			
		
			return UrlSymString_List;
			
			
			
		}
		
		public static List<COTData_Obj> MakeListOfCOTData(string FilePath)
		//public  void LoadCOTIntoList(string Sym)
		{
			//List<COTData_Obj> MyCotDataList = new List<COTData_Obj>();
			
		//List<COTData_Obj>
			MyCotDataList = new List<COTData_Obj>();
			
			
			int Date_SplitIndexNum=0;
			int OI_SplitIndexNum=8;
			
			string p1 = FilePath ;
			
			if (!File.Exists(p1))
			{
	
				NinjaTrader.Code.Output.Process("File does not exist.", PrintTo.OutputTab1);	
			
				return null;
			}		
			
			StreamReader s1;
		
			s1 = new System.IO.StreamReader(p1);
		
			string Line1;
			string[] split;
			
			while ((Line1 = s1.ReadLine()) != null) 
			{
				
				int splitCounter = 0;
				
				split =  Line1.Split(new char[] {','}, StringSplitOptions.None);			
				
				
	
					foreach(string s in split)
					{
					
						//If it finds Date in row, then proper header found, and we should use this apporach.
						if(s.Equals("Date"))
						{
							Date_SplitIndexNum = splitCounter;
								
						}
						else if(s.Equals("Previous Day Open Interest"))
						{
							OI_SplitIndexNum = splitCounter;
								
						}
							splitCounter++;
					}
							
				string myString;
				DateTime dateFortmated;


				try
				{
		
					
			
					myString=split[Date_SplitIndexNum];

					dateFortmated=	ParseDateFromString(myString);
					
					
					
				}
				catch
				{
					NinjaTrader.Code.Output.Process("Header was caught", PrintTo.OutputTab1);
					continue;	
				}
				
				double OIToUse =0;  //A+
				
				
				
				try
				{		
						if(double.TryParse(split[OI_SplitIndexNum], out OIToUse));	
						
						
						
						
				}
				catch(Exception e)
				{
					continue;  //Skip this one.
				}
				
				
				
				MyCotDataList.Add(new COTData_Obj
				{
					Time =  DateTime.Parse(split[0]), 
					OI  = double.Parse(split[8]),	

						
				});
					
			
			}
			
			if (s1  != null)
			{
				s1.Dispose();
				s1  = null;
			}
			
			return MyCotDataList;
			
			
			
		}
		
		public static DateTime ReadLastRunTimeFromFile()
		{
		
			DateTime lastRunTime = 	new DateTime(1900,1,1);
			string FileName = "LastRan"+".txt";
			string FolderSavePath = NinjaTrader.Core.Globals.UserDataDir +"APData\\"+"COT\\"+"OI\\";


			try
			{						
				string FolderSavePathPlusFileName 	= FolderSavePath + FileName;
				
				if (!File.Exists(FolderSavePathPlusFileName))
				{
					
					NinjaTrader.Code.Output.Process("FILE DOES NOT EXIST FOR THIS CONTRACT.", PrintTo.OutputTab1);				
					return lastRunTime;
				}		
				
				StreamReader s1;
			
				s1 = new System.IO.StreamReader(FolderSavePathPlusFileName);
			
				string Line1;

				int LineCounter = 0;
			
				while ((Line1 = s1.ReadLine()) != null) 
				{		
			
					if (LineCounter == 0)  //First line of file saved to xDate string variable.
							lastRunTime = DateTime.Parse(Line1);
					
					LineCounter++;					

				}
					
				if (s1  != null)
				{
					s1.Dispose();
					s1  = null;
				}	
			}
			catch(Exception e)
			{
				NinjaTrader.Code.Output.Process("Exception in SaveLastRuntimeToTxt"+e.ToString(), PrintTo.OutputTab1);	
				
				return lastRunTime;
			}
			
			return lastRunTime;
			
		}	
		public static void SaveLastRuntimeToTxt()
		{
				
			DateTime lastRunTime = DateTime.Now;
			string FileName = "LastRan"+".txt";
			string FolderSavePath = NinjaTrader.Core.Globals.UserDataDir +"APData\\"+"COT\\"+"OI\\";
			
			NinjaTrader.Code.Output.Process("Calling SaveLastRuntimeToTxt" , PrintTo.OutputTab1);

			try
			{						
				string FolderSavePathPlusFileName 	= FolderSavePath + FileName;	

				if(!Directory.Exists(FolderSavePath ))
				{

					 Directory.CreateDirectory(FolderSavePath);
				}
				
				if (File.Exists(FolderSavePathPlusFileName))
					File.Delete(FolderSavePathPlusFileName);//DELETE FILE IF ITS ALREADY THERE.
			
				if (!File.Exists(FolderSavePathPlusFileName))
				{
			
					using (StreamWriter sw = File.CreateText(FolderSavePathPlusFileName)) 
		            {
				
						sw.WriteLine(lastRunTime.ToString());
					
						sw.Close(); 
						sw.Dispose();
					}	

            
				}
				
			}
			catch(Exception e)
			{
				NinjaTrader.Code.Output.Process("Exception in SaveLastRuntimeToTxt"+e.ToString(), PrintTo.OutputTab1);	
			}
				
			
			
		}
	
		public static DateTime ParseDateFromString(string s)
		{
			
			//Method Written by Alan Palmer.  Palmer.ARJ at Gmail.com
			
			try
			{
				//Awesome method.  These are all the types of datetime types you can parse.  Add more as they change.
				string[] formats= { "yyyyMMdd","MM/dd/yyyy","MM-dd-yyyy","M-dd-yyyy", "M-d-yyyy", "MM-d-yyyy",
				"yyyy-MM-dd", "yyyy-M-d", "yyyy-MM-d", "yyyy-M-dd",
				"M/dd/yyyy", "M/d/yyyy", "MM/d/yyyy", 	"MM/dd/yyyy hh:mm:ss tt", 	"yyyy-MM-dd hh:mm:ss" };
				
				string myString;
				DateTime dateFortmated;					

				//Having a try catch in here was fucking our results!!!  Cause I had to return something so I returned datetime.price.
				dateFortmated = DateTime.ParseExact(s, formats, new CultureInfo("en-GB"), DateTimeStyles.None);
				return dateFortmated;
			
			}
			catch (Exception error)
			{
				
					//Will try our new extensive list of formats if the first doesnt' work.
					
					NinjaTrader.Code.Output.Process( "Problem in ParseDateFromString, Trying backup method.  New list of formats:", PrintTo.OutputTab1);	
						
					string[] formats= { "yyyyMMdd","MM/dd/yyyy","MM-dd-yyyy","M-dd-yyyy", "M-d-yyyy", "MM-d-yyyy",
					"yyyy-MM-dd", "yyyy-M-d", "yyyy-MM-d", "yyyy-M-dd",
					"M/dd/yyyy", "M/d/yyyy", "MM/d/yyyy", 	"MM/dd/yyyy hh:mm:ss tt", 	"yyyy-MM-dd hh:mm:ss" ,		
					"M/d/yyyy","M/d/yy","MM/dd/yy","MM/dd/yyyy","yy/MM/dd","yyyy-MM-dd",
					"dd-MMM-yy","dddd, MMMM d, yyyy","dddd, MMMM dd, yyyy","MMMM dd, yyyy","dddd, dd MMMM, yyyy","dd MMMM, yyyy",
					"dddd, MMMM d, yyyy h:mm tt","dddd, MMMM d, yyyy hh:mm tt","dddd, MMMM d, yyyy H:mm",
					"dddd, MMMM d, yyyy HH:mm","dddd, MMMM dd, yyyy h:mm tt","dddd, MMMM dd, yyyy hh:mm tt",
					"dddd, MMMM dd, yyyy H:mm","dddd, MMMM dd, yyyy HH:mm","MMMM dd, yyyy h:mm tt",
					"MMMM dd, yyyy hh:mm tt","MMMM dd, yyyy H:mm","MMMM dd, yyyy HH:mm","dddd, dd MMMM, yyyy h:mm tt",
					"dddd, dd MMMM, yyyy hh:mm tt","dddd, dd MMMM, yyyy H:mm","dddd, dd MMMM, yyyy HH:mm","dd MMMM, yyyy h:mm tt",
					"dd MMMM, yyyy hh:mm tt","dd MMMM, yyyy H:mm","dd MMMM, yyyy HH:mm","dddd, MMMM d, yyyy h:mm:ss tt","dddd, MMMM d, yyyy hh:mm:ss tt",
					"dddd, MMMM d, yyyy H:mm:ss","dddd, MMMM d, yyyy HH:mm:ss","dddd, MMMM dd, yyyy h:mm:ss tt","dddd, MMMM dd, yyyy hh:mm:ss tt",
					"dddd, MMMM dd, yyyy H:mm:ss","dddd, MMMM dd, yyyy HH:mm:ss","MMMM dd, yyyy h:mm:ss tt","MMMM dd, yyyy hh:mm:ss tt",
					"MMMM dd, yyyy H:mm:ss","MMMM dd, yyyy HH:mm:ss","dddd, dd MMMM, yyyy h:mm:ss tt",
					"dddd, dd MMMM, yyyy hh:mm:ss tt","dddd, dd MMMM, yyyy H:mm:ss","dddd, dd MMMM, yyyy HH:mm:ss","dd MMMM, yyyy h:mm:ss tt",
					"dd MMMM, yyyy hh:mm:ss tt","dd MMMM, yyyy H:mm:ss","dd MMMM, yyyy HH:mm:ss","M/d/yyyy h:mm tt","M/d/yyyy hh:mm tt","M/d/yyyy H:mm",
					"M/d/yyyy HH:mm","M/d/yy h:mm tt","M/d/yy hh:mm tt","M/d/yy H:mm","M/d/yy HH:mm","MM/dd/yy h:mm tt",
					"MM/dd/yy hh:mm tt","MM/dd/yy H:mm","MM/dd/yy HH:mm","MM/dd/yyyy h:mm tt",
					"MM/dd/yyyy hh:mm tt","MM/dd/yyyy H:mm","MM/dd/yyyy HH:mm","yy/MM/dd h:mm tt",
					"yy/MM/dd hh:mm tt","yy/MM/dd H:mm","yy/MM/dd HH:mm","yyyy-MM-dd h:mm tt","yyyy-MM-dd hh:mm tt",
					"yyyy-MM-dd H:mm","yyyy-MM-dd HH:mm","dd-MMM-yy h:mm tt","dd-MMM-yy hh:mm tt","dd-MMM-yy H:mm","dd-MMM-yy HH:mm",
					"M/d/yyyy h:mm:ss tt","M/d/yyyy hh:mm:ss tt","M/d/yyyy H:mm:ss",
					"M/d/yyyy HH:mm:ss","M/d/yy h:mm:ss tt","M/d/yy hh:mm:ss tt","M/d/yy H:mm:ss","M/d/yy HH:mm:ss",
					"MM/dd/yy h:mm:ss tt","MM/dd/yy hh:mm:ss tt","MM/dd/yy H:mm:ss","MM/dd/yy HH:mm:ss","MM/dd/yyyy h:mm:ss tt",
					"MM/dd/yyyy hh:mm:ss tt","MM/dd/yyyy H:mm:ss","MM/dd/yyyy HH:mm:ss","yy/MM/dd h:mm:ss tt",
					"yy/MM/dd hh:mm:ss tt","yy/MM/dd H:mm:ss","yy/MM/dd HH:mm:ss","yyyy-MM-dd h:mm:ss tt","yyyy-MM-dd hh:mm:ss tt",
					"yyyy-MM-dd H:mm:ss","yyyy-MM-dd HH:mm:ss","dd-MMM-yy h:mm:ss tt","dd-MMM-yy hh:mm:ss tt","dd-MMM-yy H:mm:ss",
					"dd-MMM-yy HH:mm:ss","MMMM dd","MMMM dd","yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK","yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK",
					"ddd, dd MMM yyyy HH':'mm':'ss 'GMT'","ddd, dd MMM yyyy HH':'mm':'ss 'GMT'","yyyy'-'MM'-'dd'T'HH':'mm':'ss",
					"h:mm tt","hh:mm tt","H:mm","HH:mm","h:mm:ss tt","hh:mm:ss tt","H:mm:ss","HH:mm:ss",
					"yyyy'-'MM'-'dd HH':'mm':'ss'Z'","dddd, MMMM d, yyyy h:mm:ss tt","dddd, MMMM d, yyyy hh:mm:ss tt","dddd, MMMM d, yyyy H:mm:ss",
					"dddd, MMMM d, yyyy HH:mm:ss","dddd, MMMM dd, yyyy h:mm:ss tt",
					"dddd, MMMM dd, yyyy hh:mm:ss tt","dddd, MMMM dd, yyyy H:mm:ss","dddd, MMMM dd, yyyy HH:mm:ss","MMMM dd, yyyy h:mm:ss tt",
					"MMMM dd, yyyy hh:mm:ss tt","MMMM dd, yyyy H:mm:ss","MMMM dd, yyyy HH:mm:ss","dddd, dd MMMM, yyyy h:mm:ss tt",
					"dddd, dd MMMM, yyyy hh:mm:ss tt","dddd, dd MMMM, yyyy H:mm:ss", "dddd, dd MMMM, yyyy HH:mm:ss","dd MMMM, yyyy h:mm:ss tt",
					"dd MMMM, yyyy hh:mm:ss tt","dd MMMM, yyyy H:mm:ss", 
					"dd MMMM, yyyy HH:mm:ss", "MMMM yyyy", "MMMM, yyyy", "MMMM yyyy","MMMM, yyyy"};
				
					
					string myString;
					DateTime dateFortmated;					

					//Having a try catch in here was fucking our results!!!  Cause I had to return something so I returned datetime.price.
					dateFortmated = DateTime.ParseExact(s, formats, new CultureInfo("en-GB"), DateTimeStyles.None);
					return dateFortmated;	
				
		
						
		
			}
				
		
		

		}
		private static string ReturnOldSchoolSym(string NewSchoolSym)
		{
				
			//Need to change certain signals since they use old school pit symbols.
			
			
			string xSym = NewSchoolSym;
			
				if(NewSchoolSym.Equals("ZT"))
					xSym ="TU";
				if(NewSchoolSym.Equals("ZF"))
					xSym ="FV";
				if(NewSchoolSym.Equals("ZN"))
					xSym ="TY";					
				if(NewSchoolSym.Equals("ZB"))
					xSym ="US";			
				if(NewSchoolSym.Equals("UB"))				
					xSym ="UL";				
				//Grains
				if(NewSchoolSym.Equals("ZS"))
					xSym ="S";
				if(NewSchoolSym.Equals("ZC"))
					xSym ="C";
				if(NewSchoolSym.Equals("ZW"))
					xSym ="W";					
				if(NewSchoolSym.Equals("ZM"))
					xSym ="SM";			
				if(NewSchoolSym.Equals("ZL"))				
					xSym ="BO";	
			
				if(NewSchoolSym.Equals("ZR"))				
					xSym ="RR";					
				if(NewSchoolSym.Equals("ZO"))				
					xSym ="O";					
				//CATTLE	
				if(NewSchoolSym.Equals("LE"))				
					xSym ="LC";	
				if(NewSchoolSym.Equals("HE"))				
					xSym ="LN";					
				 //FX
				if(NewSchoolSym.Equals("6E"))				
					xSym ="EC";	
				if(NewSchoolSym.Equals("6B"))				
					xSym ="BP";	
				if(NewSchoolSym.Equals("6J"))				
					xSym ="JY";	
				if(NewSchoolSym.Equals("6C"))				
					xSym ="CD";	
				
				if(NewSchoolSym.Equals("6M"))				
					xSym ="MP";	
				
				if(NewSchoolSym.Equals("6A"))				
					xSym ="AD";	
				
				if(NewSchoolSym.Equals("6S"))				
					xSym ="SF";	
				
				if(NewSchoolSym.Equals("6N"))				
					xSym ="NE";					
		
				return xSym;
				
			}
				
		
		public static void CreateCheckFolder()
		{			
			if(!Directory.Exists(NinjaTrader.Core.Globals.UserDataDir+"APData\\"))
			{
				NinjaTrader.Code.Output.Process("Creating APData Folder", PrintTo.OutputTab1);
				Directory.CreateDirectory(NinjaTrader.Core.Globals.UserDataDir+"APData\\");
			}
			if(!Directory.Exists(NinjaTrader.Core.Globals.UserDataDir+"APData\\"+"COT\\" ))
			{
				NinjaTrader.Code.Output.Process("Creating COT Folder", PrintTo.OutputTab1);
				Directory.CreateDirectory(NinjaTrader.Core.Globals.UserDataDir+"APData\\"+"COT\\");
			}	
			if(!Directory.Exists(NinjaTrader.Core.Globals.UserDataDir+"APData\\"+"COT\\"+"OI\\" ))
			{
				NinjaTrader.Code.Output.Process("Creating OI Folder", PrintTo.OutputTab1);
				Directory.CreateDirectory(NinjaTrader.Core.Globals.UserDataDir+"APData\\"+"COT\\"+"OI\\");
			}	
		
			
		}
		
		
		[NinjaScriptProperty]
		[Display(Name = "QuandlAPIKey", GroupName = "Parameters", Order = 0)]
		public string QuandlAPIKey
		{ get; set; }
		
		
		private static List<COTData_Obj> MyCotDataList;
		public class COTData_Obj  //Create Custom Object, an object with 2 objects inside..  //An object, with 2 objects inside.
		{
			public string Symbol {get; set;}
			public DateTime Time {get; set;}
			public double OI {get; set;}

		}
		

	}}




























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.DevDailyOpenInterestQuandl[] cacheDevDailyOpenInterestQuandl;
		public Suri.dev.DevDailyOpenInterestQuandl DevDailyOpenInterestQuandl(string quandlAPIKey)
		{
			return DevDailyOpenInterestQuandl(Input, quandlAPIKey);
		}

		public Suri.dev.DevDailyOpenInterestQuandl DevDailyOpenInterestQuandl(ISeries<double> input, string quandlAPIKey)
		{
			if (cacheDevDailyOpenInterestQuandl != null)
				for (int idx = 0; idx < cacheDevDailyOpenInterestQuandl.Length; idx++)
					if (cacheDevDailyOpenInterestQuandl[idx] != null && cacheDevDailyOpenInterestQuandl[idx].QuandlAPIKey == quandlAPIKey && cacheDevDailyOpenInterestQuandl[idx].EqualsInput(input))
						return cacheDevDailyOpenInterestQuandl[idx];
			return CacheIndicator<Suri.dev.DevDailyOpenInterestQuandl>(new Suri.dev.DevDailyOpenInterestQuandl(){ QuandlAPIKey = quandlAPIKey }, input, ref cacheDevDailyOpenInterestQuandl);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevDailyOpenInterestQuandl DevDailyOpenInterestQuandl(string quandlAPIKey)
		{
			return indicator.DevDailyOpenInterestQuandl(Input, quandlAPIKey);
		}

		public Indicators.Suri.dev.DevDailyOpenInterestQuandl DevDailyOpenInterestQuandl(ISeries<double> input , string quandlAPIKey)
		{
			return indicator.DevDailyOpenInterestQuandl(input, quandlAPIKey);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevDailyOpenInterestQuandl DevDailyOpenInterestQuandl(string quandlAPIKey)
		{
			return indicator.DevDailyOpenInterestQuandl(Input, quandlAPIKey);
		}

		public Indicators.Suri.dev.DevDailyOpenInterestQuandl DevDailyOpenInterestQuandl(ISeries<double> input , string quandlAPIKey)
		{
			return indicator.DevDailyOpenInterestQuandl(input, quandlAPIKey);
		}
	}
}

#endregion
