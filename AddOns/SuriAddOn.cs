#region Using declarations
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
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
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
using TabControl = System.Windows.Controls.TabControl;

#endregion

namespace NinjaTrader.Gui.NinjaScript {
	public sealed class ChartWindowData {
		public List<string> names = new List<string>();
		public ItemCollection tabs;
		public Chart.Chart chart;

		public ChartWindowData(ItemCollection tabs, Chart.Chart chart) {
			foreach (TabItem tab in tabs) {
				names.Add(tab.Header as string);
			}
			this.tabs = tabs;
			this.chart = chart;
		}

		/*public string GetName(int index) {
			TabItem t = (tabs[index] as TabItem);
			if (t == null) return "";
			return t.Dispatcher.Invoke(() => t.Header as string);
		}*/

	}
	
	public sealed class SuriAddOn : AddOnBase {
		private NTMenuItem startSuri;
		public static string path;

		public static List<ChartWindowData> charts = new List<ChartWindowData>();
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Name 		= "Suri Tool";
				Description = "Suri Tool";
			}
		}

		public static readonly Suri suri = SuriServer.GetSuri(Cbi.License.MachineId);
		public static License license { get { return suri.license; } }
		
		protected override void OnWindowCreated(Window window) {
			/*Chart.Chart chart = window as Chart.Chart;
			if (chart != null) {
				Grid g = chart.Content as Grid;
				if (g != null) {
					foreach (var control in g.Children) {
						TabControl t = control as TabControl;
						if (t != null) {
							charts.Add(new ChartWindowData(t.Items, chart));
						}
					}
				}
				if (SuriAddOnWindow.current != null) {
					SuriAddOnWindow.current.Dispatcher.Invoke(() => {
						SuriAddOnWindow.current.Redraw();
					});
				}
			}*/

			ControlCenter cc = window as ControlCenter;
			if (cc == null) return;

			path = Globals.UserDataDir + @"suri\";
			Directory.CreateDirectory(path + @"\downloads");
			
			using (WebClient webClient = new WebClient()) {
				bool isFirstInstall = false;
				string previouslyDownloadedVersion = "";
				try {
					previouslyDownloadedVersion = File.ReadAllText(path + "version.suri");
				} catch (Exception) {
					isFirstInstall = true;
				}
				webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/version.suri", path + "version.suri");
				SuriCommon.mostRecentVersion = File.ReadAllText(path + "version.suri");
				bool versionHasChanged = !previouslyDownloadedVersion.Equals(SuriCommon.mostRecentVersion);
				
				if (isFirstInstall || versionHasChanged) {
					webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/ninjat.jpg", path + "ninjat.jpg");
					webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/barinfo.png", path + "barinfo.png");
					webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/rectanglePlus.png", path + "rectanglePlus.png");
					webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/strikingHigh.png", path + "strikingHigh.png");
					webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/strikingLow.png", path + "strikingLow.png");
					webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/ruler.png", path + "ruler.png");
				}
				webClient.DownloadFile(@"https://app.suricate-trading.de/ninja/SuriMain.xaml", path + "SuriMain.xaml");
			}
		
			if (!Globals.MarketDataOptions.DownloadCotData) {
				Globals.MarketDataOptions.DownloadCotData = true;
				NTMessageBoxSimple.Show(window, "Bitte starte Ninja Trader erneut, damit erforderliche Einstellungen vorgenommen werden können.", "Erststart erkannt", MessageBoxButton.OK, MessageBoxImage.None);
			}
			
			startSuri = new NTMenuItem {
				Header = "Suricate Trading",
				Style = Application.Current.TryFindResource("MainMenuItem") as Style,
				Icon = Geometry.Parse("M 0.36134209,59.832282 C 0.38406148,48.88256 0.61813856,45.515001 1.6008535,42.000002 2.550822,38.60213 2.7196824,36.443372 2.3830644,32.000001 1.0666644,14.623466 1.0772343,13.842494 2.6792146,10.11859 5.4803854,3.6070933 11.636307,0.14870085 18.935186,0.9859971 c 1.868981,0.2144016 5.007584,0.1196218 6.974672,-0.21062174 4.677634,-0.78530197 8.049596,0.0168433 9.905505,2.35639004 3.812434,4.8059302 1.847414,4.4342435 20.607866,3.8980109 L 73.333336,6.5464321 v 3.0601176 c 0,3.0336903 -0.01951,3.0601173 -2.25914,3.0601173 -2.08136,0 -2.29592,0.196059 -2.72654,2.49147 -0.74628,3.978008 -4.636334,13.042495 -7.414971,17.278138 -3.268311,4.982083 -12.307461,13.88297 -17.159533,16.89706 l -3.756192,2.333334 -0.0085,3.542951 c -0.0061,2.524965 0.757727,5.57622 2.658188,10.619435 1.466666,3.892068 2.666666,7.194734 2.666666,7.339268 0,0.144526 -0.480768,0.16578 -1.068374,0.0472 -1.654059,-0.333687 -6.196125,-12.755336 -6.234046,-17.048854 l -0.03091,-3.5 h -3.633852 c -6.210291,0 -12.364847,-1.717406 -11.45136,-3.195459 0.276249,-0.44698 1.724521,-0.336268 4.252077,0.325046 2.188019,0.572478 5.473804,0.885017 7.655908,0.728218 3.227071,-0.231886 4.540845,-0.765328 8.430834,-3.42324 6.064285,-4.143542 14.616219,-13.122629 17.603126,-18.482368 4.303109,-7.721547 7.002968,-17.802841 5.08518,-18.9880996 -1.073242,-0.6633001 -1.923826,0.4734746 -3.952872,5.2828716 -4.184179,9.917652 -12.158244,19.068026 -20.055974,23.014568 -5.049043,2.523036 -6.830786,2.562139 -14.642562,0.321352 -7.194081,-2.063603 -9.702383,-1.68086 -13.912751,2.122955 -2.764677,2.497719 -4.2668507,2.941482 -3.923592,1.159083 0.3203607,-1.663501 5.505078,-5.37947 8.709709,-6.242387 2.251351,-0.606226 3.597325,-0.533055 7.138634,0.388078 8.843169,2.300205 10.038437,2.537328 10.493927,2.081838 0.255786,-0.255785 -0.906894,-1.731227 -2.583732,-3.27876 -3.121458,-2.880757 -4.374039,-5.871551 -3.548467,-8.472695 0.453806,-1.429816 3.518919,-3.341003 5.358216,-3.341003 0.598765,0 0.965164,-0.375 0.814221,-0.833333 -0.35806,-1.087242 -3.170003,-1.499487 -3.170003,-0.464737 0,0.599434 -0.245675,0.590772 -1,-0.03526 -0.55,-0.456459 -0.990878,-1.618193 -0.979728,-2.58163 0.01749,-1.511626 0.113403,-1.591807 0.699794,-0.585037 0.695,1.193244 4.070797,1.58881 6.290358,0.737084 0.61571,-0.23627 1.752233,-1.81127 2.525605,-3.5 0.773372,-1.688729 1.529694,-3.327061 1.680714,-3.640738 C 42.034414,11.44934 41.22218,11.398231 40.07843,11.649441 38.40815,12.016295 37.919736,11.86613 37.596718,10.886428 36.670336,8.0766783 34.258116,3.8552851 33.26903,3.3129253 32.683662,2.9919422 27.651821,2.829884 22.087161,2.9527959 12.40232,3.166714 11.853666,3.2529912 9.2590057,4.9700564 7.3481963,6.23457 6.0470164,7.8590383 4.8490958,10.475641 3.0174213,14.476545 3.0268305,13.999326 4.4115626,32.666668 4.758463,37.343175 4.5792264,39.233217 3.3857959,43.483344 2.0867529,48.109585 1.9861511,49.572233 2.3966172,57.864991 2.8831531,67.694602 2.4198178,73.334003 1.1258181,73.332283 0.53881953,73.331503 0.34059583,69.831036 0.36134209,59.832282 Z M 42.666668,35.228038 c 4.960054,-2.830997 11.077742,-9.311767 14.437395,-15.294247 3.318154,-5.908584 4.092422,-8.095254 2.673633,-7.550813 -0.536888,0.206023 -1.17421,0.05413 -1.416271,-0.337528 -0.542425,-0.877662 -9.694757,-0.957712 -9.694757,-0.08479 0,0.345027 0.301102,0.813412 0.669114,1.040856 1.615394,0.998369 -1.819164,8.534889 -4.508988,9.894165 -1.004736,0.507733 -3.712517,1.086439 -6.017292,1.286013 -4.673403,0.404678 -7.476168,1.84426 -7.476168,3.83998 0,1.116096 6.320826,9.085767 7.333334,9.246302 0.183333,0.02907 1.983333,-0.888903 4,-2.039934 z m 2.012636,-15.061371 c 2.618363,-2.360427 3.48187,-6.480737 1.490638,-7.112729 -2.147774,-0.681678 -3.557718,0.29587 -4.881702,3.384597 -0.802036,1.871074 -1.866003,3.147466 -2.975167,3.569169 -2.380055,0.904895 -1.44359,1.992297 1.715755,1.992297 1.917241,0 3.160477,-0.490115 4.650476,-1.833334 z M 8.7173143,18.246303 C 6.6456798,15.612646 5.7748141,11.70234 6.6615318,9.0155597 7.8986556,5.2670381 11.097965,4.9063335 13.63274,8.2295943 c 1.958657,2.5679327 2.218075,5.9080587 0.734612,9.4584857 -1.215888,2.910029 -3.613754,3.146937 -5.6500377,0.558223 z m 4.1958287,-1.746302 c 1.018992,-3.802355 -0.436188,-8.5000007 -2.633027,-8.5000007 -1.3632737,0 -2.3674177,2.4098717 -1.869357,4.4863207 1.172342,4.887574 3.671208,7.115203 4.502384,4.01368 z M 28.920802,12.306024 C 28.03197,10.949494 27.95966,10.285483 28.557808,8.972691 29.35918,7.2138709 30.843932,6.9267983 33.640352,7.989995 c 2.450759,0.9317773 2.797496,2.458004 0.994449,4.377259 -1.995875,2.124513 -4.298009,2.099843 -5.713999,-0.06123 z m 5.745866,-1.557727 c 0,-0.8525026 -1.960251,-2.3436007 -2.444445,-1.859408 -0.484193,0.4841933 1.006906,2.444445 1.859408,2.444445 0.32177,0 0.585037,-0.263267 0.585037,-0.585037 z"),
			};
			
			startSuri.Click += (sender, args) => {
				//if (SuriAddOnWindow.current == null) {
					Globals.RandomDispatcher.BeginInvoke(new Action(() => new SuriAddOnWindow().Show()));
				/*} else {
					SuriAddOnWindow.current.Dispatcher.Invoke(() => {
						SuriAddOnWindow.current.Activate();
					});
				}*/
			};

			if (!SuriCommon.isUpToDate) {
				NTMessageBoxSimple.Show(Window.GetWindow(window), "Hallo Trader,\nEs ist eine neue Version verfügbar!", "Neue Version", MessageBoxButton.OK, MessageBoxImage.None);
			}
			
			cc.MainMenu.Add(startSuri);
		}
		
		protected override void OnWindowDestroyed(Window window) {
			/*SuriAddOnWindow suriAddOnWindow = window as SuriAddOnWindow;
			if (suriAddOnWindow != null) {
				SuriAddOnWindow.current.Dispatcher.Invoke(() => {
					SuriAddOnWindow.current = null;
				});
			}
			
			Chart.Chart chart = window as Chart.Chart;
			if (chart != null) {
				charts.RemoveAll(data => data.chart == chart);
				if (SuriAddOnWindow.current != null) {
					SuriAddOnWindow.current.Dispatcher.Invoke(() => {
						SuriAddOnWindow.current.Redraw();
					});
				}
			}*/
			
			ControlCenter cc = window as ControlCenter;
			if (cc != null && startSuri != null) {
				cc.MainMenu.Remove(startSuri);
				startSuri = null;
			}
		}
	}

	
	
	
	public sealed class SuriAddOnWindow : NTWindow, IWorkspacePersistence {
		public static SuriAddOnWindow current;
		private Page page;
		
		public SuriAddOnWindow() {
			Caption = "Suricate Trading";
			Content = LoadXaml();
			current = this;
		}

		private DependencyObject LoadXaml() {
			try {
				StreamReader reader = new StreamReader(Path.Combine(Globals.UserDataDir, SuriAddOn.path + "SuriMain.xaml"), Encoding.UTF8); 
				page = (Page) XamlReader.Load(reader.BaseStream);
				LoadControlEvents();
				return page.Content as DependencyObject;
			} catch (Exception) { return null; }
		}

		public void Redraw() {
			StackPanel manageGrid = LogicalTreeHelper.FindLogicalNode(page, "ManageGrid") as StackPanel;
			if (manageGrid == null) return;
			manageGrid.Children.Clear();
			int windowCount = 0;
			foreach (var chartWindow in SuriAddOn.charts) {
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
						tab1.Dispatcher.Invoke(() => {
							tab1.IsSelected = true;
							Window w = GetWindow(tab1);
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

			//Redraw();
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
			downloadVpBig.Click		+= (sender, args) => SuriAdmin.StoreVpBigToFile();
			downloadVpBigDev.Click	+= (sender, args) => SuriAdmin.StoreVpBigToFile(true);
			downloadVpIntra.Click	+= (sender, args) => SuriAdmin.StoreVpIntra();
			correlation.Click		+= (sender, args) => new DevCorrelation().LoadData();
			downloadTicks.Click		+= (sender, args) => SuriAdmin.StoreTickData();
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


	
}
