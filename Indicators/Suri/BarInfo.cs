#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
    public class BarInfo : Indicator {
        private Point mousePosition;
        private int barIndex;

        private double valOpen, valHigh, valLow, valClose, valRange, valBid, valAsk, valSpread, valVolume;
        private DateTime valTime;

        private Chart chartWindow;
        private Grid chartGrid;
        private ChartControl chartControl;
        private ChartBars chartBars;
        private bool panelActive;
        private TextBlock mainTextBlock;

        private Timer myTimer;
        private TabItem tabItem;
        private ChartTab chartTab;

        private double timeLimit = 30000; //	in milliseconds (30,000 = 30 seconds)
        private double staleDataCounter;
        private double staleClose;
        private DateTime staleTime;
        private int staleBar;
        
        private double timerInterval = 250;
        private SimpleFont myFont = new SimpleFont("Global User Interface", 12) {Size = 12, Italic = true, Bold = true, Family = new FontFamily("Global User Interface")};
        private readonly Brush brush = Brushes.Green;
        
        #region Properties
        [Display(Name = "Time", Order = 10, GroupName = "Select Fields", Description = "Checked = Display Time.")]
        public bool ShowTime { get; set; }
        [Display(Name = "OHLC", Order = 20, GroupName = "Select Fields", Description = "Checked = Display Open, High, Low, & Close prices.")]
        public bool ShowOhlc { get; set; }
        [Display(Name = "Range", Order = 30, GroupName = "Select Fields", Description = "Checked = Display bar Range. (High - Low)")]
        public bool ShowRange { get; set; }
        [Display(Name = "Volume", Order = 50, GroupName = "Select Fields", Description = "Checked = Display Volume.")]
        public bool ShowVolume { get; set; }
        [Display(Name = "Ticks", Order = 60, GroupName = "Select Fields", Description = "Checked = Display Ticks.\r\n(For historical values, Tick Replay must be enabled.)")]
        public bool ShowTicks { get; set; }
        [Display(Name = "Bid", Order = 80, GroupName = "Select Fields", Description = "Checked = Display Bid price.")]
        public bool ShowBid { get; set; }
        [Display(Name = "Ask", Order = 90, GroupName = "Select Fields", Description = "Checked = Display Ask price.")]
        public bool ShowAsk { get; set; }
        [Display(Name = "Spread", Order = 100, GroupName = "Select Fields", Description = "Checked = Display Bid/Ask spread.")]
        public bool ShowSpread { get; set; }
        #endregion

        protected override void OnBarUpdate() {}
        protected override void OnStateChange() {
            if (State == State.SetDefaults) {
                Description = "";
                Name = "Toolbar";
                Calculate = Calculate.OnEachTick;
                ScaleJustification = ScaleJustification.Right;
                IsChartOnly = true;
                IsOverlay = true;
                DisplayInDataBox = false;
                DrawOnPricePanel = true;
                PaintPriceMarkers = false;
                IsSuspendedWhileInactive = true;
                IsAutoScale = false;

                ShowTime = true;
                ShowOhlc = true;
                ShowRange = false;
                ShowVolume = true;
                ShowTicks = false;
                ShowBid = false;
                ShowAsk = false;
                ShowSpread = true;
            } else if (State == State.DataLoaded) {
                Name = "";
                Calculate = Calculate.OnEachTick;
                
                chartControl = ChartControl;
                chartBars = ChartBars;

                if (chartControl == null || chartBars == null) return;

                chartWindow = chartControl.OwnerChart;

                if (chartWindow == null) return;

                chartGrid = chartWindow.MainTabControl.Parent as Grid;
            } else if (State == State.Realtime) {
                if (chartControl == null || Bars == null || chartBars == null || State < State.Realtime) return;
                chartControl.Dispatcher.InvokeAsync(CreateWpfControls);
            } else if (State == State.Terminated) {
                chartTab = null;
                tabItem = null;
                chartBars = null;
                chartGrid = null;
                chartWindow = null;
                if (chartControl != null) {
                    chartControl.Dispatcher.Invoke(DisposeWpfControls);
                    chartControl = null;
                }
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
            if (State < State.Realtime || myTimer == null) {
                return;
            }
            if (!myTimer.Enabled && barIndex != staleBar) {
                staleDataCounter = 0;
                //UpdateValues();
                staleBar = barIndex;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            //mousePosition = e.GetPosition(chartControl);
            //mousePosition.X = mousePosition.X.ConvertToHorizontalPixels(chartControl.PresentationSource);
            //barIndex = chartBars.GetBarIdxByX(chartControl, Convert.ToInt32(Math.Round(mousePosition.X, 0)));

            if (myTimer != null) {
                myTimer.Enabled = true;
            }
        }

        private void TimerEventProcessor(object source, ElapsedEventArgs e) {
            if (State < State.Realtime) {
                return;
            }

            if (!IsVisible) {
                if (myTimer != null) {
                    myTimer.Enabled = false;
                }

                return;
            }

            //UpdateValues();
        }

        private void UpdateValues() {
                if (State < State.Realtime) {
                    return;
                }

                chartControl.Dispatcher.InvokeAsync(() => {
                    #region update TextBlock mainTextBlock

                    mainTextBlock.Inlines.Clear();

                    #region Time

                    if (ShowTime) {
                        valTime = Time.GetValueAt(barIndex);
                        string strValTime = valTime.ToString("dd/MM/yyyy (ddd) hh:mm:ss tt",
                            System.Globalization.CultureInfo.CurrentCulture);

                        InlineUIContainer timeIuiCont = new InlineUIContainer();
                        TextBlock timeTextBlock = new TextBlock();
                        timeTextBlock.Inlines.Add(new Run("") {Foreground = brush});
                        timeTextBlock.Inlines.Add(new InlineUIContainer {
                            Child = new TextBlock {
                                Text = strValTime,
                                Foreground = brush,
                                Width = WidthOfString(strValTime, 3)
                            }
                        });
                        timeIuiCont.Child = timeTextBlock;
                        mainTextBlock.Inlines.Add(timeIuiCont);
                    }

                    #endregion

                    #region OHLC

                    if (ShowOhlc) {
                        #region Open

                        valOpen = Open.GetValueAt(barIndex);
                        string strValOpen = Bars.Instrument.MasterInstrument.FormatPrice(valOpen);

                        InlineUIContainer openIuiCont = new InlineUIContainer();
                        TextBlock openTextBlock = new TextBlock();
                        openTextBlock.Inlines.Add(new Run("O: ") {Foreground = brush});
                        openTextBlock.Inlines.Add(new InlineUIContainer {
                            Child = new TextBlock {
                                Text = strValOpen,
                                Foreground = brush,
                                Width = WidthOfString(strValOpen)
                            }
                        });
                        openIuiCont.Child = openTextBlock;
                        mainTextBlock.Inlines.Add(openIuiCont);

                        #endregion

                        #region High

                        valHigh = High.GetValueAt(barIndex);
                        string strValHigh = Bars.Instrument.MasterInstrument.FormatPrice(valHigh);

                        InlineUIContainer highIuiCont = new InlineUIContainer();
                        TextBlock highTextBlock = new TextBlock();
                        highTextBlock.Inlines.Add(new Run("H: ") {Foreground = brush});
                        highTextBlock.Inlines.Add(new InlineUIContainer {
                            Child = new TextBlock {
                                Text = strValHigh,
                                Foreground = brush,
                                Width = WidthOfString(strValHigh)
                            }
                        });
                        highIuiCont.Child = highTextBlock;
                        mainTextBlock.Inlines.Add(highIuiCont);

                        #endregion

                        #region Low

                        valLow = Low.GetValueAt(barIndex);
                        string strValLow = Bars.Instrument.MasterInstrument.FormatPrice(valLow);

                        InlineUIContainer lowIuiCont = new InlineUIContainer();
                        TextBlock lowTextBlock = new TextBlock();
                        lowTextBlock.Inlines.Add(new Run("L: ") {Foreground = brush});
                        lowTextBlock.Inlines.Add(new InlineUIContainer {
                            Child = new TextBlock {
                                Text = strValLow,
                                Foreground = brush,
                                Width = WidthOfString(strValLow)
                            }
                        });
                        lowIuiCont.Child = lowTextBlock;
                        mainTextBlock.Inlines.Add(lowIuiCont);

                        #endregion

                        #region Close

                        valClose = Close.GetValueAt(barIndex);
                        string strValClose = Bars.Instrument.MasterInstrument.FormatPrice(valClose);

                        InlineUIContainer closeIuiCont = new InlineUIContainer();
                        TextBlock closeTextBlock = new TextBlock();
                        closeTextBlock.Inlines.Add(new Run("C: ") {Foreground = brush});
                        closeTextBlock.Inlines.Add(new InlineUIContainer {
                            Child = new TextBlock {
                                Text = strValClose,
                                Foreground = brush,
                                Width = WidthOfString(strValClose)
                            }
                        });
                        closeIuiCont.Child = closeTextBlock;
                        mainTextBlock.Inlines.Add(closeIuiCont);

                        #endregion
                    }

                    #endregion

                    #region Range

                    if (ShowRange) {
                        valRange = High.GetValueAt(barIndex) - Low.GetValueAt(barIndex);
                        string strValRange = Instrument.MasterInstrument.FormatPrice(valRange);

                        InlineUIContainer rangeIuiCont = new InlineUIContainer();
                        TextBlock rangeTextBlock = new TextBlock();
                        rangeTextBlock.Inlines.Add(new Run("R: ") {Foreground = brush});
                        rangeTextBlock.Inlines.Add(new InlineUIContainer {
                            Child = new TextBlock {
                                Text = strValRange,
                                Foreground = brush,
                                Width = WidthOfString(strValRange)
                            }
                        });
                        rangeIuiCont.Child = rangeTextBlock;
                        mainTextBlock.Inlines.Add(rangeIuiCont);
                    }

                    #endregion

                    #region Volume

                    if (ShowVolume) {
                        valVolume = Volume.GetValueAt(barIndex);
                        string strValVolume = valVolume.ToString();

                        InlineUIContainer volIuiCont = new InlineUIContainer();
                        TextBlock volTextBlock = new TextBlock();
                        volTextBlock.Inlines.Add(new Run("V: ") {Foreground = brush});
                        volTextBlock.Inlines.Add(new InlineUIContainer {
                            Child = new TextBlock {
                                Text = strValVolume,
                                Foreground = brush,
                                Width = WidthOfString(strValVolume)
                            }
                        });
                        volIuiCont.Child = volTextBlock;
                        mainTextBlock.Inlines.Add(volIuiCont);
                    }

                    #endregion

                    #region Bid

                    if (ShowBid) {
                        valBid = Bars.GetBid(barIndex);
                        string strValBid = Instrument.MasterInstrument.FormatPrice(valBid);

                        InlineUIContainer bidIuiCont = new InlineUIContainer();
                        TextBlock bidTextBlock = new TextBlock();
                        bidTextBlock.Inlines.Add(new Run("B: ") {Foreground = brush});
                        bidTextBlock.Inlines.Add(new InlineUIContainer {
                            Child = new TextBlock {
                                Text = strValBid,
                                Foreground = brush,
                                Width = WidthOfString(strValBid)
                            }
                        });
                        bidIuiCont.Child = bidTextBlock;
                        mainTextBlock.Inlines.Add(bidIuiCont);
                    }

                    #endregion

                    #region Ask

                    if (ShowAsk) {
                        valAsk = Bars.GetAsk(barIndex);
                        string strValAsk = Instrument.MasterInstrument.FormatPrice(valAsk);

                        InlineUIContainer askIuiCont = new InlineUIContainer();
                        TextBlock askTextBlock = new TextBlock();
                        askTextBlock.Inlines.Add(new Run("A: ") {Foreground = brush});
                        askTextBlock.Inlines.Add(new InlineUIContainer {
                            Child = new TextBlock {
                                Text = strValAsk,
                                Foreground = brush,
                                Width = WidthOfString(strValAsk)
                            }
                        });
                        askIuiCont.Child = askTextBlock;
                        mainTextBlock.Inlines.Add(askIuiCont);
                    }

                    #endregion

                    #region Spread

                    if (ShowSpread) {
                        if (!ShowBid) {
                            valBid = Bars.GetBid(barIndex);
                        }

                        if (!ShowAsk) {
                            valAsk = Bars.GetAsk(barIndex);
                        }

                        valSpread = valAsk - valBid;
                        string strValSpread = Instrument.MasterInstrument.FormatPrice(valSpread);

                        InlineUIContainer spreadIuiCont = new InlineUIContainer();
                        TextBlock spreadTextBlock = new TextBlock();
                        spreadTextBlock.Inlines.Add(new Run("S: ") {Foreground = brush});
                        spreadTextBlock.Inlines.Add(new InlineUIContainer {
                            Child = new TextBlock {
                                Text = strValSpread,
                                Foreground = brush,
                                Width = WidthOfString(strValSpread, .75)
                            }
                        });
                        spreadIuiCont.Child = spreadTextBlock;
                        mainTextBlock.Inlines.Add(spreadIuiCont);
                    }

                    #endregion

                    #endregion

                    #region Check for stale data

                    staleDataCounter =
                        Time.GetValueAt(barIndex) == staleTime && Close.GetValueAt(barIndex) == staleClose
                            ? staleDataCounter + timerInterval
                            : timerInterval;

                    //	uncomment the next line to see when the timer is enabled/disabled
//					Print(chartBars.ToChartString() + " : " + staleDataCounter);

                    staleTime = Time.GetValueAt(barIndex);
                    staleClose = Close.GetValueAt(barIndex);

                    #endregion
                });
        }

        private double WidthOfString(string textString, double multiplier = 1) {
            FormattedText ft = new FormattedText(textString ?? string.Empty,
                System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, myFont.Typeface,
                myFont.Size, Brushes.Black);
            return (ft.Width + 20);
        }

        private bool TabSelected() {
            bool tabSelected = false;

            // loop through tabs and see if the tab that this indicator is added to is the selected item
            foreach (TabItem tab in chartWindow.MainTabControl.Items)
                if (((ChartTab) tab.Content).ChartControl == chartControl &&
                    tab == chartWindow.MainTabControl.SelectedItem)
                    tabSelected = true;

            return tabSelected;
        }

        private void TabChangedHandler(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count <= 0) return;

            tabItem = e.AddedItems[0] as TabItem;
            if (tabItem == null) return;

            chartTab = tabItem.Content as ChartTab;
            if (chartTab == null) return;

            if (TabSelected()) {
                InsertWpfControls();
                myTimer.Enabled = true;
            } else {
                RemoveWpfControls();
                myTimer.Enabled = false;
            }
        }

        protected void CreateWpfControls() {
            if (chartControl == null || Bars == null || chartBars == null || (State < State.Realtime)) {
                return;
            }

            bool thereIsNoPoint = !IsVisible || !ShowTime && !ShowOhlc && !ShowVolume && !ShowTicks &&
                                                  !ShowBid && !ShowAsk && !ShowSpread;

            if (thereIsNoPoint) {
                if (myTimer != null) myTimer.Enabled = false;
                return;
            }

            if (mainTextBlock == null) {
                mainTextBlock = new TextBlock() {
                    Padding = new Thickness(0),
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Text = ""
                };
            }

            myFont.ApplyTo(mainTextBlock);

            if (myTimer == null) {
                myTimer = new Timer(timerInterval);
                myTimer.Elapsed += TimerEventProcessor;
                myTimer.AutoReset = true;
                myTimer.Enabled = true;
            }

            if (TabSelected()) {
                InsertWpfControls();
            }

            if (myTimer.Enabled == false) {
                //	do it manually one time
                //UpdateValues();
            }

            chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
            chartControl.MouseMove += OnMouseMove;
        }

        protected void InsertWpfControls() {
            if (panelActive || !IsVisible) {
                if (myTimer != null) myTimer.Enabled = false;
                return;
            }

            if (chartGrid.RowDefinitions.Count == 0)
                chartGrid.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(1, GridUnitType.Star)});

            int tabControlStartRow = Grid.GetRow(chartWindow.MainTabControl);

            chartGrid.RowDefinitions.Insert(tabControlStartRow,
                new RowDefinition() {Height = new GridLength(1, GridUnitType.Auto)});

            // including the chartTabControl move all items below the chart down one row
            for (int i = 0; i < chartGrid.Children.Count; i++) {
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

        private void DisposeWpfControls() {
            RemoveWpfControls();

            mainTextBlock = null;

            if (myTimer != null) {
                myTimer.Enabled = false;
                myTimer.Elapsed -= TimerEventProcessor;
                myTimer.Dispose();
            }

            chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
            chartControl.MouseMove -= OnMouseMove;
        }

        protected void RemoveWpfControls() {
            if (!panelActive) {
                return;
            }

            if (mainTextBlock != null) {
                chartGrid.RowDefinitions.RemoveAt(Grid.GetRow(mainTextBlock));
                chartGrid.Children.Remove(mainTextBlock);
            }

            // if the childs row is 1 (so we can move it to 0) and the row is below the row we are removing, shift it up
            for (int i = 0; i < chartGrid.Children.Count; i++) {
                if (Grid.GetRow(chartGrid.Children[i]) > 0 &&
                    Grid.GetRow(chartGrid.Children[i]) > Grid.GetRow(mainTextBlock))
                    Grid.SetRow(chartGrid.Children[i], Grid.GetRow(chartGrid.Children[i]) - 1);
            }

            panelActive = false;
        }

    }
}





































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.BarInfo[] cacheBarInfo;
		public Suri.BarInfo BarInfo()
		{
			return BarInfo(Input);
		}

		public Suri.BarInfo BarInfo(ISeries<double> input)
		{
			if (cacheBarInfo != null)
				for (int idx = 0; idx < cacheBarInfo.Length; idx++)
					if (cacheBarInfo[idx] != null &&  cacheBarInfo[idx].EqualsInput(input))
						return cacheBarInfo[idx];
			return CacheIndicator<Suri.BarInfo>(new Suri.BarInfo(), input, ref cacheBarInfo);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.BarInfo BarInfo()
		{
			return indicator.BarInfo(Input);
		}

		public Indicators.Suri.BarInfo BarInfo(ISeries<double> input )
		{
			return indicator.BarInfo(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.BarInfo BarInfo()
		{
			return indicator.BarInfo(Input);
		}

		public Indicators.Suri.BarInfo BarInfo(ISeries<double> input )
		{
			return indicator.BarInfo(input);
		}
	}
}

#endregion
