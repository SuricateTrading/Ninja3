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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri
{
	public class COT2 : Indicator
	{
		private CotReport cotReportCommShort;
		
		private double lastMax;
		private int lastMaxBar;
		private double lastMin;
		private int lastMinBar;
		private double linie75;
		private double linie50;
		private double linie25;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "COT 2";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				COT2Periode					= 1011;
				COT2LinienSchieben			= true;
				ShowMaxMin					= false;
				linientyp					= LineType.Dynamic;
				
				cotReportCommShort			= new CotReport { ReportType = CotReportType.Futures, Field = CotReportField.CommercialShort };
			
				AddPlot(Brushes.White, "CommShort");
				AddPlot(Brushes.Red, "75%");
				AddPlot(Brushes.Green, "25%");
				AddPlot(Brushes.Cyan, "100%");
				AddPlot(Brushes.Cyan, "0%");
				AddPlot(Brushes.Gray, "50%");
				AddPlot(Brushes.DarkRed, "Test"); // 6
			}
		}

		protected override void OnBarUpdate()
		{

			
			//COT Data
			double valueCommShort = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[0]);
			if (!double.IsNaN(valueCommShort)) // returns NaN if Instrument/Report combination is not valid.
					Values[0][0] = valueCommShort;
			
			if (CurrentBar > COT2Periode)
			{
				//Max und Min der Periode rausfinden
				if (CurrentBar == 0)
				{
					lastMax = 0;
					lastMaxBar = 0;
					lastMin = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[0]);
					lastMinBar = 0;
				}
				
				if (linientyp == LineType.Dynamic)
				{						
					#region Max
					if (CurrentBar - lastMaxBar >= COT2Periode)
					{					
						double tempMax = valueCommShort;
						int tempMaxBar = CurrentBar;
						for (int i = CurrentBar; i > CurrentBar - COT2Periode; i--)
						{
							double tempCommShort = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[CurrentBar - i]);
							if (tempCommShort > tempMax)
							{
								tempMax = tempCommShort;
								tempMaxBar = i;
							}
							
						}
						lastMax = tempMax;
						lastMaxBar = tempMaxBar;
					}			
					else
					{
						if (valueCommShort > lastMax)
						{
							lastMax = valueCommShort;						
							lastMaxBar = CurrentBar;
						}
						
					}		
					
					#endregion Max
					
					#region Min
					if (CurrentBar - lastMinBar >= COT2Periode)
					{									
						double tempMin = valueCommShort;
						int tempMinBar = CurrentBar;
						for (int i = CurrentBar; i > CurrentBar-COT2Periode; i--)
						{
							double tempCommShort = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[CurrentBar - i]);
							if (tempCommShort < tempMin)
							{
								tempMin = tempCommShort;
								tempMinBar = i;						
							}
							
						}
						lastMin = tempMin;
						lastMinBar = tempMinBar;
						
						Draw.Text(this, "LastMinTag", "LastMin", 0, lastMin, Brushes.White);
					}			
					else
					{
						if (valueCommShort < lastMin)
						{
							lastMin = valueCommShort;						
							lastMinBar = CurrentBar;
						}
					}
					


					#endregion Min	
					
					#region linien75und50und25
					
					linie75 = (lastMax - lastMin) * 0.75 + lastMin;
					Values[1][0] = linie75;
					linie50 = (lastMax - lastMin) * 0.5 + lastMin;
					Values[5][0] = linie50;
					linie25 = (lastMax - lastMin) * 0.25 + lastMin;
					Values[2][0] = linie25;
					
					#endregion linien75und50und25
					
					#region LinienSchieben
					
					if (COT2LinienSchieben)
					{
						double tempCommShortPre = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[COT2Periode + 1]);
						double xHighTemp = 0; // Extremwert-Hoch aktuellen "Berges"
						double xHighMin = 0; // niedrigster Extremwert-Hoch
						double xLowTemp = 0; // Extremwert-Tief aktuellen "Berges"
						double xLowMax = 0; // höchster Extremwert-Tief
						for (int i = COT2Periode;  i > 0; i--)
						{
							double tempCommShort = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[i]);
							
							
							#region linie75v
							if (tempCommShort > linie75 && tempCommShortPre < linie75) //Punkt finden wo 75% Linie nach oben überschritten wird
							{
								xHighTemp = tempCommShort;
							}
							if (tempCommShort > linie75) //nach Max Wert Suchen solange über 75%
							{
								if (tempCommShort > xHighTemp)
									xHighTemp = tempCommShort;
							}
							if (tempCommShort < linie75 && tempCommShortPre > linie75) //Punkt finden wo 75% Linie nach unten überschritten wird
							{
								if (xHighMin == 0) // wenn erster Wert dann abspeichern
									xHighMin = xHighTemp;
								else
								{
									if (xHighMin > xHighTemp)	// abspeichern wenn neuster niedrigster Extremwert-Hoch												
										xHighMin = xHighTemp;
								}
							}
							#endregion
							
							#region linie25v
							if (tempCommShort < linie25 && tempCommShortPre > linie25) //Punkt finden wo 25% Linie nach unten überschritten wird
							{
								xLowTemp = tempCommShort;
							}
							if (tempCommShort < linie25) //nach Min Wert Suchen solange unter 25%
							{
								if (tempCommShort < xLowTemp)
									xLowTemp = tempCommShort;
							}
							if (tempCommShort > linie25 && tempCommShortPre < linie25) //Punkt finden wo 75% Linie nach unten überschritten wird
							{
								if (xLowMax == 0) // wenn erster Wert dann abspeichern
									xLowMax = xLowTemp;
								else
								{
									if (xLowMax < xLowTemp)	// abspeichern wenn neuster höchster Extremwert-Tief												
										xLowMax = xLowTemp;
								}
							}									
							#endregion
							tempCommShortPre = tempCommShort;
						}	
						double linie75v = xHighMin; // verschobene linie75 definieren
						double linie25v = xLowMax; // verschobene linie25 definieren
						
						for (int i = lastMaxBar; i >= 0; i--)
						{
							Values[1][i] = linie75v;
							Values[2][i] = linie25v;
						}								
					}					
					
					#endregion LinienSchieben
					
					if (ShowMaxMin)
						{
							Values[3][0] = lastMin;
							Values[4][0] = lastMax;	
						}
				
				}
				else
				{
					#region LineType.Last
					
					if (linientyp == LineType.Last)
					{
						if (CurrentBar >= Count-2)
						{
							//Max und Min finden
							lastMax = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[0]);
							for (int i = CurrentBar; i >= CurrentBar - COT2Periode; i--)
							{
								double tempCommShort = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[CurrentBar - i]);
								if (tempCommShort > lastMax)
								{
									lastMax = tempCommShort;
								}	
							}
							
							lastMin = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[0]);
							for (int i = CurrentBar; i >= CurrentBar - COT2Periode; i--)
							{
								double tempCommShort = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[CurrentBar - i]);
								if (tempCommShort < lastMin)
								{
									lastMin = tempCommShort;
								}	
							}
							
							#region linienZeichnen25,50,75 
							for (int i = CurrentBar; i >=0; i--)
							{
								linie75 = (lastMax - lastMin) * 0.75 + lastMin;
								Values[1][i] = linie75;
								linie50 = (lastMax - lastMin) * 0.5 + lastMin;
								Values[5][i] = linie50;
								linie25 = (lastMax - lastMin) * 0.25 + lastMin;
								Values[2][i] = linie25;
								if (ShowMaxMin)
									{
										Values[3][i] = lastMin;
										Values[4][i] = lastMax;	
									}	

							}
							#endregion linienZeichnen25,50,75 
														
							
							#region linienSchieben							
							
							if (COT2LinienSchieben)
							{
								double tempCommShortPre = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[COT2Periode + 1]);
								double xHighTemp = 0; // Extremwert-Hoch aktuellen "Berges"
								double xHighMin = 0; // niedrigster Extremwert-Hoch
								double xLowTemp = 0; // Extremwert-Tief aktuellen "Berges"
								double xLowMax = 0; // höchster Extremwert-Tief
								for (int i = COT2Periode;  i > 0; i--)
								{
									double tempCommShort = cotReportCommShort.Calculate(Instrument.MasterInstrument.Name, Time[i]);
									
									
									#region linie75v
									if (tempCommShort > linie75 && tempCommShortPre < linie75) //Punkt finden wo 75% Linie nach oben überschritten wird
									{
										xHighTemp = tempCommShort;
									}
									if (tempCommShort > linie75) //nach Max Wert Suchen solange über 75%
									{
										if (tempCommShort > xHighTemp)
											xHighTemp = tempCommShort;
									}
									if (tempCommShort < linie75 && tempCommShortPre > linie75) //Punkt finden wo 75% Linie nach unten überschritten wird
									{
										if (xHighMin == 0) // wenn erster Wert dann abspeichern
											xHighMin = xHighTemp;
										else
										{
											if (xHighMin > xHighTemp)	// abspeichern wenn neuster niedrigster Extremwert-Hoch												
												xHighMin = xHighTemp;
										}
									}
									#endregion
									
									#region linie25v
									if (tempCommShort < linie25 && tempCommShortPre > linie25) //Punkt finden wo 25% Linie nach unten überschritten wird
									{
										xLowTemp = tempCommShort;
									}
									if (tempCommShort < linie25) //nach Min Wert Suchen solange unter 25%
									{
										if (tempCommShort < xLowTemp)
											xLowTemp = tempCommShort;
									}
									if (tempCommShort > linie25 && tempCommShortPre < linie25) //Punkt finden wo 75% Linie nach unten überschritten wird
									{
										if (xLowMax == 0) // wenn erster Wert dann abspeichern
											xLowMax = xLowTemp;
										else
										{
											if (xLowMax < xLowTemp)	// abspeichern wenn neuster höchster Extremwert-Tief												
												xLowMax = xLowTemp;
										}
									}									
									#endregion
									tempCommShortPre = tempCommShort;
								}	
								double linie75v = xHighMin; // verschobene linie75 definieren
								double linie25v = xLowMax; // verschobene linie25 definieren
								
								for (int i = CurrentBar; i >=0; i--)
								{
									Values[1][i] = linie75v;
									Values[2][i] = linie25v;
								}								
							}
							
							#endregion linienSchieben
							

							
							
						}
					}
					#endregion LineType.Last
				}
			}
			
										
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="COT2Periode", Description="Periode in Bars", Order=1, GroupName="Parameter")]
		public int COT2Periode
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ShowMaxMin", Description="Max Min Linien Zeigen", Order=2, GroupName="Parameter")]
		public bool ShowMaxMin
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="COT2LinienSchieben", Description="COT2", Order=3, GroupName="Parameter")]
		public bool COT2LinienSchieben
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Linientyp", Description="Linien am letzen COT-Wert oder dynamisch in der Vergangenheit anzeigen", Order=4, GroupName="Parameter")]
		public LineType linientyp
		{ get; set; }
		#endregion

	}
}

