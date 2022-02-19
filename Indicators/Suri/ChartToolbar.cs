#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NinjaTrader.Gui.Chart;
using System.Windows.Controls;
using System.Windows.Input;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.SuriCommon;
using VerticalAlignment = System.Windows.VerticalAlignment;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class ChartToolbar : Indicator {
		private Chart		chartWindow;
		private Grid		chartGrid;
		private TabItem		tabItem;
		private ChartTab	chartTab;
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

		// Returns true if visible. Returns false if not visible. Returns null iff not laoded.
		private bool? IsIndicatorVisible(Type[] types, params SuriCotReportField[] cotFields) {
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
			return null;
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
			if (chartWindow == null) return;
			chartGrid = chartWindow.MainTabControl.Parent as Grid;
			menu = new Grid() {
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				ColumnDefinitions = {
					new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(),
					new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition()
				},
				RowDefinitions = { new RowDefinition() },
			};

			InitIndicatorSettings();
			
			if (TabSelected()) InsertWpfControls();
			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}


		private void AddCheckBox(string name, bool? visible, int x, int y, RoutedEventHandler onClick) {
			CheckBox checkBox = new CheckBox() {
				Content = name,
				IsChecked = visible ?? false,
				IsEnabled = visible != null,
				Margin = new Thickness(0, 0, 15, 0),
			};
			checkBox.Click += onClick;
			Grid.SetRow(checkBox, x);
			Grid.SetColumn(checkBox, y);
			menu.Children.Add(checkBox);
		}

		private void InitIndicatorSettings() {
			AddCheckBox("COT 1", IsIndicatorVisible(new[] {typeof(Cot1)}), 0, 0, (sender, args) => OnCheckBoxClick(new[] {typeof(Cot1)}));
			AddCheckBox("COT 2", IsIndicatorVisible(new[] {typeof(Cot22)}), 0, 1, (sender, args) => OnCheckBoxClick(new[] {typeof(Cot22)}));
			AddCheckBox("Comm Netto", IsIndicatorVisible(new[] {typeof(CotBase)}, SuriCotReportField.CommercialNet), 0, 2, (sender, args) => OnCheckBoxClick(new[] {typeof(CotBase)}, SuriCotReportField.CommercialNet));
			AddCheckBox("SMA", IsIndicatorVisible(new[] {typeof(Sma)}), 0, 3, (sender, args) => OnCheckBoxClick(new[] {typeof(Sma)}));
			AddCheckBox("Volumen", IsIndicatorVisible(new[] {typeof(Volumen)}), 0, 4, (sender, args) => OnCheckBoxClick(new[] {typeof(Volumen)}));
			AddCheckBox("Preis Range", IsIndicatorVisible(new[] {typeof(BarRange)}), 0, 5, (sender, args) => OnCheckBoxClick(new[] {typeof(BarRange)}));
			AddCheckBox("Trend Trader", IsIndicatorVisible(new []{typeof(CotBase)}, SuriCotReportField.NoncommercialLong, SuriCotReportField.NoncommercialShort), 0, 6, (sender, args) => OnCheckBoxClick(new []{typeof(ComShortOpenInterest), typeof(CotBase)}, SuriCotReportField.OpenInterest));
			AddCheckBox("Open Interest", IsIndicatorVisible(new []{typeof(ComShortOpenInterest), typeof(CotBase)}, SuriCotReportField.OpenInterest), 0, 7, (sender, args) => OnCheckBoxClick(new []{typeof(ComShortOpenInterest), typeof(CotBase)}, SuriCotReportField.OpenInterest));

			var comList = new ComboBox { Width = 150 };
			foreach (KeyValuePair<Commodity,CommodityData> entry in SuriStrings.data) {
				comList.Items.Add(entry.Value.longName);
			}

			Commodity? currentComm = SuriStrings.GetComm(Instrument.MasterInstrument.Name);
			if (currentComm != null) {
				comList.SelectedItem = SuriStrings.data[currentComm.Value].longName;
			}
			
			comList.SelectionChanged += (sender, args) => {
				Keyboard.ClearFocus();
				ChartControl.OwnerChart.Focus();

				string change = ((ComboBox) sender).SelectedItem as string;
				string shortName = SuriStrings.LongNameToShortName(change);
				if (shortName == null) return;
			
				Instrument ins = Instrument.All.Where(x => x.MasterInstrument.Name == shortName && x.MasterInstrument.InstrumentType == InstrumentType.Future && x.Expiry.Date > DateTime.Now)
					.OrderBy(o => o.Expiry.Date).First();
			
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
			Grid.SetColumn(comList, 8);
			menu.Children.Add(comList);
		}
		
		
		private void InsertWpfControls() {
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
		}

		private void DisposeWpfControls() {
			if (chartWindow != null) chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
			RemoveWpfControls();
		}
		private void RemoveWpfControls() {
			if (menu != null) {
				chartGrid.RowDefinitions.RemoveAt(Grid.GetRow(menu));
				chartGrid.Children.Remove(menu);
			}
			for (int i = 0; i < chartGrid.Children.Count; i++) {
				if (menu != null && Grid.GetRow(chartGrid.Children[i]) > 0 && Grid.GetRow(chartGrid.Children[i]) > Grid.GetRow(menu)) {
					Grid.SetRow(chartGrid.Children[i], Grid.GetRow(chartGrid.Children[i]) - 1);
				}
			}
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
