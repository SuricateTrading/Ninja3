#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using NinjaTrader.Gui.Chart;
using System.Windows.Controls;
using System.Windows.Input;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.NinjaScript.Indicators.Suri.dev;
using VerticalAlignment = System.Windows.VerticalAlignment;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public sealed class SuriChartToolbar : Indicator {
		private Chart		chartWindow;
		private Grid		chartGrid;
		private TabItem		tabItem;
		private ChartTab	chartTab;
		private Grid		menu;
		private bool		isPanelActive;
		
		public override string DisplayName { get { return "Toolbar"; } }
		protected override void OnBarUpdate() {}
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Name								= "Toolbar";
				Description							= "Zeigt eine Toolbar an, mit der Indikatoren ein- und ausgeschaltet werden können.";
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

		// Returns true if visible. Returns false if not visible. Returns null iff not laoded.
		private bool? IsIndicatorVisible(Type[] types, params SuriCotReportField[] cotFields) {
			foreach (var indicator in ChartControl.Indicators) {
				foreach (Type type in types) {
					if (indicator.GetType() == type) {
						if (type != typeof(SuriCot)) return indicator.IsVisible;
						// for CotBase the report field must match too:
						foreach (SuriCotReportField field in cotFields) {
							if (((SuriCot)indicator).reportField == field) return indicator.IsVisible;
						}
					}
				}
			}
			return null;
		}
		private void OnCheckBoxClick(Type[] types, params SuriCotReportField[] cotFields) {
			try {
				bool update = false;
				foreach (var indicator in ChartControl.Indicators) {
					foreach (Type type in types) {
						if (indicator.GetType() == type) {
							if (type != typeof(SuriCot)) {
								indicator.IsVisible = !indicator.IsVisible;
								update = true;
							} else {
								foreach (SuriCotReportField field in cotFields) {
									if (((SuriCot) indicator).reportField == field) {
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
			if (chartWindow == null) return;
			chartGrid = chartWindow.MainTabControl.Parent as Grid;
			menu = new Grid {
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				ColumnDefinitions = {
					new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(),
					new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(),
					new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition {Width = new GridLength(200)},
				},
				RowDefinitions = { new RowDefinition() },
			};

			InitIndicatorSettings();
			
			if (TabSelected()) InsertWpfControls();
			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}


		private void AddCheckBox(string name, bool? visible, int x, int y, RoutedEventHandler onClick) {
			if (visible == null) return;
			CheckBox checkBox = new CheckBox() {
				Content = name,
				IsChecked = visible,
				Margin = new Thickness(0, 0, 15, 0),
			};
			checkBox.Click += onClick;
			Grid.SetRow(checkBox, x);
			Grid.SetColumn(checkBox, y);
			menu.Children.Add(checkBox);
		}

		private void InitIndicatorSettings() {
			AddCheckBox("COT 1", IsIndicatorVisible(new[] {typeof(SuriCot1)}), 0, 0, (sender, args) => OnCheckBoxClick(new[] {typeof(SuriCot1)}));
			AddCheckBox("COT 2", IsIndicatorVisible(new[] {typeof(SuriCot2)}), 0, 1, (sender, args) => OnCheckBoxClick(new[] {typeof(SuriCot2)}));
			AddCheckBox("Comm Netto", IsIndicatorVisible(new[] {typeof(SuriCot)}, SuriCotReportField.CommercialNet), 0, 2, (sender, args) => OnCheckBoxClick(new[] {typeof(SuriCot)}, SuriCotReportField.CommercialNet));
			AddCheckBox("SMA", IsIndicatorVisible(new[] {typeof(SuriSma)}), 0, 3, (sender, args) => OnCheckBoxClick(new[] {typeof(SuriSma)}));
			AddCheckBox("Volumen", IsIndicatorVisible(new[] {typeof(SuriVolume)}), 0, 4, (sender, args) => OnCheckBoxClick(new[] {typeof(SuriVolume)}));
			AddCheckBox("Bargröße", IsIndicatorVisible(new[] {typeof(SuriBarRange)}), 0, 5, (sender, args) => OnCheckBoxClick(new[] {typeof(SuriBarRange)}));
			AddCheckBox("Mega", IsIndicatorVisible(new[] {typeof(SuriMega)}), 0, 6, (sender, args) => OnCheckBoxClick(new[] {typeof(SuriMega)}));
			AddCheckBox("Trend Trader", IsIndicatorVisible(new []{typeof(SuriCot)}, SuriCotReportField.NoncommercialLong, SuriCotReportField.NoncommercialShort), 0, 7, (sender, args) => OnCheckBoxClick(new []{typeof(SuriCot)}, SuriCotReportField.NoncommercialLong, SuriCotReportField.NoncommercialShort));
			AddCheckBox("Open Interest", IsIndicatorVisible(new []{typeof(ComShortOpenInterest), typeof(SuriCot)}, SuriCotReportField.OpenInterest), 0, 8, (sender, args) => OnCheckBoxClick(new []{typeof(ComShortOpenInterest), typeof(SuriCot)}, SuriCotReportField.OpenInterest));
			AddCheckBox("VP groß", IsIndicatorVisible(new []{typeof(SuriVolumeProfileBig)}), 0, 9, (sender, args) => OnCheckBoxClick(new []{typeof(SuriVolumeProfileBig)}));
			AddCheckBox("Produktionskosten", IsIndicatorVisible(new []{typeof(SuriProductionCost)}), 0, 10, (sender, args) => OnCheckBoxClick(new []{typeof(SuriProductionCost)}));
			
			var comList = new ComboBox();
			foreach (KeyValuePair<Commodity,CommodityData> entry in SuriStrings.data) {
				comList.Items.Add(entry.Value.shortName + "\t" + entry.Value.longName);
			}
			
			Commodity? currentComm = SuriStrings.GetComm(Instrument.MasterInstrument.Name);
			if (currentComm != null) {
				var c = SuriStrings.data[currentComm.Value];
				comList.SelectedItem = c.shortName + "\t" + c.longName;
			}
			
			comList.SelectionChanged += (sender, args) => {
				Keyboard.ClearFocus();
				ChartControl.OwnerChart.Focus();

				string change = ((ComboBox) sender).SelectedItem as string;
				if (change == null) return;
				string shortName = Regex.Replace(change, "\t.*", "");
				Instrument ins = Instrument.GetInstrument(shortName + Instrument.GetInstrument(shortName+" ##-##").MasterInstrument.GetNextExpiry(DateTime.Now).ToString(" MM-yy"));
			
				Keyboard.FocusedElement.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
						new TextComposition(InputManager.Current, ChartControl.OwnerChart, "open sesame"))
					{ RoutedEvent = TextCompositionManager.PreviewTextInputEvent });
				Keyboard.FocusedElement.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
						new TextComposition(InputManager.Current, ChartControl.OwnerChart, ins.FullName))
					{ RoutedEvent = TextCompositionManager.TextInputEvent });
				Keyboard.FocusedElement.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(ChartControl.OwnerChart), 0,  Key.Enter) { RoutedEvent = Keyboard.PreviewKeyDownEvent } );
			};
			Grid.SetRow(comList, 0);
			Grid.SetColumn(comList, 11);
			menu.Children.Add(comList);
		}
		
		
		private void InsertWpfControls() {
			if (isPanelActive) return;
			
			if (chartGrid.RowDefinitions.Count == 0) chartGrid.RowDefinitions.Add(new RowDefinition());

			var tabControlStartRow = Grid.GetRow(chartWindow.MainTabControl);
			chartGrid.RowDefinitions.Insert(tabControlStartRow, new RowDefinition() { Height = new GridLength(26) });
			// including the chartTabControl move all items below the chart and down one row
			for (int i = 0; i < chartGrid.Children.Count; i++) {
				if (Grid.GetRow(chartGrid.Children[i]) >= tabControlStartRow) {
					Grid.SetRow(chartGrid.Children[i], Grid.GetRow(chartGrid.Children[i]) + 1);
				}
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
			
			if (TabSelected()) {
				InsertWpfControls();
			} else {
				RemoveWpfControls();
			}
		}
		
	}
}























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriChartToolbar[] cacheSuriChartToolbar;
		public Suri.SuriChartToolbar SuriChartToolbar()
		{
			return SuriChartToolbar(Input);
		}

		public Suri.SuriChartToolbar SuriChartToolbar(ISeries<double> input)
		{
			if (cacheSuriChartToolbar != null)
				for (int idx = 0; idx < cacheSuriChartToolbar.Length; idx++)
					if (cacheSuriChartToolbar[idx] != null &&  cacheSuriChartToolbar[idx].EqualsInput(input))
						return cacheSuriChartToolbar[idx];
			return CacheIndicator<Suri.SuriChartToolbar>(new Suri.SuriChartToolbar(), input, ref cacheSuriChartToolbar);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriChartToolbar SuriChartToolbar()
		{
			return indicator.SuriChartToolbar(Input);
		}

		public Indicators.Suri.SuriChartToolbar SuriChartToolbar(ISeries<double> input )
		{
			return indicator.SuriChartToolbar(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriChartToolbar SuriChartToolbar()
		{
			return indicator.SuriChartToolbar(Input);
		}

		public Indicators.Suri.SuriChartToolbar SuriChartToolbar(ISeries<double> input )
		{
			return indicator.SuriChartToolbar(input);
		}
	}
}

#endregion
