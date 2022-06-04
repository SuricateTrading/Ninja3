#region Using declarations
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using NinjaTrader.Gui.Tools;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using NinjaTrader.Core;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Custom.AddOns.SuriCommon.Vp;
using NinjaTrader.NinjaScript;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
using TabControl = System.Windows.Controls.TabControl;
#endregion

namespace NinjaTrader.Gui.NinjaScript {
    public sealed class SuriAddOnWindow : NTWindow, IWorkspacePersistence {
		public static SuriAddOnWindow current;
		private Page page;
		private readonly List<ChartWindowData> charts = new List<ChartWindowData>();
		
		public SuriAddOnWindow() {
			Caption = "Suricate Trading";
			Content = LoadXaml();
			current = this;
			foreach (var window in NinjaTrader.Core.Globals.AllWindows) {
				Chart.Chart chart = window as Chart.Chart;
				if (chart == null) continue;
				chart.Dispatcher.Invoke(() => {
					Grid g = chart.Content as Grid;
					if (g != null) {
						foreach (var control in g.Children) {
							TabControl t = control as TabControl;
							if (t != null) {
								charts.Add(new ChartWindowData(t.Items, chart));
							}
						}
					}
				});
				Redraw();
			}
		}

		private DependencyObject LoadXaml() {
			try {
				StreamReader reader = new StreamReader(Path.Combine(Globals.UserDataDir, SuriAddOn.path + "SuriMain.xaml"), Encoding.UTF8); 
				page = (Page) XamlReader.Load(reader.BaseStream);
				LoadControlEvents();
				return page.Content as DependencyObject;
			} catch (Exception) { return null; }
		}

		private void Redraw() {
			StackPanel manageGrid = LogicalTreeHelper.FindLogicalNode(page, "ManageGrid") as StackPanel;
			if (manageGrid == null || charts.IsNullOrEmpty()) return;
			manageGrid.Children.Clear();
			int windowCount = 0;
			foreach (var chartWindow in charts) {
				windowCount++;
				WrapPanel grid = new WrapPanel {
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Center,
				};
				grid.Children.Add(new TextBlock {
					FontSize = 16,
					Text = "  Fenster " + windowCount + ": ",
					VerticalAlignment = VerticalAlignment.Center,
				});
				int x = 0;
				foreach (TabItem tab in chartWindow.tabs) {
					Button b = new Button {
						Content = chartWindow.names[x],
						Width = 100,
						Margin = new Thickness(5),
						FontSize = 16,
						BorderBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FF0B76C4")),
					};
					var tab1 = tab;
					b.Click += (sender, args) => {
						if (tab1 == null) return;
						tab1.Dispatcher.Invoke(() => {
							tab1.IsSelected = true;
							Window w = GetWindow(tab1);
							if (w == null) return;
							w.Activate();
						});
					};
					grid.Children.Add(b);
					x++;
				}

				manageGrid.Children.Add(
					new Border {
						BorderBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FF0B76C4")),
						Child = grid,
						BorderThickness = new Thickness(0.5),
						Margin = new Thickness(0, 3, 0, 25),
					}
				);
			}
		}
		
