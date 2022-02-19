#region Using declarations
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
    public class SuriDrawingToolTile : DrawingToolTile {
		private		Border		b;
		private		Grid		grid;
		private		Thickness	margin;
		private		Point		startPoint;

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Name							= "Zeichenwerkzeuge";
				Description						= "Zeichenwerkzeuge Schnellmenü";
				IsOverlay						= true;
				IsChartOnly						= true;
				DisplayInDataBox				= false;
				PaintPriceMarkers				= false;
				IsSuspendedWhileInactive		= true;
				SelectedTypes					= new XElement("SelectedTypes");

				foreach (Type type in new[] {
					typeof(ExtendedLine), typeof(Line), typeof(PathTool),
					typeof(HorizontalLine), typeof(VerticalLine), 
					typeof(Rectangle), typeof(Text), typeof(Ruler),
					typeof(RectangleS), typeof(RectangleBarInfo),
					typeof(StrikingHighRectangle), typeof(StrikingLowRectangle),
				}) {
					if (type.FullName != null) {
						XElement el = new XElement(type.FullName);
						el.Add(new XAttribute("Assembly", "NinjaTrader.Custom"));
						SelectedTypes.Add(el);
					}
				}
				Left			= 5;
				Top				= -1;
				NumberOfRows	= 5;
			} else if (State == State.Historical) {
				if (IsVisible && ChartControl != null) {
					if (Top < 0) Top = 25;
					ChartControl.Dispatcher.InvokeAsync(() => { UserControlCollection.Add(CreateControl()); });
				}
			}
		}
		public override string DisplayName { get { return "Zeichenwerkzeuge"; } }

		private FrameworkElement CreateControl() {
			if (grid != null) return grid;

			grid = new Grid { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(Left, Top, 0, 0) };

			grid.ColumnDefinitions	.Add(new ColumnDefinition	{ Width		= new GridLength() });
			grid.ColumnDefinitions	.Add(new ColumnDefinition	{ Width		= new GridLength() });
			grid.RowDefinitions		.Add(new RowDefinition		{ Height	= new GridLength() });

			Brush	background	= Application.Current.FindResource("BackgroundMainWindow")	as Brush ?? Brushes.White;
			Brush	borderBrush	= Application.Current.FindResource("BorderThinBrush")		as Brush ?? Brushes.Black;

			Grid	g			= new Grid();
			g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
			g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });

			for (int r = 0; r < g.RowDefinitions.Count; r++) {
				System.Windows.Shapes.Ellipse e = new System.Windows.Shapes.Ellipse {
					Width				= 3,
					Height				= 3,
					HorizontalAlignment	= HorizontalAlignment.Center,
					VerticalAlignment	= VerticalAlignment.Center,
					Fill				= borderBrush
				};
				Grid.SetRow(e, r);
				g.Children.Add(e);
			}
			
			b = new Border {
				VerticalAlignment	= VerticalAlignment.Top,
				BorderThickness		= new Thickness(0, 1, 1, 1),
				BorderBrush			= borderBrush,
				Background			= background,
				Width				= 12,
				Height				= 24,
				Cursor				= System.Windows.Input.Cursors.Hand,
				Child				= g
			};

			b.MouseDown += (o, e) => {
				startPoint	= e.GetPosition(ChartPanel);
				margin		= grid.Margin;
				if (e.ClickCount > 1) {
					b.ReleaseMouseCapture();
					ChartControl.OnIndicatorsHotKey(this, null);
				} else {
					b.CaptureMouse();
				}
			};

			b.MouseUp += (o, e) => { b.ReleaseMouseCapture(); };

			b.MouseMove += (o, e) => {
				if (!b.IsMouseCaptured || grid == null || ChartPanel == null) return;
				Point newPoint	= e.GetPosition(ChartPanel);
				grid.Margin		= new Thickness	{
					Left		= Math.Max(0, Math.Min(margin.Left	+ (newPoint.X - startPoint.X), ChartPanel.ActualWidth	- grid.ActualWidth)),
					Top			= Math.Max(0, Math.Min(margin.Top	+ (newPoint.Y - startPoint.Y), ChartPanel.ActualHeight	- grid.ActualHeight))
				};
				Left			= grid.Margin.Left;
				Top				= grid.Margin.Top;
			};

			Grid.SetColumn(b, 1);

			grid.Children.Add(b);

			Grid			contentGrid		= new Grid();
			List<XElement>	elements		= SortElements(XElement.Parse(SelectedTypes.ToString()));
			int				column			= 0;
			int				count			= 0;
			FontFamily		fontFamily		= Application.Current.Resources["IconsFamily"] as FontFamily;
			Style			style			= Application.Current.Resources["LinkButtonStyle"] as Style;

			while (count < elements.Count) {
				if (contentGrid.ColumnDefinitions.Count <= column) {
					contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star)});
				}
				for (int j = 0; j < NumberOfRows && count < elements.Count; j++) {
					if (contentGrid.RowDefinitions.Count <= j) {
						contentGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)});
					}
					XElement element = elements[count];
					try {
						DrawingTool dt = Core.Globals.AssemblyRegistry[element.Attribute("Assembly").Value].CreateInstance(element.Name.ToString()) as DrawingTool;
						if (dt != null && dt.DisplayOnChartsMenus) {
							Button bb = new Button {
								Content		= dt.Icon ?? Gui.Tools.Icons.DrawPencil,
								ToolTip		= dt.DisplayName,
								Style		= style,
								FontFamily	= fontFamily,
								FontSize	= 16,
								FontStyle	= FontStyles.Normal,
								Margin		= new Thickness(3),
								Padding		= new Thickness(3)
							};

							Grid.SetRow(bb, j);
							Grid.SetColumn(bb, column);

							bb.Click += (sender, args) => {
								if (ChartControl != null) ChartControl.TryStartDrawing(dt.GetType().FullName);
							};
							contentGrid.Children.Add(bb);
							count++;
						} else {
							elements.RemoveAt(j);
							j--;
						}
					} catch (Exception e) {
						elements.RemoveAt(j);
						j--;
						Cbi.Log.Process(typeof(Custom.Resource), "NinjaScriptTileError", new object[] { element.Name.ToString(), e }, LogLevel.Error, LogCategories.NinjaScript);
					}
				}
				column++;
			}

			Border tileHolder	= new Border {
				Cursor				= System.Windows.Input.Cursors.Arrow,
				Background			= Application.Current.FindResource("BackgroundMainWindow")as Brush,
				BorderThickness		= new Thickness ((double)(Application.Current.FindResource("BorderThinThickness") ?? 1)),
				BorderBrush			= Application.Current.FindResource("BorderThinBrush")as Brush,
				Child				= contentGrid
			};

			grid.Children.Add(tileHolder);

			return grid;
		}

		private List<XElement> SortElements(XElement elements) {
			string[] ordered =	{
				typeof(Ruler)					.FullName,
				typeof(Text)					.FullName,
				typeof(RiskReward)				.FullName,
				typeof(RegionHighlightX)		.FullName,
				typeof(RegionHighlightY)		.FullName,
				typeof(Line)		.FullName,
				typeof(ExtendedLine)			.FullName,
				typeof(PathTool)				.FullName,
				typeof(Ray)						.FullName,
				typeof(ArrowLine)				.FullName,
				typeof(HorizontalLine)			.FullName,
				typeof(VerticalLine)			.FullName,
				typeof(FibonacciRetracements)	.FullName,
				typeof(FibonacciExtensions)		.FullName,
				typeof(FibonacciTimeExtensions).FullName,
				typeof(FibonacciCircle)			.FullName,
				typeof(AndrewsPitchfork)		.FullName,
				typeof(GannFan)					.FullName,
				typeof(DrawingTools.RegressionChannel)		.FullName,
				typeof(TrendChannel)			.FullName,
				typeof(TimeCycles)				.FullName,
				typeof(Ellipse)					.FullName,
				typeof(Rectangle)				.FullName,
				typeof(Triangle)				.FullName,
				typeof(Polygon)					.FullName,
				typeof(Arc)						.FullName,
				typeof(ArrowUp)					.FullName,
				typeof(ArrowDown)				.FullName,
				typeof(Diamond)					.FullName,
				typeof(Dot)						.FullName,
				typeof(Square)					.FullName,
				typeof(TriangleUp)				.FullName,
				typeof(TriangleDown)			.FullName
			};

			List<XElement> ret = new List<XElement>();
			foreach (string s in ordered) {
				XElement c = elements.Element(s);
				if (c != null) {
					ret.Add(XElement.Parse(c.ToString()));
					c.Remove();
				}
			}
			foreach (XElement element in elements.Elements()) ret.Add(element);
			return ret;
		}
	}
}









































