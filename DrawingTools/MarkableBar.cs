#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.SuriCommon;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Brush = System.Windows.Media.Brush;
using DashStyle = SharpDX.Direct2D1.DashStyle;
using License = NinjaTrader.Custom.AddOns.SuriCommon.License;
using Point = System.Windows.Point;

#endregion

namespace NinjaTrader.NinjaScript.DrawingTools {
	public abstract class MarkableBar : DrawingTool 	{
		private enum ResizeMode {
			None,
			MoveAll,
		}
		
		private				int							areaOpacity;
		private				Brush						areaBrush;
		private	readonly	DeviceBrush					areaBrushDevice			= new DeviceBrush();
		private	const		double						CURSOR_SENSITIVITY		= 15;
		private				ChartAnchor					lastMouseMoveDataPoint;
		private				ResizeMode					resizeMode;
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolShapesAreaBrush", GroupName = "NinjaScriptGeneral", Order = 1)]
		public Brush AreaBrush {
			get { return areaBrush; }
			set {
				areaBrush = value;
				if (areaBrush != null) {
					if (areaBrush.IsFrozen) areaBrush = areaBrush.Clone();
					areaBrush.Freeze();
				}
				areaBrushDevice.Brush = null;
			}
		}

		[Browsable(false)]
		public string AreaBrushSerialize {
			get { return Serialize.BrushToString(AreaBrush); }
			set { AreaBrush = Serialize.StringToBrush(value); }
		}

