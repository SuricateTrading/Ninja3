#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
#endregion



#region NOTES:
/*

I've written this script to be able to quickly see basic chart/bar info
without having to use the Data Box or clicking the middle mouse button/wheel.

Some basic functionality include:
- The option to select which fields you'd like to see/hide.
	- Bar Time/Date
	- Open/High/Low/Close values
	- Ticks per bar (for historical values, you will need to enable Tick Replay)
	- Volume per bar
	- Volume per Tick average per bar (for historical values, you will need to enable Tick Replay)
	- Bid
	- Ask
	- Spread
- The options to change the font, font size, and text colors.
- When the "Visible" property checkbox in the "Indicators" window is unchecked,
	even though this indicator is not rendered like a typical indicator,
	unchecking will still allow this indicator to be hidden from view.
	
---

Aside from the basics, I've tried to make this script as resource friendly as possible
by using the following:


Rather than updating values directly within OnMouseMove(), 
I've used a Timer to regularly trigger the UpdateValues() method
which then processes the updates for values, colors, etc.

This way, we can include the following two options to disable/enable the Timer/updates as needed:

First, disable the Timer if the mouse is no longer over the ChartControl.
The Timer is re-enabled when the mouse re-enters the ChartControl.

Second, disable the Timer if the chart window is deactivated (loses focus).
The Timer is re-enabled when the chart window is activated again.

You can use any combination of the two options.

If both options are enabled, the Timer will remain disabled if the chart window
is reactivated while the mouse still remains out of the ChartControl.
(For example, you click on the upper menu area, or the blank area at the bottom of the window)

---

You can manually change the speed/freqency at which this script updates values.

---

If you have "Use Last Visible Bar" checked and the last visible bar is not the absolute last bar on the chart 
the Timer will automatically be disabled for historical bars as they do not update values.
It will automatically be re-enabled when needed.

If you have "Use Last Visible Bar" unchecked or the last visible bar is the absolute last bar on the chart while checked, 
the script automatically checks to see if any new updates have been made in the last 30 seconds, 
if no new updates made in that time, the Timer will automatically shut off.
Again, it will automatically be re-enabled when needed.

---

If you hold the mouse over the same candle, the Timer will shut off under the following conditions:

- If you have "Use Last Visible Bar" checked 
	- If the last visible bar is not the absolute last bar on the chart,
		holding the mouse over the same candle will disable the Timer until the mouse is moved again.
	- If the last visible bar is the absolute last bar on the chart, 
		holding the mouse over the same candle will disable the Timer until there is an update in market data.

- If you have "Use Last Visible Bar" unchecked
	- Holding the mouse over the same candle will disable the Timer until there is an update in market data.

---

"Thank you" to Chelsea Bell of the NT support team for his "ChartCustomToolBarExample" script.
(https://ninjatrader.com/support/forum/showthread.php?p=499327)
A good portion of code for this indicator was taken from Chelsea's script and modified as needed.


UPDATES:

	05-07-2018 - Original version

	06/24/2020 - Added Range and Volume/Range as requested

*/
#endregion



namespace NinjaTrader.NinjaScript.Indicators.TG
{



    public class BarInfo : Indicator
	{
		
		
		#region class variables
		private	Point			mousePosition;
		
		private	int				barIndex					= 0, 
								prevBar						= 0,
								lastBar						= 0;
		
		private double			valOpen,   prevOpen, 
								valHigh,   prevHigh, 
								valLow,    prevLow, 
								valClose,  prevClose, 
								valRange,  prevRange, 
								valVPR,    prevVPR, 
								valVPT,    prevVPT, 
								valBid,	   prevBid, 
								valAsk,    prevAsk, 
								valSpread, prevSpread;
		
		private double			valVolume, prevVolume;						//	the Volume Iseries is of type double, not long
		
		private int				valTicks,  prevTicks;
		
		private DateTime		valTime;
		
		private const string	strOpen						= "O: ", 
								strHigh						= "H: ", 
								strLow						= "L: ", 
								strClose					= "C: ", 
								strRange					= "R: ", 
								strVPR					    = "V/R: ", 
								strVolume					= "V: ", 
								strTicks					= "T: ", 
								strVPT						= "V/T: ", 
								strTime						= "Time: ", 
								strBid						= "B: ", 
								strAsk						= "A: ", 
								strSpread					= "S: ";
		
		private double			valueTextWidth;
		
		private Chart 			chartWindow;
		private Grid			chartGrid;
		private ChartControl	chartControl;
		private ChartBars		chartBars;
		
		private bool			panelActive, 
								mouseIsInChartControl;
		private TextBlock		mainTextBlock;
		
		private Series<int> 	TickCount;									//	tick counter as a custom data series
		private Series<double> 	VolumePerTick;								//	volume per tick average as a custom data series
		
		private Timer 			myTimer;
		private TabItem			tabItem;
		private ChartTab		chartTab;
		