//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Suri.SuriDrawingToolTile[] cacheSuriDrawingToolTile;
		public Suri.SuriDrawingToolTile SuriDrawingToolTile()
		{
			return SuriDrawingToolTile(Input);
		}

		public Suri.SuriDrawingToolTile SuriDrawingToolTile(ISeries<double> input)
		{
			if (cacheSuriDrawingToolTile != null)
				for (int idx = 0; idx < cacheSuriDrawingToolTile.Length; idx++)
					if (cacheSuriDrawingToolTile[idx] != null &&  cacheSuriDrawingToolTile[idx].EqualsInput(input))
						return cacheSuriDrawingToolTile[idx];
			return CacheIndicator<Suri.SuriDrawingToolTile>(new Suri.SuriDrawingToolTile(), input, ref cacheSuriDrawingToolTile);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Suri.SuriDrawingToolTile SuriDrawingToolTile()
		{
			return indicator.SuriDrawingToolTile(Input);
		}

		public Indicators.Suri.SuriDrawingToolTile SuriDrawingToolTile(ISeries<double> input )
		{
			return indicator.SuriDrawingToolTile(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Suri.SuriDrawingToolTile SuriDrawingToolTile()
		{
			return indicator.SuriDrawingToolTile(Input);
		}

		public Indicators.Suri.SuriDrawingToolTile SuriDrawingToolTile(ISeries<double> input )
		{
			return indicator.SuriDrawingToolTile(input);
		}
	}
}

#endregion