		private void LoadControlEvents() {
			Image ninjatImage = LogicalTreeHelper.FindLogicalNode(page, "NinjatImage") as Image;
			if (ninjatImage != null) {
				ninjatImage.Source = new BitmapImage(new Uri(SuriAddOn.path + "ninjat.jpg", UriKind.Absolute));
			}
			
			Hyperlink website		= LogicalTreeHelper.FindLogicalNode(page, "website")		as Hyperlink;
			Hyperlink terminkurven	= LogicalTreeHelper.FindLogicalNode(page, "terminkurven")	as Hyperlink;
			Hyperlink seasonals		= LogicalTreeHelper.FindLogicalNode(page, "seasonals")	as Hyperlink;
			Hyperlink contact		= LogicalTreeHelper.FindLogicalNode(page, "contact")		as Hyperlink;
			if (website			!= null) { website.RequestNavigate		+= (sender, e) => { Process.Start(e.Uri.ToString()); }; }
			if (terminkurven	!= null) { terminkurven.RequestNavigate	+= (sender, e) => { Process.Start(e.Uri.ToString()); }; }
			if (seasonals		!= null) { seasonals.RequestNavigate	+= (sender, e) => { Process.Start(e.Uri.ToString()); }; }
			if (contact			!= null) { contact.RequestNavigate		+= (sender, e) => { Process.Start(e.Uri.ToString()); }; }
			
			TextBlock suricateTextBlock = LogicalTreeHelper.FindLogicalNode(page, "Version") as TextBlock;
			if (suricateTextBlock != null) {
				suricateTextBlock.Text += " " + SuriCommon.version + " \n" + (SuriCommon.isUpToDate
					? "Du bist auf dem neusten Stand."
					: "Die neuste Version ist " + SuriCommon.mostRecentVersion);
			}
			
			Button downloadTool = LogicalTreeHelper.FindLogicalNode(page, "DownloadTool") as Button;
			if (downloadTool != null) {
				downloadTool.Click += (sender, args) => {
					using (WebClient webClient = new WebClient()) {
						string fileName = "Suri_plugin_" + SuriCommon.mostRecentVersion + ".zip";
						webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/" + fileName, SuriAddOn.path + @"downloads\" + fileName);
					}
					Process.Start(SuriAddOn.path + @"downloads\");
				};
			}
			
			Button downloadWorkspace = LogicalTreeHelper.FindLogicalNode(page, "DownloadWorkspace") as Button;
			if (downloadWorkspace != null) {
				downloadWorkspace.Click += (sender, args) => {
					using (WebClient webClient = new WebClient()) {
						SaveFileDialog saveFileDialog = new SaveFileDialog {
							Title = @"Workspace speichern",
							Filter = @"Workspace (*.xml)|*.xml",
							InitialDirectory = Globals.UserDataDir + @"workspaces\",
							FileName = SuriAddOn.suri.Vp ? "VP_Workspace.xml" : "COT_Workspace.xml",
						};
						if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
							if (SuriAddOn.suri.Vp) {
								webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/VP_Workspace.xml", saveFileDialog.FileName);
							} else {
								webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/COT_Workspace.xml", saveFileDialog.FileName);
							}
						}
					}
				};
			}
			
			TextBlock licenseUntil = LogicalTreeHelper.FindLogicalNode(page, "LicenseUntil") as TextBlock;
			if (licenseUntil != null && SuriAddOn.suri.Until != null) {
				licenseUntil.Text += " " + DateTime.Parse(SuriAddOn.suri.Until).ToString("dd.MM.yyyy");
			}
			Button extendLicense = LogicalTreeHelper.FindLogicalNode(page, "ExtendLicense") as Button;
			if (extendLicense != null) {
				extendLicense.Click += (sender, args) => {
					Process.Start("mailto:tools@suricate-trading.de?subject=Lizenz verlängern");
				};
			}
			
			if (SuriAddOn.suri.license == License.Dev) InitAdminMode(page);

			Redraw();
		}

		private void InitAdminMode(Page page) {
			TabControl tabs = LogicalTreeHelper.FindLogicalNode(page, "tabs") as TabControl;
			if (tabs == null) return;

			Button downloadVpBig		= new Button { Content = "Download VP Big"		, Padding = new Thickness(15), Margin = new Thickness(5) };
			Button downloadVpBigDev		= new Button { Content = "Download VP Big Dev"	, Padding = new Thickness(15), Margin = new Thickness(5) };
			Button downloadVpIntra		= new Button { Content = "Download VP Intra"	, Padding = new Thickness(15), Margin = new Thickness(5) };
			Button correlation			= new Button { Content = "Korrelation"			, Padding = new Thickness(15), Margin = new Thickness(5) };
			Button downloadTicks		= new Button { Content = "Download Ticks"		, Padding = new Thickness(15), Margin = new Thickness(5) };
			Button suriStatistics		= new Button { Content = "Suri Statistics"		, Padding = new Thickness(15), Margin = new Thickness(5) };
			downloadVpBig.Click		+= (sender, args) => SuriVpBigScripts.StoreVpBigToFile();
			downloadVpBigDev.Click	+= (sender, args) => SuriVpBigScripts.StoreVpBigToFile(true);
			downloadVpIntra.Click	+= (sender, args) => SuriVpIntraScripts.StoreVpIntra();
			correlation.Click		+= (sender, args) => new DevCorrelation().LoadData();
			downloadTicks.Click		+= (sender, args) => SuriVpIntraScripts.StoreTickData();
			suriStatistics.Click	+= (sender, args) => new DevStatistics().Start();
			
			tabs.Items.Insert(0, new TabItem {
				Header = "Admin",
				Padding = new Thickness(10.5),
				Content = new ScrollViewer {
					Content = new StackPanel {
						Children = { downloadVpBig, downloadVpBigDev, downloadVpIntra, correlation, downloadTicks, suriStatistics }
					}
				}
			});
		}
		
		public void Restore(XDocument document, XElement element) { }
		public void Save(XDocument document, XElement element) { }
		public WorkspaceOptions WorkspaceOptions { get; set; }
	}

	public sealed class ChartWindowData {
		public List<string> names = new List<string>();
		public ItemCollection tabs;

		public ChartWindowData(ItemCollection tabs, Chart.Chart chart) {
			foreach (TabItem tab in tabs) {
				names.Add(tab.Header as string);
			}
			this.tabs = tabs;
		}
	}
}