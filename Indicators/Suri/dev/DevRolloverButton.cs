#region Using declarations
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri.dev {
	public class DevRolloverButton : Indicator {
		private ChartTab						chartTab;
		private Chart							chartWindow;
		private TabItem							tabItem;
		
		private string							buttonText, nextExpiryString;
		private bool							hideButton, isPlayback, panelActive;
		private int								rollColIndex;
		private Button							rolloverButton;

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Name										= "Dev Rollover Button";
				Description									= @"Button um zum nächsten Kontraktmonat zu springen";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				ScaleJustification							= ScaleJustification.Overlay;
				IsSuspendedWhileInactive					= true;
			} else if (State == State.Historical) {
				hideButton			= false;
				isPlayback			= false;
				if (Account.All.FirstOrDefault(a => a.Name == "Playback101").ConnectionStatus == ConnectionStatus.Connected)
					isPlayback = true;
				CalculateRollDate();
				if (ChartControl != null) {
					ChartControl.Dispatcher.InvokeAsync(CreateWpfControls);
				}
			} else if (State == State.Terminated) {
				if (ChartControl != null) {
					ChartControl.Dispatcher.InvokeAsync(DisposeWpfControls);
				}
			}
		}
		
		private void CreateWpfControls() {
			if (Instrument.MasterInstrument.InstrumentType != InstrumentType.Future) return;

			rolloverButton = new Button {
				Content				= buttonText,
				Margin				= new Thickness(3, 0, 3, 0),
				Padding				= new Thickness(3, 2, 3, 2),
				VerticalAlignment	= VerticalAlignment.Center
			};
			rolloverButton.Click	+= RolloverBtn_Click;

			chartWindow			= Window.GetWindow(ChartControl.Parent) as Chart;
			if (TabSelected()) InsertWpfControls();
			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		private void CalculateRollDate() {
			if (Instrument.MasterInstrument.InstrumentType != InstrumentType.Future) {
				hideButton = true;
				return;
			}

			int activeExpiry	= -1;
			int nextRollCount	= -1;
			rollColIndex		= -1;	

			// loop through all rollovers looking for..
			for (int i = 0; i < Instrument.MasterInstrument.RolloverCollection.Count; i++) {
				// .. the next expiry after the one on the chart
				if (Instrument.Expiry == Instrument.MasterInstrument.RolloverCollection[i].ContractMonth && nextRollCount < 0)
					nextRollCount = i + 1;

				// ..the expiry used for the first historical bar on the chart for drawing roll date lines
				if (Count > 0 && Time.GetValueAt(0) < Instrument.MasterInstrument.RolloverCollection[i].Date && rollColIndex < 0)
					rollColIndex = i;

				// .. the expiry that should be used based on todays date and the rollover dates
				if (activeExpiry == -1
					&& Instrument.MasterInstrument.RolloverCollection[i].Date > (isPlayback ? Connection.PlaybackConnection.Now.Date : Core.Globals.Now))
					activeExpiry = i - 1;
			}

			// if we are on the last expiry skip making the button
			if (nextRollCount >= Instrument.MasterInstrument.RolloverCollection.Count) {
				hideButton = true;
			} else {
				// set the expiry that will be used for the button action
				nextExpiryString = string.Format("{0} {1}", Instrument.MasterInstrument.Name, Instrument.MasterInstrument.RolloverCollection[nextRollCount]);
				// and text of the button
				buttonText = Instrument.MasterInstrument.RolloverCollection[nextRollCount].ToString();
			}
		}

		private void DisposeWpfControls() {
			if (rolloverButton != null) rolloverButton.Click -= RolloverBtn_Click;
			if (chartWindow != null) chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
			RemoveWpfControls();
		}

		private void InsertWpfControls() {
			if (hideButton || panelActive) return;
			// let the script know the panel is active
			chartWindow.MainMenu.Add(rolloverButton);
			panelActive = true;
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate) {
			if (connectionStatusUpdate.Connection.Options.Name == "Playback Connection")
				isPlayback = true;
		}

		private void RemoveWpfControls() {
			if (!panelActive) return;
			if (rolloverButton != null) chartWindow.MainMenu.Remove(rolloverButton);
			panelActive = false;
		}

		private void RolloverBtn_Click(object sender, RoutedEventArgs e) {
			DependencyObject scope = FocusManager.GetFocusScope(rolloverButton); 
			FocusManager.SetFocusedElement(scope, null);
			Keyboard.ClearFocus();
			ChartControl.OwnerChart.Focus();

			Keyboard.FocusedElement.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
					new TextComposition(InputManager.Current, ChartControl.OwnerChart, "open sesame"))
						{ RoutedEvent = TextCompositionManager.PreviewTextInputEvent });
			Keyboard.FocusedElement.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
					new TextComposition(InputManager.Current, ChartControl.OwnerChart, nextExpiryString))
						{ RoutedEvent = TextCompositionManager.TextInputEvent });
			Keyboard.FocusedElement.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,
				PresentationSource.FromVisual(ChartControl.OwnerChart), 0,  Key.Enter) { RoutedEvent = Keyboard.PreviewKeyDownEvent } );
		}

		private bool TabSelected() {
			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (TabItem tab in chartWindow.MainTabControl.Items) {
				if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					return true;
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

























//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.dev.DevRolloverButton[] cacheDevRolloverButton;
		public Suri.dev.DevRolloverButton DevRolloverButton()
		{
			return DevRolloverButton(Input);
		}

		public Suri.dev.DevRolloverButton DevRolloverButton(ISeries<double> input)
		{
			if (cacheDevRolloverButton != null)
				for (int idx = 0; idx < cacheDevRolloverButton.Length; idx++)
					if (cacheDevRolloverButton[idx] != null &&  cacheDevRolloverButton[idx].EqualsInput(input))
						return cacheDevRolloverButton[idx];
			return CacheIndicator<Suri.dev.DevRolloverButton>(new Suri.dev.DevRolloverButton(), input, ref cacheDevRolloverButton);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.dev.DevRolloverButton DevRolloverButton()
		{
			return indicator.DevRolloverButton(Input);
		}

		public Indicators.Suri.dev.DevRolloverButton DevRolloverButton(ISeries<double> input )
		{
			return indicator.DevRolloverButton(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.dev.DevRolloverButton DevRolloverButton()
		{
			return indicator.DevRolloverButton(Input);
		}

		public Indicators.Suri.dev.DevRolloverButton DevRolloverButton(ISeries<double> input )
		{
			return indicator.DevRolloverButton(input);
		}
	}
}

#endregion