		[Range(0,100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolAreaOpacity", GroupName = "NinjaScriptGeneral", Order = 2)]
		public int AreaOpacity {
			get { return areaOpacity; }
			set {
				areaOpacity = Math.Max(0, Math.Min(100, value));
				areaBrushDevice.Brush = null;
			}
		} 
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolTextOutlineStroke", GroupName = "NinjaScriptGeneral", Order = 3)]
		public Stroke				OutlineStroke	{ get; set; }

		[Display(Order = 1)]
		public ChartAnchor			StartAnchor		{ get; set; }
		[Display(Order = 2)]
		public ChartAnchor			EndAnchor		{ get; set;	}

		public override bool SupportsAlerts { get { return true; } }
		
		public override IEnumerable<ChartAnchor> Anchors { get { return new[] { StartAnchor, EndAnchor }; } }

		public override void OnCalculateMinMax() {
			MinValue = double.MaxValue;
			MaxValue = double.MinValue;

			if (!IsVisible) return;

			// return min/max values only if something has been actually drawn
			if (Anchors.Any(a => !a.IsEditing))
				foreach (ChartAnchor anchor in Anchors) {
					MinValue = Math.Min(anchor.Price, MinValue);
					MaxValue = Math.Max(anchor.Price, MaxValue);
				}
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			if (areaBrushDevice != null) areaBrushDevice.RenderTarget = null;
		}

		private Rect GetAnchorsRect(ChartControl chartControl, ChartScale chartScale) {
			if (StartAnchor == null || EndAnchor == null)
				return new Rect();
			
			ChartPanel chartPanel	= chartControl.ChartPanels[chartScale.PanelIndex];
			Point startPoint		= StartAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point endPoint 			= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);
			
			//rect doesnt handle negative width/height so we need to determine and wind it up ourselves
			// make sure to always use smallest left/top anchor for start
			double left 	= Math.Min(endPoint.X, startPoint.X) - chartControl.BarWidth;
			double top 		= Math.Min(endPoint.Y, startPoint.Y);
			double width 	= Math.Abs(endPoint.X - startPoint.X) + chartControl.BarWidth*2;
			double height 	= Math.Abs(endPoint.Y - startPoint.Y);
			return new Rect(left, top, width, height);
		}

		public override Cursor GetCursor(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, Point point) {
			if (DrawingState == DrawingState.Building) return Cursors.Pen;
			if (DrawingState == DrawingState.Moving) return IsLocked ? Cursors.No : Cursors.SizeAll;
			if (DrawingState == DrawingState.Editing && IsLocked) return Cursors.No;
			
			ResizeMode tmpResizeMode = resizeMode != ResizeMode.None ? resizeMode : GetResizeModeForPoint(point, chartControl, chartScale, DrawingState == DrawingState.Normal);
			switch (tmpResizeMode) {
				case ResizeMode.MoveAll:	return	IsLocked ? Cursors.Arrow : Cursors.SizeAll;
			}
			return null;
		}
		
		private static Point? GetClosestPoint(IEnumerable<Point> inputPoints, Point desired, bool useSensitivity) {
			IOrderedEnumerable<Point> ordered = inputPoints.OrderBy(pt => (pt - desired).Length);
			Point closestPoint = ordered.First();
			if (useSensitivity && (closestPoint - desired).Length > CURSOR_SENSITIVITY) return null;
			return closestPoint;
		}

		private ResizeMode GetResizeModeForPoint(Point pt, ChartControl chartControl, ChartScale chartScale, bool useCursorSens) {
			Rect rect = GetAnchorsRect(chartControl, chartScale);
			Point[] rectPoints = { new Point(rect.Left, rect.Bottom + (rect.Top-rect.Bottom)/2), new Point(rect.Right, rect.Bottom + (rect.Top-rect.Bottom)/2)  };
			
			Point? closest = GetClosestPoint(rectPoints, pt, useCursorSens);
			if (closest != null) {
				return ResizeMode.MoveAll;
			}
			rectPoints = new[]{ rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft };
			// check if mouse is along edges of rect, and do move if so
			for (int i = 0; i < 4; ++i) {
				Point startPoint = rectPoints[i == 3 ? 0 : i + 1]; // if we're on last point, check to first
				Vector vec = rectPoints[i] - startPoint;
				if (MathHelper.IsPointAlongVector(pt, startPoint, vec, CURSOR_SENSITIVITY))
					return ResizeMode.MoveAll;
			}
			return ResizeMode.None;
		}
		
		public override bool IsVisibleOnChart(ChartControl chartControl, ChartScale chartScale, DateTime firstTimeOnChart, DateTime lastTimeOnChart) {
			if (DrawingState == DrawingState.Building)
				return true;

			float minX = float.MaxValue;
			float maxX = float.MinValue;
			ChartPanel chartPanel = chartControl.ChartPanels[PanelIndex];
			// create an axis aligned bounding box taking into acct all 3 points for triangle to use for visibility checks
			foreach (Point pt in Anchors.Select(a => a.GetPoint(chartControl, chartPanel, chartScale))) {
				minX = (float)Math.Min(minX, pt.X);
				maxX = (float)Math.Max(maxX, pt.X);
			}

			DateTime	leftWidthTime	= chartControl.GetTimeByX((int) minX);
			DateTime	rightWidthTime	= chartControl.GetTimeByX((int) maxX);
			
			// check our width is visible somewhere horizontally
			return leftWidthTime <= lastTimeOnChart && rightWidthTime >= firstTimeOnChart;
		}

		public override void OnMouseDown(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint) {
			switch (DrawingState) {
				case DrawingState.Building:
					dataPoint.CopyDataValues(StartAnchor);
					dataPoint.CopyDataValues(EndAnchor);
					StartAnchor.IsEditing = false;
					EndAnchor.IsEditing = false;
					DrawingState 	= DrawingState.Normal;
					IsSelected 		= false;
					CalculateBox(chartControl, chartScale);
					break;
				case DrawingState.Normal:
					Point point = dataPoint.GetPoint(chartControl, chartPanel, chartScale);
					Cursor clickedCursor = GetCursor(chartControl, chartPanel, chartScale, point);
					if (clickedCursor == Cursors.SizeAll || clickedCursor == Cursors.No) {
						DrawingState = DrawingState.Moving;	
					} else {
						// we need to emulate editing depending on where they clicked
						resizeMode = GetResizeModeForPoint(point, chartControl, chartScale, true);
						if (resizeMode != ResizeMode.None) {
							DrawingState = resizeMode == ResizeMode.MoveAll ? DrawingState.Moving : DrawingState.Editing;
						} else {
							Rect rect = GetAnchorsRect(chartControl, chartScale);
							if (!rect.IntersectsWith(new Rect(point.X, point.Y, 1, 1))) {
								// user missed completely, deselect
								IsSelected = false;
							}
							// otherwise they clicked in a rect, but not close to anything so dont do anything
						}
					}
					if (lastMouseMoveDataPoint == null) lastMouseMoveDataPoint = new ChartAnchor();
					dataPoint.CopyDataValues(lastMouseMoveDataPoint);
					break;
			}
		}

		public override void OnMouseUp(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint) {
			if (DrawingState == DrawingState.Building) return;
			lastMouseMoveDataPoint	= null;
			DrawingState			= DrawingState.Normal;
			resizeMode				= ResizeMode.None;
		}
		
		private void CalculateBox(ChartControl chartControl, ChartScale chartScale) {
			int startIndex = chartScale.GetFirstChartBars().GetBarIdxByTime(chartControl, StartAnchor.Time);
			int endIndex = chartScale.GetFirstChartBars().GetBarIdxByTime(chartControl, EndAnchor.Time);
			if (startIndex > endIndex) {
				int temp = startIndex;
				startIndex = endIndex;
				endIndex = temp;
			}
			double? high = null;
			double? low = null;

			for (int i = startIndex; i <= endIndex; i++) {
				if (i < 0 || i >= chartScale.GetFirstChartBars().Bars.Count) continue;
				double currentHigh = chartScale.GetFirstChartBars().Bars.GetHigh(i);
				double currentLow = chartScale.GetFirstChartBars().Bars.GetLow(i);
				if (high==null || high < currentHigh) high = currentHigh;
				if (low==null || low > currentLow) low = currentLow;
			}
			
			StartAnchor.Price = low ?? 0;
			EndAnchor.Price = high ?? chartScale.GetFirstChartBars().Bars.LastPrice;
		}
		
		public override void OnMouseMove(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint) {
			if (IsLocked && DrawingState != DrawingState.Building)
				return;

			if (DrawingState == DrawingState.Building) {
				if (EndAnchor.IsEditing) {
					dataPoint.CopyDataValues(EndAnchor);
					CalculateBox(chartControl, chartScale);
				}
			} else if (DrawingState == DrawingState.Moving) {
				foreach (ChartAnchor anchor in Anchors)
					anchor.MoveAnchor(InitialMouseDownAnchor, dataPoint, chartControl, chartPanel, chartScale, this);
				CalculateBox(chartControl, chartScale);
			}
		}
		
		public override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			Stroke outlineStroke		= OutlineStroke;
			RenderTarget.AntialiasMode	= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			outlineStroke.RenderTarget	= RenderTarget;
			ChartPanel chartPanel		= chartControl.ChartPanels[PanelIndex];
			Point startPoint			= StartAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point endPoint				= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);

