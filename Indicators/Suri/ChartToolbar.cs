#region Using declarations
using System;
using System.Windows;
using NinjaTrader.Gui.Chart;
using System.Windows.Controls;
using System.Windows.Input;
using VerticalAlignment = System.Windows.VerticalAlignment;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class ChartToolbar : Indicator {
		private Chart		chartWindow;
		private Grid		chartGrid;
		private TabItem		tabItem;
		private ChartTab	chartTab;
		private bool		isPanelActive;
		private Grid		menu;
		
		public override string DisplayName { get { return "Toolbar"; } }
		protected override void OnBarUpdate() {}

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Name								= "Toolbar";
				Description							= @"Zeigt eine Toolbar an, mit der Indikatoren ein- und ausgeschaltet werden können.";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= false;
				IsSuspendedWhileInactive			= true;
			} else if (State == State.Historical) {
				if (ChartControl != null) ChartControl.Dispatcher.InvokeAsync(CreateWpfControls);
			} else if (State == State.Terminated) {
				if (ChartControl != null) ChartControl.Dispatcher.InvokeAsync(DisposeWpfControls);
			}
		}

		private bool IsIndicatorVisible(Type[] types, params SuriCotReportField[] cotFields) {
			foreach (var indicator in ChartControl.Indicators) {
				foreach (Type type in types) {
					if (indicator.GetType() == type) {
						if (type != typeof(CotBase)) return indicator.IsVisible;
						// for CotBase the report field must match too:
						foreach (SuriCotReportField field in cotFields) {
							if (((CotBase)indicator).reportField == field) return indicator.IsVisible;
						}
					}
				}
			}
			return false;
		}
		private void OnCheckBoxClick(Type[] types, params SuriCotReportField[] cotFields) {
			try {
				bool update = false;
				foreach (var indicator in ChartControl.Indicators) {
					foreach (Type type in types) {
						if (indicator.GetType() == type) {
							if (type != typeof(CotBase)) {
								indicator.IsVisible = !indicator.IsVisible;
								update = true;
							} else {
								foreach (SuriCotReportField field in cotFields) {
									if (((CotBase) indicator).reportField == field) {
										indicator.IsVisible = !indicator.IsVisible;
										update = true;
									}
								}
							}
						}
					}
				}
				if (update) {
					var p = PresentationSource.FromVisual(ChartControl.OwnerChart);
					if (p == null) return;
					var args = new KeyEventArgs(Keyboard.PrimaryDevice, p, 0, Key.F5)
						{RoutedEvent = Keyboard.PreviewKeyDownEvent};
					Keyboard.FocusedElement.RaiseEvent(args);
				}
			} catch (Exception) {
				// the exception doesnt seem to have any effect...
			}
		}


		private void CreateWpfControls() {
			chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
			chartGrid = chartWindow.MainTabControl.Parent as Grid;
			menu = new Grid() {
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				RowDefinitions = { new RowDefinition(), new RowDefinition() },
				ColumnDefinitions = { new ColumnDefinition() },
			};

			InitIndicatorSettings();
			InitMarketButtons();
			
			if (TabSelected())
				InsertWpfControls();
			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		private void InitIndicatorSettings() {
			Grid submenu1 = new Grid() {
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				ColumnDefinitions = {
					new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(),
					new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition()
				},
			};
			Grid.SetRow(submenu1, 0);
			Grid.SetColumn(submenu1, 0);
			menu.Children.Add(submenu1);

			CheckBox cot1CheckBox = new CheckBox() {
				Content = "COT 1",
				IsChecked = IsIndicatorVisible(new[] {typeof(Cot1)}),
				Margin = new Thickness(0, 0, 15, 0),
			};
			cot1CheckBox.Click += (sender, args) => OnCheckBoxClick(new[] {typeof(Cot1)});
			Grid.SetRow(cot1CheckBox, 0);
			Grid.SetColumn(cot1CheckBox, 0);
			submenu1.Children.Add(cot1CheckBox);

			CheckBox cot2CheckBox = new CheckBox() {
				Content = "COT 2",
				IsChecked = IsIndicatorVisible(new[] {typeof(Cot22)}),
				Margin = new Thickness(0, 0, 15, 0),
			};
			cot2CheckBox.Click += (sender, args) => OnCheckBoxClick(new[] {typeof(Cot22)});
			Grid.SetRow(cot2CheckBox, 0);
			Grid.SetColumn(cot2CheckBox, 1);
			submenu1.Children.Add(cot2CheckBox);

			CheckBox comnetCheckBox = new CheckBox() {
				Content = "Comm Netto",
				IsChecked = IsIndicatorVisible(new[] {typeof(CotBase)}, SuriCotReportField.CommercialNet),
				Margin = new Thickness(0, 0, 15, 0),
			};
			comnetCheckBox.Click += (sender, args) => OnCheckBoxClick(new[] {typeof(CotBase)}, SuriCotReportField.CommercialNet);
			Grid.SetRow(comnetCheckBox, 0);
			Grid.SetColumn(comnetCheckBox, 2);
			submenu1.Children.Add(comnetCheckBox);

			CheckBox smaCheckBox = new CheckBox() {
				Content = "SMA",
				IsChecked = IsIndicatorVisible(new[] {typeof(Sma)}),
				Margin = new Thickness(0, 0, 15, 0),
			};
			smaCheckBox.Click += (sender, args) => OnCheckBoxClick(new[] {typeof(Sma)});
			Grid.SetRow(smaCheckBox, 0);
			Grid.SetColumn(smaCheckBox, 3);
			submenu1.Children.Add(smaCheckBox);

			CheckBox volumeCheckBox = new CheckBox() {
				Content = "Volumen",
				IsChecked = IsIndicatorVisible(new[] {typeof(Volumen)}),
				Margin = new Thickness(0, 0, 15, 0),
			};
			volumeCheckBox.Click += (sender, args) => OnCheckBoxClick(new[] {typeof(Volumen)});
			Grid.SetRow(volumeCheckBox, 0);
			Grid.SetColumn(volumeCheckBox, 4);
			submenu1.Children.Add(volumeCheckBox);

			CheckBox priceRangeCheckBox = new CheckBox() {
				Content = "Preis Range",
				IsChecked = IsIndicatorVisible(new[] {typeof(SuriRange)}),
				Margin = new Thickness(0, 0, 15, 0),
			};
			priceRangeCheckBox.Click += (sender, args) => OnCheckBoxClick(new []{typeof(SuriRange)});
			Grid.SetRow(priceRangeCheckBox, 0);
			Grid.SetColumn(priceRangeCheckBox, 5);
			submenu1.Children.Add(priceRangeCheckBox);

			CheckBox trendTraderCheckBox = new CheckBox() {
				Content = "Trend Trader",
				IsChecked = IsIndicatorVisible(new []{typeof(CotBase)}, SuriCotReportField.NoncommercialLong, SuriCotReportField.NoncommercialShort),
				Margin = new Thickness(0, 0, 15, 0),
			};
			trendTraderCheckBox.Click += (sender, args) => OnCheckBoxClick(new []{typeof(CotBase)}, SuriCotReportField.NoncommercialLong, SuriCotReportField.NoncommercialShort);
			Grid.SetRow(trendTraderCheckBox, 0);
			Grid.SetColumn(trendTraderCheckBox, 6);
			submenu1.Children.Add(trendTraderCheckBox);

			CheckBox openInterestCheckBox = new CheckBox() {
				Content = "Open Interest",
				IsChecked = IsIndicatorVisible(new []{typeof(ComShortOpenInterest), typeof(CotBase)}, SuriCotReportField.OpenInterest),
				Margin = new Thickness(0, 0, 15, 0),
			};
			openInterestCheckBox.Click += (sender, args) => OnCheckBoxClick(new []{typeof(ComShortOpenInterest), typeof(CotBase)}, SuriCotReportField.OpenInterest);
			Grid.SetRow(openInterestCheckBox, 0);
			Grid.SetColumn(openInterestCheckBox, 7);
			submenu1.Children.Add(openInterestCheckBox);
			
			var comList = new ComboBox() { ItemsSource = Enum.GetValues(typeof(Commodity)), Width = 150 };
			comList.SelectionChanged += (sender, args) => { };
			Grid.SetRow(comList, 0);
			Grid.SetColumn(comList, 8);
			submenu1.Children.Add(comList);
		}

		private void InitMarketButtons() {
			/*Grid submenu2 = new Grid() {
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				ColumnDefinitions = {
					new ColumnDefinition()
				},
			};
			Grid.SetRow(submenu2, 1);
			Grid.SetColumn(submenu2, 0);
			menu.Children.Add(submenu2);*/

		}
		
		

		private void InsertWpfControls() {
			if (isPanelActive) return;

			if (chartGrid.RowDefinitions.Count == 0) chartGrid.RowDefinitions.Add(new RowDefinition());

			var tabControlStartRow = Grid.GetRow(chartWindow.MainTabControl);
			chartGrid.RowDefinitions.Insert(tabControlStartRow, new RowDefinition() { Height = new GridLength(40) });
			// including the chartTabControl move all items below the chart and down one row
			for (int i = 0; i < chartGrid.Children.Count; i++) {
				if (Grid.GetRow(chartGrid.Children[i]) >= tabControlStartRow)
					Grid.SetRow(chartGrid.Children[i], Grid.GetRow(chartGrid.Children[i]) + 1);
			}

			// set the columns and rows for our new items
			Grid.SetColumn(menu, Grid.GetColumn(chartWindow.MainTabControl));
			Grid.SetRow(menu, tabControlStartRow);
			chartGrid.Children.Add(menu);
		
			isPanelActive = true;
		}

		private void DisposeWpfControls() {
			if (chartWindow != null) chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
			RemoveWpfControls();
		}
		private void RemoveWpfControls() {
			if (!isPanelActive) return;
			if (menu != null) {
				chartGrid.RowDefinitions.RemoveAt(Grid.GetRow(menu));
				chartGrid.Children.Remove(menu);
			}
			for (int i = 0; i < chartGrid.Children.Count; i++) {
				if (menu != null && Grid.GetRow(chartGrid.Children[i]) > 0 && Grid.GetRow(chartGrid.Children[i]) > Grid.GetRow(menu)) {
					Grid.SetRow(chartGrid.Children[i], Grid.GetRow(chartGrid.Children[i]) - 1);
				}
			}
			isPanelActive = false;
		}
		
		
		
		private bool TabSelected() {
			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (TabItem tab in chartWindow.MainTabControl.Items) {
				if (((ChartTab) tab.Content).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem) {
					return true;
				}
			}
			return false;
		}
		private void TabChangedHandler(object sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count <= 0) return;
			
			tabItem = e.AddedItems[0] as TabItem;
			if (tabItem == null) return;
			chartTab = tabItem.Content as ChartTab;
			if (chartTab == null) return;
			
			if (TabSelected())
				InsertWpfControls();
			else
				RemoveWpfControls();
		}
		
	}
	
	
	
	
	
	
	
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.ChartToolbar[] cacheChartToolbar;
		public Suri.ChartToolbar ChartToolbar()
		{
			return ChartToolbar(Input);
		}

		public Suri.ChartToolbar ChartToolbar(ISeries<double> input)
		{
			if (cacheChartToolbar != null)
				for (int idx = 0; idx < cacheChartToolbar.Length; idx++)
					if (cacheChartToolbar[idx] != null &&  cacheChartToolbar[idx].EqualsInput(input))
						return cacheChartToolbar[idx];
			return CacheIndicator<Suri.ChartToolbar>(new Suri.ChartToolbar(), input, ref cacheChartToolbar);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.ChartToolbar ChartToolbar()
		{
			return indicator.ChartToolbar(Input);
		}

		public Indicators.Suri.ChartToolbar ChartToolbar(ISeries<double> input )
		{
			return indicator.ChartToolbar(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.ChartToolbar ChartToolbar()
		{
			return indicator.ChartToolbar(Input);
		}

		public Indicators.Suri.ChartToolbar ChartToolbar(ISeries<double> input )
		{
			return indicator.ChartToolbar(input);
		}
	}
}

#endregion
