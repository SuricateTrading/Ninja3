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
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
#endregion

namespace NinjaTrader.NinjaScript.AddOns {
	public class SuriChart : NinjaTrader.NinjaScript.AddOnBase {
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Description		= @"";
				Name			= "SuriChart";
			}
		}
		
		private Button exampleButton;
		
		protected override void OnWindowCreated(Window window) {
			Chart chartWindow = window as Chart;
			if (chartWindow == null) return;
			
			
	System.Windows.Controls.ToolBar chartToolBar = Window.GetWindow(chartWindow.Parent).FindFirst("ChartWindowToolBar") as System.Windows.Controls.ToolBar;

	System.Windows.Controls.Grid parentGrid = chartToolBar.Parent as System.Windows.Controls.Grid;
	System.Windows.Controls.ToolBar myToolBar = new System.Windows.Controls.ToolBar
	{
		Background = Brushes.Blue,
		Height = 200
	};
	chartWindow.MainMenu.Add(myToolBar);
	
			
			bool isToolBarButtonAdded = false;
			foreach (DependencyObject item in chartWindow.MainMenu) {
				if (AutomationProperties.GetAutomationId(item) == "exampleButton 4") {
					isToolBarButtonAdded = true;
					break;
				}
			}
			
			if (!isToolBarButtonAdded) {
				exampleButton = new Button { Content = "Example Button 4", };
				exampleButton.Click += ExampleButtonClick;
				AutomationProperties.SetAutomationId(exampleButton, "exampleButton 4");
				chartWindow.MainMenu.Add(exampleButton);
			}
		}
		
		protected override void OnWindowDestroyed(Window window) {
			Chart chartWindow = window as Chart;
			if (exampleButton == null || chartWindow == null) return;
			
			if (exampleButton != null) {
				chartWindow.MainMenu.Remove(exampleButton);						
				exampleButton.Click -= ExampleButtonClick;
				exampleButton = null;
			}
			chartWindow.MainTabControl.SelectionChanged -= MySelectionChangedHandler;
		}
		
		
		private void ExampleButtonClick(object sender, RoutedEventArgs e) {
			Print("Button is working"); 
		}
		
		
		private void MySelectionChangedHandler(object sender, SelectionChangedEventArgs e) {
			/*if (e.AddedItems.Count <= 0) return;
			TabItem tabItem = e.AddedItems[0] as TabItem;
			if (tabItem == null) return;
			ChartTab temp = tabItem.Content as ChartTab; 
			if (temp != null) {
				if (exampleButton != null)
					exampleButton.Visibility = temp.ChartControl == ChartControl ? Visibility.Visible : Visibility.Collapsed;
			}*/
		}
		
	}
}