		private	double			timeLimit					= 30000;		//	in milliseconds (30,000 = 30 seconds)
		private	double			staleDataCounter			= 0;
		private	double			staleClose;
		private	DateTime		staleTime;
		private	int				staleBar					= 0;
		#endregion
		
		
		#region OnStateChange()
		protected override void OnStateChange()
		{
			#region State.SetDefaults
			if (State == State.SetDefaults)
			{
				#region base properties
				Description									= String.Format(@"This indicator acts as a mini Data Box by keeping basic info visible on your chart at all times.{0}{0}NOTE:{0}To get historical values of Tick Count and Average Volume Per Tick, Tick Replay must be enabled.{0}{0}This script was Written by ""Gubbar924""{0}http://ninjatrader.com/support/forum/member.php?u=30178", Environment.NewLine);
				Name										= "BarInfo";
				
				Calculate									= Calculate.OnEachTick;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				
				IsChartOnly									= true;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				PaintPriceMarkers							= false;
				IsSuspendedWhileInactive					= true;
				IsAutoScale									= false;
				#endregion
				
				#region inputs
				disableOnMouseLeave							= false;
				disableOnDeactivate							= false;
				timerInterval								= 500;
				allowHistorical								= true;
				
				openCompareToPrevious						= false;
				closeCompareToPrevious						= false;
				
				showTime									= true;
				showOHLC									= true;
				showRange									= false;
				showVPR										= false;
				showVolume									= true;
				showTicks									= false;
				showVPT										= true;
				showBid										= false;
				showAsk										= false;
				showSpread									= true;
				
				myFont		   								= new SimpleFont("Global User Interface", 12) { Size = 12, Italic = true, Bold = true, Family = new FontFamily("Global User Interface") };
				myTextWrapping								= TextWrapping.Wrap;
				upBrush										= Brushes.Green;
				dnBrush										= Brushes.Red;
				sdBrush										= Brushes.Blue;
				tmBrush										= Brushes.Black;
				lbBrush										= Brushes.DimGray;
				#endregion
			}
			#endregion
			
			#region State.DataLoaded
			else if (State == State.DataLoaded)
			{
				Name                                        = "";
				Calculate									= Calculate.OnEachTick;
				TickCount 									= new Series<int>(this);
				VolumePerTick 								= new Series<double>(this);
				
				if (myFont.Size < 16)
				{
					valueTextWidth							= myFont.Size * 6;
				}
				else if (myFont.Size >= 16 && myFont.Size < 21)
				{
					valueTextWidth							= myFont.Size * 5;
				}
				else if (myFont.Size >= 21 && myFont.Size < 30)
				{
					valueTextWidth							= myFont.Size * 3;
				}
				else
				{
					valueTextWidth							= myFont.Size * 2;
				}
				
				#region instantiate chart components
				chartControl 								= this.ChartControl;
				chartBars									= this.ChartBars;
				
				if(TickCount == null || VolumePerTick == null || chartControl == null || chartBars == null)
				{
					return;
				}
				
				chartWindow									= chartControl.OwnerChart as Chart;
				
				if(chartWindow == null)
				{
					return;
				}
				
				chartGrid									= chartWindow.MainTabControl.Parent as Grid;
				
				if(chartGrid == null)
				{
					return;
				}
				#endregion
			}
			#endregion
			
			#region State.Realtime
			else if (State == State.Realtime)
			{
				if(chartControl == null || Bars == null || chartBars == null || (State < State.Realtime))
				{
					return;
				}
			
				chartControl.Dispatcher.InvokeAsync((Action)(() =>
				{
					CreateWPFControls();
				}));
            }
			#endregion
			
			#region State.Terminated
			else if (State == State.Terminated)
			{
				if (VolumePerTick != null)
                {
                    VolumePerTick = null;
                }
				
				if (TickCount != null)
                {
                    TickCount = null;
                }
				
				if (chartTab != null)
                {
                    chartTab = null;
                }
				
				if (tabItem != null)
                {
                    tabItem = null;
                }
				
				if (chartBars != null)
                {
                    chartBars = null;
                }
				
                if (chartControl != null)
                {
					chartControl.Dispatcher.Invoke((Action)(() =>
					{
						DisposeWPFControls();
					}));
					
					chartControl = null;
                }
				
				if (chartGrid != null)
                {
                    chartGrid = null;
                }
				
				if (chartWindow != null)
                {
                    chartWindow = null;
                }
            }
			#endregion
		}
		#endregion
		
		
		#region OnMarketData()
		protected override void OnMarketData(MarketDataEventArgs dataUpdate)
		{
			try
			{
				//	if the timer is disabled due to no new updates within the predefined time limit
				//	new incoming market data (under the right conditions) will re-enable the timer
				
				
				if((State < State.Realtime) || (myTimer == null) || (barIndex != lastBar))
				{
					return;
				}
				
				//	just check these 2 things first, as they are the ones most likely to not meet their criteria.
				//	we continue only if: 
				//		- myTimer is NOT enabled, if it were enabled, it would call UpdateValues() on its own, so avoid redundant work.
				//		- we are pointing to the absolute last bar on the chart, if not, then let OnRender() handle it.
				if(!myTimer.Enabled && (barIndex == lastBar))
				{
					//	we add more work load only if the first 2 criteria are met
					chartWindow.Dispatcher.InvokeAsync((Action)(() =>
					{
						if(!(disableOnMouseLeave && !mouseIsInChartControl) && !(disableOnDeactivate && !chartWindow.IsActive))
						{
							if (dataUpdate.MarketDataType == MarketDataType.Last)
							{
								if((dataUpdate.Price != staleClose) || (dataUpdate.Time != staleTime))
								{
									staleDataCounter	= 0;
									UpdateValues();
								}
							}
						}
					}));
				}
			}
            catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
			}
		}
		#endregion
		
		
		#region OnBarUpdate()
		protected override void OnBarUpdate()
		{
			TickCount[0] 		= (!Bars.IsTickReplay && CurrentBar < lastBar) ? (BarsPeriod.BarsPeriodType == BarsPeriodType.Tick) ? BarsPeriod.Value : 0 : Bars.TickCount;
			VolumePerTick[0] 	= Volume[0] / TickCount[0];
		}
		#endregion
		
		
		#region OnRender()
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			//	When "Use Last Visible Bar" is checked, the timer is automatically disabled
			//	if the last visible bar is NOT the absolute last bar on the chart.
			//	However, we still need to retrieve info for that last visible bar one time.
			
			
			//	if (barIndex == lastBar) or !allowHistorical, we are pointing to the absolute last bar on the chart
			//	in that case, lets allow OnMarketData() to handle it, no need to repeat the work.
			if((State < State.Realtime) || (myTimer == null) || (barIndex == lastBar) || !allowHistorical)
			{
				return;
			}
			
