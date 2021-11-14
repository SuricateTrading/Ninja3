#region Using declarations
using System;
using System.Windows;
using System.Windows.Media;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;

using System.Windows.Controls;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class ChartToolbar : Indicator {
		
		private NinjaTrader.Gui.Chart.Chart				chartWindow;
		private System.Windows.Controls.Grid			chartGrid;
		private System.Windows.Controls.Menu			menu;
		
		private bool									isPanelActive;
		private int										tabControlStartColumn;
		private int										tabControlStartRow;
		private System.Windows.Controls.TabItem			tabItem;
		private NinjaTrader.Gui.Chart.ChartTab			chartTab;

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description							= @"";
				Name								= "Toolbar";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= false;
				IsSuspendedWhileInactive			= true;
			} else if (State == State.Historical) {
				if (ChartControl != null)
					ChartControl.Dispatcher.InvokeAsync((Action)(() => CreateWPFControls()));
			} else if (State == State.Terminated) {
				if (ChartControl != null)
					ChartControl.Dispatcher.InvokeAsync((Action)(() => DisposeWPFControls()));
			}
		}
		
        public override string DisplayName {
          get { return "Toolbar"; }
        }

		protected void CreateWPFControls() {
			chartWindow	= System.Windows.Window.GetWindow(ChartControl.Parent) as NinjaTrader.Gui.Chart.Chart;
			chartGrid	= chartWindow.MainTabControl.Parent as Grid;
			
			menu = new Menu() {
				Background			= new System.Windows.Media.SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
				VerticalAlignment	= VerticalAlignment.Center,
			};
			
			foreach (Coms coms in Enum.GetValues(typeof(Coms))) {
				MenuItem menuItem = new MenuItem() {
					Margin = new System.Windows.Thickness(0, 0, 5, 0),
					Padding = new System.Windows.Thickness(10, 0, 10, 2),
					Background = new System.Windows.Media.SolidColorBrush(Color.FromRgb(64, 63, 69)),
					Foreground = new System.Windows.Media.SolidColorBrush(Color.FromRgb(204, 204, 204)),
					FontSize = 16,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment	= VerticalAlignment.Center,
				};
				menuItem.Header = new System.Windows.Controls.TextBlock() {
					Text				= SuriStrings.ComsDShort[coms],
					ToolTip				= SuriStrings.ComsD[coms],
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment	= VerticalAlignment.Center,
				};
				menu.Items.Add(menuItem);
			}
			
			
			if (TabSelected()) InsertWPFControls();
			
			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		private void DisposeWPFControls() {
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
			RemoveWPFControls();
		}

		protected override void OnBarUpdate() {}
		
		protected void InsertWPFControls() {
			if (isPanelActive) return;

			if (chartGrid.RowDefinitions.Count == 0)
				chartGrid.RowDefinitions.Add(new RowDefinition() { /*Height = new GridLength(1, GridUnitType.Star)*/ });

			tabControlStartColumn	= Grid.GetColumn(chartWindow.MainTabControl);
			tabControlStartRow		= Grid.GetRow(chartWindow.MainTabControl);

			chartGrid.RowDefinitions.Insert(tabControlStartRow, new RowDefinition() { Height = new GridLength(45) });

			// including the chartTabControl move all items right of the chart and below the chart to the right one column and down one row
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
		
		protected void RemoveWPFControls() {
			if (!isPanelActive) return;
			
			if (menu != null) {
				chartGrid.RowDefinitions.RemoveAt(Grid.GetRow(menu));
				chartGrid.Children.Remove(menu);
			}
			
			for (int i = 0; i < chartGrid.Children.Count; i++) {
				if (Grid.GetRow(chartGrid.Children[i]) > 0 && Grid.GetRow(chartGrid.Children[i]) > Grid.GetRow(menu))
					Grid.SetRow(chartGrid.Children[i], Grid.GetRow(chartGrid.Children[i]) - 1);
			}
			
			isPanelActive = false;
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		private bool TabSelected() {
			bool tabSelected = false;
			
			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;
			
			return tabSelected;
		}

		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			if (e.AddedItems.Count <= 0)
				return;
			
			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null)
				return;
			
			chartTab = tabItem.Content as NinjaTrader.Gui.Chart.ChartTab;
			if (chartTab == null)
				return;
			
			if (TabSelected())
				InsertWPFControls();
			else
				RemoveWPFControls();
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