public enum LineType
{
	Dynamic,
	Last
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.COT2[] cacheCOT2;
		public Suri.COT2 COT2(int cOT2Periode, bool showMaxMin, bool cOT2LinienSchieben, LineType linientyp)
		{
			return COT2(Input, cOT2Periode, showMaxMin, cOT2LinienSchieben, linientyp);
		}

		public Suri.COT2 COT2(ISeries<double> input, int cOT2Periode, bool showMaxMin, bool cOT2LinienSchieben, LineType linientyp)
		{
			if (cacheCOT2 != null)
				for (int idx = 0; idx < cacheCOT2.Length; idx++)
					if (cacheCOT2[idx] != null && cacheCOT2[idx].COT2Periode == cOT2Periode && cacheCOT2[idx].ShowMaxMin == showMaxMin && cacheCOT2[idx].COT2LinienSchieben == cOT2LinienSchieben && cacheCOT2[idx].linientyp == linientyp && cacheCOT2[idx].EqualsInput(input))
						return cacheCOT2[idx];
			return CacheIndicator<Suri.COT2>(new Suri.COT2(){ COT2Periode = cOT2Periode, ShowMaxMin = showMaxMin, COT2LinienSchieben = cOT2LinienSchieben, linientyp = linientyp }, input, ref cacheCOT2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.COT2 COT2(int cOT2Periode, bool showMaxMin, bool cOT2LinienSchieben, LineType linientyp)
		{
			return indicator.COT2(Input, cOT2Periode, showMaxMin, cOT2LinienSchieben, linientyp);
		}

		public Indicators.Suri.COT2 COT2(ISeries<double> input , int cOT2Periode, bool showMaxMin, bool cOT2LinienSchieben, LineType linientyp)
		{
			return indicator.COT2(input, cOT2Periode, showMaxMin, cOT2LinienSchieben, linientyp);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.COT2 COT2(int cOT2Periode, bool showMaxMin, bool cOT2LinienSchieben, LineType linientyp)
		{
			return indicator.COT2(Input, cOT2Periode, showMaxMin, cOT2LinienSchieben, linientyp);
		}

		public Indicators.Suri.COT2 COT2(ISeries<double> input , int cOT2Periode, bool showMaxMin, bool cOT2LinienSchieben, LineType linientyp)
		{
			return indicator.COT2(input, cOT2Periode, showMaxMin, cOT2LinienSchieben, linientyp);
		}
	}
}

#endregion