			//	if mouse is outside of chartcontrol, point to the last visible bar
			//	else OnMouseMove() will tell us what bar we are pointing at/to.
			if(!mouseIsInChartControl)
			{
				PointToLastBar();
			}
			
			//	If myTimer was enabled, it would call UpdateValues() on its own, so avoid redundant work.
			//	Because we are not pointing to the absolute last bar on the chart and historical bars do not change values, 
			//	run UpdateValues() just once to reflect the values of the last visible bar.
			//	(staleBar changed because user scrolled the chart x-xis)
			if(!myTimer.Enabled && (barIndex != staleBar))
			{
				staleDataCounter	= 0;
				UpdateValues();
				staleBar			= barIndex;
			}
		}
		#endregion
		
		
		#region OnMouseEnter()
		private void OnMouseEnter(object sender, MouseEventArgs e)
		{
			try
            {
				mouseIsInChartControl				= true;
				
				if(myTimer != null)
				{
					if( (!disableOnDeactivate || (disableOnDeactivate && chartWindow.IsActive)) && (barIndex == lastBar) )
					{
						myTimer.Enabled				= true;
					}
				}
            }
            catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
			}
		}
		#endregion
		
		
		#region OnMouseMove()
		private void OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
				if(myTimer != null)
				{
					if(disableOnDeactivate && !chartWindow.IsActive)
					{
						myTimer.Enabled				= false;
						return;
					}
				}
				
				mouseIsInChartControl				= true;
				
				mousePosition 						= e.GetPosition(chartControl);
				mousePosition.X   					= ChartingExtensions.ConvertToHorizontalPixels(mousePosition.X, chartControl.PresentationSource);
				barIndex	   						= chartBars.GetBarIdxByX(chartControl, Convert.ToInt32(Math.Round(mousePosition.X, 0)));
				prevBar								= barIndex == 0 ? 0 : barIndex - 1;
				lastBar								= Bars.Count - (Calculate == Calculate.OnBarClose ? 2 : 1);
				
				//	in case end user makes changes to the chart that would reload ninjascript while still keeping the mouse over the chart
				//	(for example: changing the instrument from the instrument selector dropdown menu)
				if(myTimer != null)
				{
					if(!disableOnDeactivate || (disableOnDeactivate && chartWindow.IsActive))
					{
						myTimer.Enabled				= true;
					}
				}
            }
            catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
			}
        }
		#endregion
		
		
		#region OnMouseLeave()
		private void OnMouseLeave(object sender, MouseEventArgs e)
		{
			try
            {
				mouseIsInChartControl				= false;
				UpdateValues();
				
				if(myTimer != null)
				{
					myTimer.Enabled					= (barIndex != lastBar) ? false : disableOnMouseLeave ? false : true;
				}
            }
            catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
			}
		}
		#endregion
		
		
		#region OnWindowActivated()
		private void OnWindowActivated(object sender, EventArgs e)
		{
			try
            {
				if(myTimer != null)
				{
					myTimer.Enabled					= (disableOnMouseLeave && !mouseIsInChartControl) ? false : (barIndex != lastBar) ? false :  true;
				}
            }
            catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
			}
		}
		#endregion
		
		
		#region OnWindowDeactivated()
		private void OnWindowDeactivated(object sender, EventArgs e)
		{
			try
            {
				if(myTimer != null && disableOnDeactivate)
				{
//					if(barIndex == lastBar)
//					{
//						UpdateValues();
//					}
					
					myTimer.Enabled					= false;
				}
            }
            catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
			}
		}
		#endregion
		
		
		#region TimerEventProcessor()
		private void TimerEventProcessor(object source, ElapsedEventArgs e)
		{
			try
			{
				if(State < State.Realtime)
				{
					return;
				}
				
				if (!IsVisible)
				{
					if(myTimer != null)
					{
						myTimer.Enabled				= false;
					}
					return;
				}
				
				UpdateValues();
			}
			catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
			}
		}
		#endregion
		
		
		#region UpdateValues()
		private void UpdateValues()
        {
			try
			{
				if(State < State.Realtime)
				{
					return;
				}
			
				chartControl.Dispatcher.InvokeAsync((Action)(() =>
				{
					if(!mouseIsInChartControl)
					{
						PointToLastBar();
					}
									
					#region update TextBlock mainTextBlock
					mainTextBlock.Inlines.Clear();
					
					#region Time
					if(showTime)
					{
						valTime				= Time.GetValueAt(barIndex);
						string strValTime	= valTime.ToString("dddd, MM/dd/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.CurrentCulture);
						
						InlineUIContainer timeIUICont = new InlineUIContainer();
						TextBlock timeTextBlock = new TextBlock();
						timeTextBlock.Inlines.Add(new Run(strTime) { Foreground = lbBrush });
						timeTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValTime,
										Foreground = tmBrush,
										Width = WidthOfString(strValTime, 3)
		                            }
		                });
						timeIUICont.Child = timeTextBlock;
						mainTextBlock.Inlines.Add(timeIUICont);
					}
					#endregion
					
					#region OHLC
					if(showOHLC)
					{
						#region Open
						valOpen				= Open.GetValueAt(barIndex);
						prevOpen			= Open.GetValueAt(prevBar);
						string strValOpen	= Bars.Instrument.MasterInstrument.FormatPrice(valOpen);
						
						InlineUIContainer openIUICont = new InlineUIContainer();
						TextBlock openTextBlock = new TextBlock();
						openTextBlock.Inlines.Add(new Run(strOpen) { Foreground = lbBrush });
						openTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValOpen,
										Foreground = !openCompareToPrevious ? (valClose > valOpen ? upBrush : valClose < valOpen ? dnBrush : sdBrush) : (valOpen > prevOpen ? upBrush : valOpen < prevOpen ? dnBrush : sdBrush),
										Width = WidthOfString(strValOpen)
		                            }
		                });
						openIUICont.Child = openTextBlock;
						mainTextBlock.Inlines.Add(openIUICont);
						#endregion
						
						#region High
						valHigh				= High.GetValueAt(barIndex);
						prevHigh			= High.GetValueAt(prevBar);
						string strValHigh	= Bars.Instrument.MasterInstrument.FormatPrice(valHigh);
						
						InlineUIContainer highIUICont = new InlineUIContainer();
						TextBlock highTextBlock = new TextBlock();
						highTextBlock.Inlines.Add(new Run(strHigh) { Foreground = lbBrush });
						highTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValHigh,
										Foreground = valHigh > prevHigh ? upBrush : valHigh < prevHigh ? dnBrush : sdBrush,
										Width = WidthOfString(strValHigh)
		                            }
		                });
						highIUICont.Child = highTextBlock;
						mainTextBlock.Inlines.Add(highIUICont);
						#endregion
						
						#region Low
						valLow				= Low.GetValueAt(barIndex);
						prevLow				= Low.GetValueAt(prevBar);
						string strValLow	= Bars.Instrument.MasterInstrument.FormatPrice(valLow);
						
						InlineUIContainer lowIUICont = new InlineUIContainer();
						TextBlock lowTextBlock = new TextBlock();
						lowTextBlock.Inlines.Add(new Run(strLow) { Foreground = lbBrush });
						lowTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValLow,
										Foreground = valLow > prevLow ? upBrush : valLow < prevLow ? dnBrush : sdBrush,
										Width = WidthOfString(strValLow)
		                            }
		                });
						lowIUICont.Child = lowTextBlock;
						mainTextBlock.Inlines.Add(lowIUICont);
						#endregion
						
						#region Close
						valClose			= Close.GetValueAt(barIndex);
						prevClose			= Close.GetValueAt(prevBar);
						string strValClose	= Bars.Instrument.MasterInstrument.FormatPrice(valClose);
						
						InlineUIContainer closeIUICont = new InlineUIContainer();
						TextBlock closeTextBlock = new TextBlock();
						closeTextBlock.Inlines.Add(new Run(strClose) { Foreground = lbBrush });
						closeTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValClose,
										Foreground = !closeCompareToPrevious ? (valClose > valOpen ? upBrush : valClose < valOpen ? dnBrush : sdBrush) : (valClose > prevClose ? upBrush : valClose < prevClose ? dnBrush : sdBrush),
										Width = WidthOfString(strValClose)
		                            }
		                });
						closeIUICont.Child = closeTextBlock;
						mainTextBlock.Inlines.Add(closeIUICont);
						#endregion
					}
					#endregion
					
					#region Range
					if(showRange)
					{
						valRange			= High.GetValueAt(barIndex) - Low.GetValueAt(barIndex);
						prevRange			= High.GetValueAt(prevBar)  - Low.GetValueAt(prevBar);
						string strValRange	= Instrument.MasterInstrument.FormatPrice(valRange);
						
						InlineUIContainer 	rangeIUICont  	= new InlineUIContainer();
						TextBlock 			rangeTextBlock 	= new TextBlock();
						rangeTextBlock.Inlines.Add(new Run(strRange) { Foreground = lbBrush });
						rangeTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValRange,
										Foreground = valRange > prevRange ? upBrush : valRange < prevRange ? dnBrush : sdBrush,
										Width = WidthOfString(strValRange)
		                            }
		                });
						rangeIUICont.Child = rangeTextBlock;
						mainTextBlock.Inlines.Add(rangeIUICont);
					}
					#endregion
					
					#region VPR
					if(showVPR)
					{
						if(!showRange)
						{
							valRange		= High.GetValueAt(barIndex) - Low.GetValueAt(barIndex);
							prevRange		= High.GetValueAt(prevBar)  - Low.GetValueAt(prevBar);
						}
						
						valVPR				= Volume.GetValueAt(barIndex) / valRange;
						prevVPR				= Volume.GetValueAt(prevBar) / prevRange;
						string strValVPR	= valVPR.ToString("0.00");
						
						InlineUIContainer 	vprIUICont  	= new InlineUIContainer();
						TextBlock 			vprTextBlock 	= new TextBlock();
						vprTextBlock.Inlines.Add(new Run(strVPR) { Foreground = lbBrush });
						vprTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValVPR,
										Foreground = valVPR > prevVPR ? upBrush : valVPR < prevVPR ? dnBrush : sdBrush,
										Width = WidthOfString(strValVPR)
		                            }
		                });
						vprIUICont.Child = vprTextBlock;
						mainTextBlock.Inlines.Add(vprIUICont);
					}
					#endregion
					
					#region Volume
					if(showVolume)
					{
						valVolume			= Volume.GetValueAt(barIndex);
						prevVolume			= Volume.GetValueAt(prevBar);
						string strValVolume	= valVolume.ToString();
						
						InlineUIContainer volIUICont = new InlineUIContainer();
						TextBlock volTextBlock = new TextBlock();
						volTextBlock.Inlines.Add(new Run(strVolume) { Foreground = lbBrush });
						volTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValVolume,
										Foreground = valVolume > prevVolume ? upBrush : valVolume < prevVolume ? dnBrush : sdBrush,
										Width = WidthOfString(strValVolume)
		                            }
		                });
						volIUICont.Child = volTextBlock;
						mainTextBlock.Inlines.Add(volIUICont);
					}
					#endregion
					
					#region Ticks
					if(showTicks)
					{
						valTicks			= TickCount.GetValueAt(barIndex);
						prevTicks			= TickCount.GetValueAt(prevBar);
						string strValTicks	= valTicks.ToString();
						
						InlineUIContainer ticksIUICont = new InlineUIContainer();
						TextBlock ticksTextBlock = new TextBlock();
						ticksTextBlock.Inlines.Add(new Run(strTicks) { Foreground = lbBrush });
						ticksTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValTicks,
										Foreground = valTicks > prevTicks ? upBrush : valTicks < prevTicks ? dnBrush : sdBrush,
										Width = WidthOfString(strValTicks)
		                            }
		                });
						ticksIUICont.Child = ticksTextBlock;
						mainTextBlock.Inlines.Add(ticksIUICont);
					}
					#endregion
					
					#region VPT
					if(showVPT)
					{
						valVPT				= VolumePerTick.GetValueAt(barIndex);
						prevVPT				= VolumePerTick.GetValueAt(prevBar);
						string strValVPT	= valVPT.ToString("0.00");
						
						InlineUIContainer vptIUICont = new InlineUIContainer();
						TextBlock vptTextBlock = new TextBlock();
						vptTextBlock.Inlines.Add(new Run(strVPT) { Foreground = lbBrush });
						vptTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValVPT,
										Foreground = valVPT > prevVPT ? upBrush : valVPT < prevVPT ? dnBrush : sdBrush,
										Width = WidthOfString(strValVPT, .75)
		                            }
		                });
						vptIUICont.Child = vptTextBlock;
						mainTextBlock.Inlines.Add(vptIUICont);
					}
					#endregion
					
					#region Bid
					if(showBid)
					{
						valBid				= barIndex == lastBar ? GetCurrentBid() : Bars.GetBid(barIndex);
						prevBid				= Bars.GetBid(prevBar);
						string strValBid	= Instrument.MasterInstrument.FormatPrice(valBid);
						
						InlineUIContainer bidIUICont = new InlineUIContainer();
						TextBlock bidTextBlock = new TextBlock();
						bidTextBlock.Inlines.Add(new Run(strBid) { Foreground = lbBrush });
						bidTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValBid,
										Foreground = valBid > prevBid ? upBrush : valBid < prevBid ? dnBrush : sdBrush,
										Width = WidthOfString(strValBid)
		                            }
		                });
						bidIUICont.Child = bidTextBlock;
						mainTextBlock.Inlines.Add(bidIUICont);
					}
					#endregion
					
					#region Ask
					if(showAsk)
					{
						valAsk				= barIndex == lastBar ? GetCurrentAsk() : Bars.GetAsk(barIndex);
						prevAsk				= Bars.GetAsk(prevBar);
						string strValAsk	= Instrument.MasterInstrument.FormatPrice(valAsk);
						
						InlineUIContainer askIUICont = new InlineUIContainer();
						TextBlock askTextBlock = new TextBlock();
						askTextBlock.Inlines.Add(new Run(strAsk) { Foreground = lbBrush });
						askTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValAsk,
										Foreground = valAsk > prevAsk ? upBrush : valAsk < prevAsk ? dnBrush : sdBrush,
										Width = WidthOfString(strValAsk)
		                            }
		                });
						askIUICont.Child = askTextBlock;
						mainTextBlock.Inlines.Add(askIUICont);
					}
					#endregion
					
					#region Spread
					if(showSpread)
					{
						if(!showBid)
						{
							valBid			= barIndex == lastBar ? GetCurrentBid() : Bars.GetBid(barIndex);
							prevBid			= Bars.GetBid(prevBar);
						}
						
						if(!showAsk)
						{
							valAsk			= barIndex == lastBar ? GetCurrentAsk() : Bars.GetAsk(barIndex);
							prevAsk			= Bars.GetAsk(prevBar);
						}
						
						valSpread			= valAsk  - valBid;
						prevSpread			= prevAsk - prevBid;
						string strValSpread	= Instrument.MasterInstrument.FormatPrice(valSpread);
						
						InlineUIContainer spreadIUICont = new InlineUIContainer();
						TextBlock spreadTextBlock = new TextBlock();
						spreadTextBlock.Inlines.Add(new Run(strSpread) { Foreground = lbBrush });
						spreadTextBlock.Inlines.Add(new InlineUIContainer
		                {
		                    Child = new TextBlock
		                            {
		                                Text = strValSpread,
										Foreground = valSpread > prevSpread ? upBrush : valSpread < prevSpread ? dnBrush : sdBrush,
										Width = WidthOfString(strValSpread, .75)
		                            }
		                });
						spreadIUICont.Child = spreadTextBlock;
						mainTextBlock.Inlines.Add(spreadIUICont);
					}
					#endregion
					#endregion
					
					#region Check for stale data
					staleDataCounter				= ((Time.GetValueAt(barIndex) == staleTime) && (Close.GetValueAt(barIndex) == staleClose)) ? staleDataCounter + timerInterval : timerInterval;
					
					//	uncomment the next line to see when the timer is enabled/disabled
