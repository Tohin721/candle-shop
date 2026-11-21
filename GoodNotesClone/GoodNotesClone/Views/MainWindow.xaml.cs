using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GoodNotesClone.Models;
using GoodNotesClone.ViewModels;

namespace GoodNotesClone.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private Point _startPoint;
        private bool _isDragging;
        private Path _currentPath;
        private Path _lassoPath;
        private List<Point> _lassoPoints = new List<Point>();
        private List<UIElement> _strokeElements = new List<UIElement>();
        
        public MainWindow()
        {
            InitializeComponent();
            
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            
            // Subscribe to view model events
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Initialize canvas
            InitializeCanvas();
            
            // Set up keyboard shortcuts
            SetupKeyboardShortcuts();
            
            // Set window properties
            Title = "GoodNotes Clone";
        }
        
        private void InitializeCanvas()
        {
            DrawingCanvas.Width = 800;
            DrawingCanvas.Height = 1131;
            CanvasClip.Rect = new Rect(0, 0, 800, 1131);
            
            // Draw initial page
            DrawPage();
        }
        
        private void SetupKeyboardShortcuts()
        {
            // File shortcuts
            InputBindings.Add(new KeyBinding(_viewModel.NewNotebookCommand, Key.N, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.OpenNotebookCommand, Key.O, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SaveNotebookCommand, Key.S, ModifierKeys.Control));
            
            // Edit shortcuts
            InputBindings.Add(new KeyBinding(_viewModel.UndoCommand, Key.Z, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.RedoCommand, Key.Y, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SelectAllCommand, Key.A, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.DeleteSelectedCommand, Key.Delete));
            InputBindings.Add(new KeyBinding(_viewModel.CopyCommand, Key.C, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.PasteCommand, Key.V, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.CutCommand, Key.X, ModifierKeys.Control));
            
            // Navigation shortcuts
            InputBindings.Add(new KeyBinding(_viewModel.NextPageCommand, Key.Right, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.PreviousPageCommand, Key.Left, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.AddPageCommand, Key.N, ModifierKeys.Control | ModifierKeys.Shift));
            
            // Zoom shortcuts
            InputBindings.Add(new KeyBinding(_viewModel.ZoomInCommand, Key.Add, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.ZoomOutCommand, Key.Subtract, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.ResetZoomCommand, Key.D0, ModifierKeys.Control));
            
            // Tool shortcuts
            InputBindings.Add(new KeyBinding(_viewModel.SelectPenCommand, Key.P, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SelectHighlighterCommand, Key.H, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SelectEraserCommand, Key.E, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SelectLassoCommand, Key.L, ModifierKeys.Control));
        }
        
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.CurrentPage))
            {
                DrawPage();
            }
            else if (e.PropertyName == nameof(_viewModel.ZoomLevel))
            {
                CanvasScaleTransform.ScaleX = _viewModel.ZoomLevel;
                CanvasScaleTransform.ScaleY = _viewModel.ZoomLevel;
            }
        }
        
        private void DrawPage()
        {
            // Clear existing strokes
            ClearCanvas();
            
            if (_viewModel.CurrentPage == null) return;
            
            // Draw each stroke
            foreach (var stroke in _viewModel.CurrentPage.Strokes)
            {
                DrawStroke(stroke);
            }
        }
        
        private void DrawStroke(Stroke stroke)
        {
            if (stroke.Points.Count < 2) return;
            
            var path = new Path
            {
                Data = stroke.GetGeometry(),
                Stroke = new SolidColorBrush(stroke.Color),
                StrokeThickness = stroke.Width,
                StrokeLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashCap = PenLineCap.Round,
                Tag = stroke
            };
            
            if (stroke.IsHighlighter)
            {
                path.Stroke.Opacity = 0.5;
            }
            
            if (stroke.IsSelected)
            {
                // Add selection border
                var bounds = stroke.GetBounds();
                var selectionBorder = new Rectangle
                {
                    Width = bounds.Width + 4,
                    Height = bounds.Height + 4,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 3, 2 },
                    RadiusX = 2,
                    RadiusY = 2,
                    Tag = "selection"
                };
                Canvas.SetLeft(selectionBorder, bounds.X - 2);
                Canvas.SetTop(selectionBorder, bounds.Y - 2);
                DrawingCanvas.Children.Add(selectionBorder);
            }
            
            DrawingCanvas.Children.Add(path);
            _strokeElements.Add(path);
        }
        
        private void ClearCanvas()
        {
            // Remove only stroke elements and selection borders
            var toRemove = new List<UIElement>();
            foreach (var element in DrawingCanvas.Children)
            {
                if (element is Path || (element is FrameworkElement fe && fe.Tag as string == "selection"))
                {
                    toRemove.Add(element);
                }
            }
            foreach (var element in toRemove)
            {
                DrawingCanvas.Children.Remove(element);
            }
            _strokeElements.Clear();
            
            // Remove lasso if present
            if (_lassoPath != null)
            {
                DrawingCanvas.Children.Remove(_lassoPath);
                _lassoPath = null;
            }
            _lassoPoints.Clear();
        }
        
        // Canvas event handlers
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.CurrentPage == null) return;
            
            _startPoint = e.GetPosition(DrawingCanvas);
            _isDragging = true;
            
            // Transform point based on zoom
            var zoom = _viewModel.ZoomLevel;
            var point = new Point(_startPoint.X / zoom, _startPoint.Y / zoom);
            
            if (_viewModel.CurrentTool.Type == ToolType.Lasso)
            {
                // Start lasso selection
                _lassoPoints.Clear();
                _lassoPoints.Add(point);
                
                _lassoPath = new Path
                {
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 2, 2 },
                    Fill = Brushes.LightBlue,
                    Opacity = 0.5
                };
                DrawingCanvas.Children.Add(_lassoPath);
            }
            else
            {
                // Start drawing
                _viewModel.StartDrawing(point);
            }
            
            DrawingCanvas.CaptureMouse();
            e.Handled = true;
        }
        
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _viewModel.CurrentPage == null) return;
            
            var currentPoint = e.GetPosition(DrawingCanvas);
            var zoom = _viewModel.ZoomLevel;
            var point = new Point(currentPoint.X / zoom, currentPoint.Y / zoom);
            
            if (_viewModel.CurrentTool.Type == ToolType.Lasso)
            {
                // Update lasso path
                _lassoPoints.Add(point);
                UpdateLassoPath();
            }
            else
            {
                // Continue drawing
                _viewModel.ContinueDrawing(point);
                
                // Draw temporary stroke
                if (_currentPath != null)
                {
                    DrawingCanvas.Children.Remove(_currentPath);
                }
                
                if (_viewModel.CurrentTool.Type != ToolType.Eraser && _viewModel.CurrentStroke != null)
                {
                    _currentPath = new Path
                    {
                        Data = GetTempGeometry(_viewModel.CurrentStroke),
                        Stroke = new SolidColorBrush(_viewModel.CurrentTool.Color),
                        StrokeThickness = _viewModel.CurrentTool.Width,
                        StrokeLineCap = PenLineCap.Round,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round
                    };
                    DrawingCanvas.Children.Add(_currentPath);
                }
            }
            
            e.Handled = true;
        }
        
        private Geometry GetTempGeometry(Stroke stroke)
        {
            if (stroke == null || stroke.Points.Count < 2) return Geometry.Empty;
            
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = stroke.Points[0], IsClosed = false, IsFilled = false };
            var segments = new PathSegmentCollection();
            for (int i = 1; i < stroke.Points.Count; i++)
                segments.Add(new LineSegment(stroke.Points[i], true));
            figure.Segments = segments;
            geometry.Figures.Add(figure);
            return geometry;
        }
        
        private void UpdateLassoPath()
        {
            if (_lassoPath == null || _lassoPoints.Count < 2) return;
            
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = _lassoPoints[0], IsClosed = false, IsFilled = true };
            var segments = new PathSegmentCollection();
            for (int i = 1; i < _lassoPoints.Count; i++)
                segments.Add(new LineSegment(_lassoPoints[i], true));
            figure.Segments = segments;
            geometry.Figures.Add(figure);
            _lassoPath.Data = geometry;
        }
        
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging || _viewModel.CurrentPage == null) return;
            
            var endPoint = e.GetPosition(DrawingCanvas);
            var zoom = _viewModel.ZoomLevel;
            var point = new Point(endPoint.X / zoom, endPoint.Y / zoom);
            
            if (_viewModel.CurrentTool.Type == ToolType.Lasso)
            {
                // Complete lasso selection
                if (_lassoPoints.Count > 2)
                {
                    // Close the lasso
                    _lassoPoints.Add(_lassoPoints[0]);
                    UpdateLassoPath();
                    
                    // Select strokes in lasso
                    var strokes = LassoUtils.SelectStrokesInLasso(_viewModel.CurrentPage.Strokes, _lassoPoints);
                    foreach (var stroke in strokes)
                        stroke.IsSelected = true;
                    
                    _viewModel.DeselectAllStrokes();
                    foreach (var stroke in strokes)
                        _viewModel.SelectStroke(stroke, true);
                }
                
                // Remove lasso
                if (_lassoPath != null)
                {
                    DrawingCanvas.Children.Remove(_lassoPath);
                    _lassoPath = null;
                }
                _lassoPoints.Clear();
            }
            else
            {
                // End drawing
                _viewModel.EndDrawing(point);
                
                // Remove temporary path
                if (_currentPath != null)
                {
                    DrawingCanvas.Children.Remove(_currentPath);
                    _currentPath = null;
                }
                
                // Redraw page with new stroke
                DrawPage();
            }
            
            _isDragging = false;
            DrawingCanvas.ReleaseMouseCapture();
            e.Handled = true;
        }
        
        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Right-click to show context menu
            var point = e.GetPosition(DrawingCanvas);
            var zoom = _viewModel.ZoomLevel;
            var canvasPoint = new Point(point.X / zoom, point.Y / zoom);
            
            // Check if we clicked on a stroke
            var hitTestResult = VisualTreeHelper.HitTest(DrawingCanvas, point);
            if (hitTestResult != null && hitTestResult.VisualHit is Path path)
            {
                if (path.Tag is Stroke stroke)
                {
                    // Select the stroke
                    _viewModel.SelectStroke(stroke, Keyboard.Modifiers == ModifierKeys.Shift);
                }
            }
            else
            {
                // Deselect all
                _viewModel.DeselectAllStrokes();
            }
            
            e.Handled = true;
        }
        
        private void PageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Page selectedPage)
            {
                _viewModel.CurrentPage = selectedPage;
                _viewModel.CurrentNotebook.CurrentPageIndex = _viewModel.Pages.IndexOf(selectedPage);
            }
        }
        
        protected override void OnClosed(EventArgs e)
        {
            // Clean up resources
            ClearCanvas();
            base.OnClosed(e);
        }
    }
    
    // Converters for tool type to checked state
    public class ToolTypeToCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ToolType toolType && parameter is ToolType expectedType)
            {
                return toolType == expectedType;
            }
            return false;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