			double width				= endPoint.X - startPoint.X;
			double height				= endPoint.Y - startPoint.Y;
			
			// dont bother with an area brush if we're doing a hit test (software) render pass. we do not render area then.
			// this allows us to select something behind our area brush (like NT7)
			if (!IsInHitTest && AreaBrush != null) {
				if (areaBrushDevice.Brush == null) {
					Brush brushCopy			= areaBrush.Clone();
					brushCopy.Opacity		= areaOpacity / 100d; 
					areaBrushDevice.Brush	= brushCopy;
				}
				areaBrushDevice.RenderTarget = RenderTarget;
			} else {
				areaBrushDevice.RenderTarget = null;
				areaBrushDevice.Brush = null;
			}
			
			// align to full pixel to avoid unneeded aliasing
			double strokePixAdjust =	outlineStroke.Width % 2 == 0 ? 0.5d : 0d;

			double offest = (startPoint.X < endPoint.X) ? chartControl.BarWidth : -chartControl.BarWidth;
			SharpDX.RectangleF rect = new SharpDX.RectangleF(
				(float) (startPoint.X + strokePixAdjust - offest),
				(float) (startPoint.Y + strokePixAdjust),
				(float) (width + offest*2),
				(float) (height)
			);

			if (!IsInHitTest && areaBrushDevice.BrushDX != null)
				RenderTarget.FillRectangle(rect, areaBrushDevice.BrushDX);

			SharpDX.Direct2D1.Brush tmpBrush = IsInHitTest ? chartControl.SelectionBrush : outlineStroke.BrushDX;
			RenderTarget.DrawRectangle(rect, tmpBrush, outlineStroke.Width, outlineStroke.StrokeStyle);
			
			DoCustomRenderWork(chartScale, chartControl, startPoint);
		}

		protected abstract void DoCustomRenderWork(ChartScale chartScale, ChartControl chartControl, Point startPoint);
		