//					Print(chartBars.ToChartString() + " : " + staleDataCounter);
					
					//	disable the Timer when not needed
					myTimer.Enabled					= (staleDataCounter >= timeLimit) ? false : (barIndex != lastBar) ? false : (disableOnDeactivate && !chartWindow.IsActive) ? false : (disableOnMouseLeave && !mouseIsInChartControl) ? false : true;
					
					staleTime						= Time.GetValueAt(barIndex);
					staleClose						= Close.GetValueAt(barIndex);
					#endregion
				}));
			}
            catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
			}
        }
		#endregion
		
		
		#region PointToLastBar()
		private void PointToLastBar()
		{
			try
            {
				if(State < State.Realtime || Bars == null || chartBars == null)
				{
					return;
				}
				
				lastBar										= Bars.Count - (Calculate == Calculate.OnBarClose ? 2 : 1);
				barIndex									= allowHistorical ? chartBars.ToIndex : lastBar;
				prevBar										= barIndex == 0 ? 0 : barIndex - 1;
            }
            catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
			}
		}
		#endregion
		
		
		#region WidthOfString()
		private double WidthOfString(string TextString, double multiplier = 1)
		{
			try
			{
				FormattedText ft = new FormattedText(TextString ?? string.Empty, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, myFont.Typeface, myFont.Size, Brushes.Black);
			    return (ft.Width + 20);
			}
			catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
				return (valueTextWidth * multiplier);
			}	
		}
        #endregion
		
		
		//	code taken and modified as needed from indicator: ChartCustomToolBarExample, written by Chelsea Bell
		//	START
		#region TabSelected()
		private bool TabSelected()
		{
			bool tabSelected = false;

			// loop through tabs and see if the tab that this indicator is added to is the selected item
			foreach (TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as ChartTab).ChartControl == chartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}
		#endregion
		
		
		#region TabChangedHandler()
		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
			{
				InsertWPFControls();
				myTimer.Enabled							= disableOnMouseLeave ? false : true;
			}
			else
			{
				RemoveWPFControls();
				myTimer.Enabled							= false;
			}
		}
		#endregion
		
		
		#region CreateWPFControls()
		protected void CreateWPFControls()
		{
			if(chartControl == null || Bars == null || chartBars == null || (State < State.Realtime))
			{
				return;
			}
			
			bool thereIsNoPoint = (!IsVisible || (!showTime && !showOHLC && !showVolume && !showTicks && !showVPT && !showBid && !showAsk && !showSpread)) ? true : false;
			
			if (thereIsNoPoint)
			{
				if(myTimer != null)
				{
					myTimer.Enabled							= false;
				}
				return;
			}
			
			if(mainTextBlock == null)
			{
				mainTextBlock								= new TextBlock()
				{
					Padding									= new Thickness(0), 
					Margin									= new Thickness(5, 0, 0, 0), 
					VerticalAlignment						= VerticalAlignment.Center, 
					TextWrapping            				= myTextWrapping, 
					Text									= "Waiting for data..."
				};
			}
			
			myFont.ApplyTo(mainTextBlock);
			
			if(myTimer == null)
			{
				myTimer 									= new Timer(timerInterval);
				myTimer.Elapsed					 	  	   += new ElapsedEventHandler(TimerEventProcessor);
				myTimer.AutoReset 							= true;
				myTimer.Enabled								= thereIsNoPoint ? false : (disableOnMouseLeave && !mouseIsInChartControl) ? false : true;
			}
			
			if (TabSelected())
			{
				InsertWPFControls();
			}
			
			if (myTimer.Enabled == false)
			{
				//	do it manually one time
				UpdateValues();
			}
			
			chartWindow.Activated						   += OnWindowActivated;
			chartWindow.Deactivated						   += OnWindowDeactivated;
			chartWindow.MainTabControl.SelectionChanged    += TabChangedHandler;
			chartControl.MouseEnter						   += OnMouseEnter;
			chartControl.MouseMove      	  			   += OnMouseMove;
			chartControl.MouseLeave						   += OnMouseLeave;
		}
		#endregion
		
		
		#region InsertWPFControls()
		protected void InsertWPFControls()
		{
			if (panelActive || !IsVisible)
			{
				if(myTimer != null)
				{
					myTimer.Enabled							= false;
				}
				return;
			}
			
			if (chartGrid.RowDefinitions.Count == 0)
				chartGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

			int tabControlStartRow		= Grid.GetRow(chartWindow.MainTabControl);

			chartGrid.RowDefinitions.Insert(tabControlStartRow, new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

			// including the chartTabControl move all items below the chart down one row
			for (int i = 0; i < chartGrid.Children.Count; i++)
			{
				if (Grid.GetRow(chartGrid.Children[i]) >= tabControlStartRow)
					Grid.SetRow(chartGrid.Children[i], Grid.GetRow(chartGrid.Children[i]) + 1);
			}

			// set the rows for our new items
			Grid.SetColumn(mainTextBlock, Grid.GetColumn(chartWindow.MainTabControl));
			Grid.SetRow(mainTextBlock, tabControlStartRow);
			
			chartGrid.Children.Add(mainTextBlock);

			// let the script know the panel is active
			panelActive = true;
		}
		#endregion
		
		
		#region DisposeWPFControls()
		private void DisposeWPFControls()
		{
			RemoveWPFControls();
			
			if (mainTextBlock != null)
            {
                mainTextBlock = null;
            }
			
			if(myTimer != null)
			{
				myTimer.Enabled  							= false;
				myTimer.Elapsed 						   -= new ElapsedEventHandler(TimerEventProcessor);
				myTimer.Dispose();
			}
			
			chartWindow.Activated						   -= OnWindowActivated;
			chartWindow.Deactivated						   -= OnWindowDeactivated;
			chartWindow.MainTabControl.SelectionChanged    -= TabChangedHandler;
			chartControl.MouseEnter						   -= OnMouseEnter;
			chartControl.MouseMove      				   -= OnMouseMove;
			chartControl.MouseLeave						   -= OnMouseLeave;
		}
		#endregion
		
		
		#region RemoveWPFControls()
		protected void RemoveWPFControls()
		{
			if (!panelActive)
			{
				return;
			}

			if (mainTextBlock != null)
			{
				chartGrid.RowDefinitions.RemoveAt(Grid.GetRow(mainTextBlock));
				chartGrid.Children.Remove(mainTextBlock);
			}

			// if the childs row is 1 (so we can move it to 0) and the row is below the row we are removing, shift it up
			for (int i = 0; i < chartGrid.Children.Count; i++)
			{
				if (Grid.GetRow(chartGrid.Children[i]) > 0 && Grid.GetRow(chartGrid.Children[i]) > Grid.GetRow(mainTextBlock))
					Grid.SetRow(chartGrid.Children[i], Grid.GetRow(chartGrid.Children[i]) - 1);
			}
			
			panelActive = false;
		}
		#endregion
		//	END
		
		
		#region Properties
		
		#region Group: Inputs
		
		[Display(Name="Update On Active Window Only", Order=10, GroupName="Manage Resources", Description="Disable updating while the chart window is not active (focused).")]
		public bool disableOnDeactivate
		{ get; set; }
		
		[Display(Name="Update On MouseOver Only", Order=20, GroupName="Manage Resources", Description="Disable updating while the mouse is not over the chart.")]
		public bool disableOnMouseLeave
		{ get; set; }
		
		[Range(250, double.MaxValue)]
		[Display(Name="Update Interval", Order=30, GroupName="Manage Resources", Description="Speed of updates in milliseconds/\r\nLower is faster, minimum 250.")]
		public double timerInterval
		{ get; set; }
		
		[Display(Name="Use Last Visible Bar", Order=40, GroupName="Manage Resources", Description="    Checked = Allows usage of info for the last visible bar on screen.\r\nUnchecked = Defaults to info of absolute last bar only.")]
		public bool allowHistorical
		{ get; set; }
		
		
		[Display(Name="Open Compare To", Order=10, GroupName="Price Comparisons", Description="    Checked = Compare current Open to previous bar's Open.\r\n\r\nUnchecked = Compare current Open to current Close.")]
		public bool openCompareToPrevious
		{ get; set; }
		
		[Display(Name="Close Compare To", Order=20, GroupName="Price Comparisons", Description="    Checked = Compare current Close to previous bar's Close.\r\n\r\nUnchecked = Compare current Close to current Open.")]
		public bool closeCompareToPrevious
		{ get; set; }
		
		
		[Display(Name="Time", Order=10, GroupName="Select Fields", Description="Checked = Display Time.")]
		public bool showTime
		{ get; set; }
		
		[Display(Name="OHLC", Order=20, GroupName="Select Fields", Description="Checked = Display Open, High, Low, & Close prices.")]
		public bool showOHLC
		{ get; set; }
		
		[Display(Name="Range", Order=30, GroupName="Select Fields", Description="Checked = Display bar Range. (High - Low)")]
		public bool showRange
		{ get; set; }
		
		[Display(Name="Range Volume Average", Order=40, GroupName="Select Fields", Description="Checked = Display Volume / Range.")]
		public bool showVPR
		{ get; set; }
		
		[Display(Name="Volume", Order=50, GroupName="Select Fields", Description="Checked = Display Volume.")]
		public bool showVolume
		{ get; set; }
		
		[Display(Name="Ticks", Order=60, GroupName="Select Fields", Description="Checked = Display Ticks.\r\n(For historical values, Tick Replay must be enabled.)")]
		public bool showTicks
		{ get; set; }
		
		[Display(Name="Volume Per Tick Average", Order=70, GroupName="Select Fields", Description="Checked = Display Volume per Tick average.\r\n(For historical values, Tick Replay must be enabled.)")]
		public bool showVPT
		{ get; set; }
		
		[Display(Name="Bid", Order=80, GroupName="Select Fields", Description="Checked = Display Bid price.")]
		public bool showBid
		{ get; set; }
		
		[Display(Name="Ask", Order=90, GroupName="Select Fields", Description="Checked = Display Ask price.")]
		public bool showAsk
		{ get; set; }
		
		[Display(Name="Spread", Order=100, GroupName="Select Fields", Description="Checked = Display Bid/Ask spread.")]
		public bool showSpread
		{ get; set; }
	
		
		[Display(Name="Font", Order=10, GroupName="Text Options")]
		public SimpleFont myFont
		{ get; set; }
		
		[Display(Name="Wrapping", Order=20, GroupName="Text Options")]
		public TextWrapping myTextWrapping
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="If Higher Value", Order=30, GroupName="Text Options", Description="Checks current bar value vs previous bar value.")]
		public Brush upBrush
		{ get; set; }
		
		[Browsable(false)]
		public string upBrushSerializable
		{
			get { return Serialize.BrushToString(upBrush); }
			set { upBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="If Lower Value", Order=40, GroupName="Text Options", Description="Checks current bar value vs previous bar value.")]
		public Brush dnBrush
		{ get; set; }
		
		[Browsable(false)]
		public string dnBrushSerializable
		{
			get { return Serialize.BrushToString(dnBrush); }
			set { dnBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="If Same Value", Order=50, GroupName="Text Options", Description="Checks current bar value vs previous bar value.")]
		public Brush sdBrush
		{ get; set; }

		[Browsable(false)]
		public string sdBrushSerializable
		{
			get { return Serialize.BrushToString(sdBrush); }
			set { sdBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Time & Date Color", Order=60, GroupName="Text Options")]
		public Brush tmBrush
		{ get; set; }
		
		[Browsable(false)]
		public string tmBrushSerializable
		{
			get { return Serialize.BrushToString(tmBrush); }
			set { tmBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Labels", Order=70, GroupName="Text Options")]
		public Brush lbBrush
		{ get; set; }

		[Browsable(false)]
		public string lbBrushSerializable
		{
			get { return Serialize.BrushToString(lbBrush); }
			set { lbBrush = Serialize.StringToBrush(value); }
		}
		
		#endregion
		
		#endregion
		
		
	}
	
	
	
}






























































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TG.BarInfo[] cacheBarInfo;
		public TG.BarInfo BarInfo()
		{
			return BarInfo(Input);
		}

		public TG.BarInfo BarInfo(ISeries<double> input)
		{
			if (cacheBarInfo != null)
				for (int idx = 0; idx < cacheBarInfo.Length; idx++)
					if (cacheBarInfo[idx] != null &&  cacheBarInfo[idx].EqualsInput(input))
						return cacheBarInfo[idx];
			return CacheIndicator<TG.BarInfo>(new TG.BarInfo(), input, ref cacheBarInfo);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TG.BarInfo BarInfo()
		{
			return indicator.BarInfo(Input);
		}

		public Indicators.TG.BarInfo BarInfo(ISeries<double> input )
		{
			return indicator.BarInfo(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TG.BarInfo BarInfo()
		{
			return indicator.BarInfo(Input);
		}

		public Indicators.TG.BarInfo BarInfo(ISeries<double> input )
		{
			return indicator.BarInfo(input);
		}
	}
}

#endregion
