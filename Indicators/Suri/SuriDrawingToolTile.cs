#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Suri {
	public class SuriDrawingToolTile : Indicator {
		private		Border		b;
		private		Grid		grid;
		private		Thickness	margin;
		private		bool		subscribedToSize;
		private		Point		startPoint;
		
		#region Properties
		[XmlIgnore] [Range(0, int.MaxValue)] [Display(Name="Oben", Order=0, GroupName="Parameter")]
		public double Top { get; set; }
		[XmlIgnore] [Range(0, int.MaxValue)] [Display(Name="Links", Order=1, GroupName="Parameter")]
		public double Left { get; set; }

		#endregion

		protected override void OnBarUpdate() {
			if (!subscribedToSize && ChartPanel != null) {
				subscribedToSize = true;

				ChartPanel.SizeChanged += (o, e) => {
					if (grid == null || ChartPanel == null) return;
					if (grid.Margin.Left + grid.ActualWidth > ChartPanel.ActualWidth || grid.Margin.Top + grid.ActualHeight > ChartPanel.ActualHeight) {
						double left	= Math.Max(0, Math.Min(grid.Margin.Left, ChartPanel.ActualWidth - grid.ActualWidth));
						double top	= Math.Max(0, Math.Min(grid.Margin.Top, ChartPanel.ActualHeight - grid.ActualHeight));
						grid.Margin	= new Thickness(left, top, 0, 0);
						Left		= left;
						Top			= top;
					}
				};
			}
		}

		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				Name							= "Zeichenwerkzeuge";
				Description						= "";
				IsOverlay						= true;
				IsChartOnly						= true;
				DisplayInDataBox				= false;
				PaintPriceMarkers				= false;
				IsSuspendedWhileInactive		= true;
				Left							= 300;
				Top								= 0;
			} else if (State == State.Historical && IsVisible && ChartControl != null) {
				if (Top < 0) Top = 0;
				ChartControl.Dispatcher.InvokeAsync(() => { UserControlCollection.Add(CreateControl()); });
			}
		}
		public override string DisplayName { get { return Name; } }
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) { }
		
		public override void CopyTo(NinjaScript ninjaScript) {
			SuriDrawingToolTile dti = ninjaScript as SuriDrawingToolTile;
			if (dti != null) {
				dti.Left	= Left;
				dti.Top		= Top;
			}
			base.CopyTo(ninjaScript);
		}

		private FrameworkElement CreateControl() {
			if (grid != null) return grid;

			grid = new Grid { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(Left, Top, 0, 0) };
			grid.ColumnDefinitions	.Add(new ColumnDefinition	{ Width		= new GridLength() });
			grid.ColumnDefinitions	.Add(new ColumnDefinition	{ Width		= new GridLength() });
			grid.RowDefinitions		.Add(new RowDefinition		{ Height	= new GridLength() });
			Brush	background	= Application.Current.FindResource("BackgroundMainWindow")	as Brush ?? Brushes.White;
			Brush	borderBrush	= Application.Current.FindResource("BorderThinBrush")		as Brush ?? Brushes.Black;
			Grid g = new Grid();
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
					Left	= Math.Max(0, Math.Min(margin.Left	+ (newPoint.X - startPoint.X), ChartPanel.ActualWidth	- grid.ActualWidth)),
					Top		= Math.Max(0, Math.Min(margin.Top	+ (newPoint.Y - startPoint.Y), ChartPanel.ActualHeight	- grid.ActualHeight))
				};
				Left	= grid.Margin.Left;
				Top		= grid.Margin.Top;
			};
			Grid.SetColumn(b, 1);
			grid.Children.Add(b);

			
			Grid contentGrid = new Grid();
			contentGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)});
			for (int i = 1; i <= 11; i++) {
				contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star)});
				string drawingToolname = "";
				object content = null;
				switch (i) {
					case 1: drawingToolname = "RectangleS";			content = new Image { Source = new BitmapImage(new Uri(SuriAddOn.path + "rectanglePlus.png",	UriKind.Absolute)), Width = 16, Height = 16}; break;
					case 2: drawingToolname = "RectangleBarInfo";	content = new Image { Source = new BitmapImage(new Uri(SuriAddOn.path + "barinfo.png",		UriKind.Absolute)), Width = 16, Height = 16 }; break;
					case 3: drawingToolname = "SuriRuler";			content = new Image { Source = new BitmapImage(new Uri(SuriAddOn.path + "ruler.png",			UriKind.Absolute)), Width = 16, Height = 16 }; break;
					case 4: drawingToolname = "Cot1Box";			content = new Image { Source = new BitmapImage(new Uri(SuriAddOn.path + "cot1.png",			UriKind.Absolute)), Width = 16, Height = 16 }; break;
					case 5: drawingToolname = "Cot2Box";			content = new Image { Source = new BitmapImage(new Uri(SuriAddOn.path + "cot2.png",			UriKind.Absolute)), Width = 16, Height = 16 }; break;
					case 6: drawingToolname = "Line";				content = Gui.Tools.Icons.DrawLineTool;			break;
					case 7: drawingToolname = "PathTool";			content = Gui.Tools.Icons.DrawPath;				break;
					case 8: drawingToolname = "HorizontalLine";		content = Gui.Tools.Icons.DrawHorizLineTool;	break;
					case 9: drawingToolname = "VerticalLine";		content = Gui.Tools.Icons.DrawVertLineTool;		break;
					case 10: drawingToolname = "Rectangle";			content = Gui.Tools.Icons.DrawRectangle;		break;
					case 11: drawingToolname = "Text";				content = Gui.Tools.Icons.DrawText;				break;
				}
				Button image = new Button {
					Content		= content,
					Style		= Application.Current.Resources["LinkButtonStyle"] as Style,
					FontFamily	= Application.Current.Resources["IconsFamily"] as FontFamily,
					FontSize	= 16,
					FontStyle	= FontStyles.Normal,
					Margin		= new Thickness(15, 2, 0, 0),
				};
				image.Click += (o, args) => ChartControl.TryStartDrawing("NinjaTrader.NinjaScript.DrawingTools." + drawingToolname);
				Grid.SetRow(image, 0);
				Grid.SetColumn(image, i-1);
				contentGrid.Children.Add(image);
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