		protected override void OnStateChange() {
			if (State == State.SetDefaults) {
				StartAnchor		= new ChartAnchor { DisplayName = Custom.Resource.NinjaScriptDrawingToolAnchorStart,	IsEditing = true, DrawingTool = this };
				EndAnchor		= new ChartAnchor { DisplayName = Custom.Resource.NinjaScriptDrawingToolAnchorEnd,		IsEditing = true, DrawingTool = this };
				DrawingState	= DrawingState.Building;
				AreaBrush		= Brushes.CornflowerBlue;
				AreaOpacity		= 15;
				OutlineStroke	= new Stroke(Brushes.CornflowerBlue, DashStyleHelper.Solid, 2f, 50);
			} else if (State == State.Terminated) {
				Dispose();
			}
		}
	}

	/*
	 Example if a strategy must draw this drawing tool:
	 
	 public sealed class RectangleMarkableBar : MarkableBar {
		public override object Icon {
			get { return new Image {
					Source = new BitmapImage(new Uri(SuriAddOn.path + "cot1.png", UriKind.Absolute)),
					Width = 16,
					Height = 16,
				};
			}
		}
		protected override void OnStateChange() {
			base.OnStateChange();
			if (State == State.SetDefaults) {
				Name		= "Suri COT1 Box";
			}
		}
	}

	public static partial class MarkableBarDraw {
		private static T ShapeCore<T>(NinjaScriptBase owner, bool isAutoScale, string tag, int startBarsAgo, int endBarsAgo, 
			DateTime startTime, DateTime endTime, double startY, double endY, Brush brush, Brush areaBrush, int areaOpacity, bool isGlobal, string templateName) 
			where T : MarkableBar {
			if (owner == null)
				throw new ArgumentException("owner");
			if (string.IsNullOrWhiteSpace(tag))
				throw new ArgumentException("tag cant be null or empty");
			if (isGlobal && tag[0] != GlobalDrawingToolManager.GlobalDrawingToolTagPrefix)
				tag = GlobalDrawingToolManager.GlobalDrawingToolTagPrefix + tag;

			T shapeT = DrawingTool.GetByTagOrNew(owner, typeof(T), tag, templateName) as T;

			if (shapeT == null)
				return null;
			if (startTime < Core.Globals.MinDate)
				throw new ArgumentException(shapeT + " startTime must be greater than the minimum Date but was " + startTime);
			else if (endTime < Core.Globals.MinDate)
				throw new ArgumentException(shapeT + " endTime must be greater than the minimum Date but was " + endTime);			

			DrawingTool.SetDrawingToolCommonValues(shapeT, tag, isAutoScale, owner, isGlobal);

			// dont overwrite existing anchor references
			ChartAnchor	startAnchor	= DrawingTool.CreateChartAnchor(owner, startBarsAgo, startTime, startY);
			ChartAnchor	endAnchor	= DrawingTool.CreateChartAnchor(owner, endBarsAgo, endTime, endY);

			startAnchor.CopyDataValues(shapeT.StartAnchor);
			endAnchor.CopyDataValues(shapeT.EndAnchor);

			// these can be null when using a templateName so mind not overwriting them
			if (brush != null)
				shapeT.OutlineStroke	= new Stroke(brush, DashStyleHelper.Solid, 2f) { RenderTarget = shapeT.OutlineStroke.RenderTarget };
			if (areaOpacity >= 0)
				shapeT.AreaOpacity		= areaOpacity;
			if (areaBrush != null) {
				shapeT.AreaBrush		= areaBrush.Clone();
				if (shapeT.AreaBrush.CanFreeze)
					shapeT.AreaBrush.Freeze();
			}

			shapeT.SetState(State.Active);

			return shapeT;
		}

		public static RectangleMarkableBar RectangleMarkableBar(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY, Brush brush, bool drawText) {
			RectangleMarkableBar s = ShapeCore<RectangleMarkableBar>(owner, false, tag, startBarsAgo, endBarsAgo, Core.Globals.MinDate, Core.Globals.MinDate, startY, endY,
				brush, Brushes.CornflowerBlue, 15, false, null);
			s.OutlineStroke = new Stroke(Brushes.CornflowerBlue, DashStyleHelper.Solid, 2f, 50);
			return s;
		}

	}
	*/
	
}
