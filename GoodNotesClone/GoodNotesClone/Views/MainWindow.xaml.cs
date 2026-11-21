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
        private Stroke _currentStroke;
        private Path _tempPath;
        private Path _lassoPath;
        private List<UIElement> _selectedElements = new List<UIElement>();
        private bool _isDragging = false;
        private Point _dragStart;
        
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            
            // Initialize the UI
            InitializeUI();
            
            // Set up keyboard shortcuts
            SetupKeyboardShortcuts();
            
            // Update UI when notebooks change
            _viewModel.Notebooks.CollectionChanged += (s, e) => UpdateNotebookList();
            _viewModel.CurrentNotebookChanged += (s, e) => UpdatePageList();
            _viewModel.CurrentPageChanged += (s, e) => DrawPage();
            _viewModel.StrokesChanged += (s, e) => DrawPage();
            _viewModel.ZoomChanged += (s, e) => UpdateCanvasTransform();
        }
        
        private void InitializeUI()
        {
            // Set initial tool
            PenButton.IsChecked = true;
            
            // Update notebook and page lists
            UpdateNotebookList();
            UpdatePageList();
            
            // Draw initial page
            DrawPage();
            
            // Set canvas size
            DrawingCanvas.Width = 800;
            DrawingCanvas.Height = 1100;
            
            // Set default page size
            if (_viewModel.CurrentPage != null)
            {
                _viewModel.CurrentPage.Width = 800;
                _viewModel.CurrentPage.Height = 1100;
            }
            
            // Show notebook grid or canvas based on mode
            UpdateViewMode();
        }
        
        private void SetupKeyboardShortcuts()
        {
            // Tool shortcuts
            InputBindings.Add(new KeyBinding(_viewModel.SetPenCommand, Key.P, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SetHighlighterCommand, Key.H, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SetEraserCommand, Key.E, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SetLassoCommand, Key.L, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SetHandCommand, Key.H, ModifierKeys.Control | ModifierKeys.Shift));
            
            // File shortcuts
            InputBindings.Add(new KeyBinding(_viewModel.NewNotebookCommand, Key.N, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.OpenNotebookCommand, Key.O, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SaveNotebookCommand, Key.S, ModifierKeys.Control));
            
            // Edit shortcuts
            InputBindings.Add(new KeyBinding(_viewModel.UndoCommand, Key.Z, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.RedoCommand, Key.Y, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.CopyCommand, Key.C, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.PasteCommand, Key.V, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.CutCommand, Key.X, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.SelectAllCommand, Key.A, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.DeleteSelectedCommand, Key.Delete));
            
            // Page navigation
            InputBindings.Add(new KeyBinding(_viewModel.PreviousPageCommand, Key.Left, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.NextPageCommand, Key.Right, ModifierKeys.Control));
            
            // Zoom
            InputBindings.Add(new KeyBinding(_viewModel.ZoomInCommand, Key.OemPlus, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.ZoomOutCommand, Key.OemMinus, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(_viewModel.ResetZoomCommand, Key.D0, ModifierKeys.Control));
        }
        
        private void UpdateViewMode()
        {
            // Check if we're in Documents mode or Notebook mode
            if (_viewModel.CurrentNotebook == null)
            {
                // Show notebook grid
                NotebookGridScroll.Visibility = Visibility.Visible;
                CanvasBorder.Visibility = Visibility.Collapsed;
                UpdateNotebookGrid();
            }
            else
            {
                // Show canvas
                NotebookGridScroll.Visibility = Visibility.Collapsed;
                CanvasBorder.Visibility = Visibility.Visible;
                DrawPage();
            }
        }
        
        private void UpdateNotebookList()
        {
            NotebookList.ItemsSource = _viewModel.Notebooks;
            if (_viewModel.Notebooks.Count > 0)
            {
                NotebookList.SelectedIndex = 0;
            }
        }
        
        private void UpdatePageList()
        {
            if (_viewModel.CurrentNotebook != null)
            {
                PageList.ItemsSource = _viewModel.CurrentNotebook.Pages;
                if (_viewModel.CurrentNotebook.Pages.Count > 0)
                {
                    PageList.SelectedIndex = _viewModel.CurrentPageIndex;
                }
            }
            else
            {
                PageList.ItemsSource = null;
            }
        }
        
        private void UpdateNotebookGrid()
        {
            NotebookGrid.Children.Clear();
            
            foreach (var notebook in _viewModel.Notebooks)
            {
                var button = new Button
                {
                    Style = (Style)Resources["NotebookItemStyle"],
                    Content = notebook.Name,
                    Tag = notebook,
                    ToolTip = notebook.Name + "\n" + notebook.Pages.Count + " pages"
                };
                button.Click += NotebookGridButton_Click;
                NotebookGrid.Children.Add(button);
            }
            
            // Add New Notebook button
            var newButton = new Button
            {
                Style = (Style)Resources["NotebookItemStyle"],
                Content = "+ New Notebook",
                Tag = null,
                ToolTip = "Create a new notebook"
            };
            newButton.Click += (s, e) => _viewModel.NewNotebookCommand.Execute(null);
            NotebookGrid.Children.Add(newButton);
        }
        
        private void NotebookGridButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Tag is Notebook notebook)
            {
                _viewModel.CurrentNotebook = notebook;
                UpdateViewMode();
            }
        }
        
        private void Navigation_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tag = button.Tag as string;
            
            // Clear selection in navigation list
            foreach (var item in NavigationList.Items)
            {
                if (item is Button btn)
                {
                    btn.IsSelected = false;
                }
            }
            button.IsSelected = true;
            
            // Handle navigation
            switch (tag)
            {
                case "Documents":
                    // Show all notebooks
                    _viewModel.CurrentNotebook = null;
                    UpdateViewMode();
                    break;
                case "Favorites":
                    // Show favorites (placeholder)
                    MessageBox.Show("Favorites view coming soon!");
                    break;
                case "Shared":
                    // Show shared notebooks (placeholder)
                    MessageBox.Show("Shared notebooks view coming soon!");
                    break;
                case "Marketplace":
                    // Show marketplace (placeholder)
                    MessageBox.Show("Marketplace coming soon!");
                    break;
                case "Trash":
                    // Show trash (placeholder)
                    MessageBox.Show("Trash view coming soon!");
                    break;
            }
        }
        
        private void NewNotebook_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.NewNotebookCommand.Execute(null);
            UpdateNotebookList();
            UpdateNotebookGrid();
        }
        
        private void NotebookList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotebookList.SelectedItem is Notebook selectedNotebook)
            {
                _viewModel.CurrentNotebook = selectedNotebook;
                UpdateViewMode();
            }
        }
        
        private void PageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageList.SelectedIndex >= 0)
            {
                _viewModel.CurrentPageIndex = PageList.SelectedIndex;
            }
        }
        
        private void ToolButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            var toolType = (ToolType)Enum.Parse(typeof(ToolType), button.Tag.ToString());
            _viewModel.CurrentTool = new DrawingTool(toolType);
            
            // Uncheck other tool buttons
            foreach (var child in ToolBar.Children)
            {
                if (child is ToggleButton tb && tb != button)
                {
                    tb.IsChecked = false;
                }
            }
        }
        
        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var colorHex = button.Tag.ToString();
            var color = (Color)ColorConverter.ConvertFromString(colorHex);
            _viewModel.CurrentTool.Color = color;
        }
        
        private void MoreColors_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                _viewModel.CurrentTool.Color = color;
            }
        }
        
        private void WidthButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var width = double.Parse(button.Tag.ToString());
            _viewModel.CurrentTool.Width = width;
        }
        
        private void FileMenu_Click(object sender, RoutedEventArgs e)
        {
            // Show file menu (placeholder)
            MessageBox.Show("File menu options:\nNew, Open, Save, Save As, Export, Print");
        }
        
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Search functionality coming soon!");
        }
        
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings coming soon!");
        }
        
        private void UpdateCanvasTransform()
        {
            var transform = new ScaleTransform(_viewModel.ZoomLevel, _viewModel.ZoomLevel);
            DrawingCanvas.RenderTransform = transform;
        }
        
        private void DrawPage()
        {
            DrawingCanvas.Children.Clear();
            
            if (_viewModel.CurrentPage == null) return;
            
            // Draw page background
            var pageBackground = new Rectangle
            {
                Width = _viewModel.CurrentPage.Width,
                Height = _viewModel.CurrentPage.Height,
                Fill = new SolidColorBrush(Colors.White),
                Stroke = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                StrokeThickness = 1
            };
            DrawingCanvas.Children.Add(pageBackground);
            
            // Draw page template (lines/grid)
            DrawPageTemplate();
            
            // Draw all strokes
            foreach (var stroke in _viewModel.CurrentPage.Strokes)
            {
                DrawStroke(stroke);
            }
            
            // Draw selected strokes (if any)
            foreach (var selectedStroke in _viewModel.SelectedStrokes)
            {
                HighlightStroke(selectedStroke);
            }
        }
        
        private void DrawPageTemplate()
        {
            if (_viewModel.CurrentPage == null) return;
            
            var template = _viewModel.CurrentPage.TemplateType;
            var width = _viewModel.CurrentPage.Width;
            var height = _viewModel.CurrentPage.Height;
            
            switch (template)
            {
                case PageTemplate.Lined:
                    // Draw horizontal lines
                    for (int y = 0; y < height; y += 30)
                    {
                        var line = new Line
                        {
                            X1 = 0,
                            Y1 = y,
                            X2 = width,
                            Y2 = y,
                            Stroke = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                            StrokeThickness = 1,
                            StrokeDashArray = new DoubleCollection { 5, 5 }
                        };
                        DrawingCanvas.Children.Add(line);
                    }
                    break;
                    
                case PageTemplate.Grid:
                    // Draw grid
                    for (int x = 0; x < width; x += 20)
                    {
                        var line = new Line
                        {
                            X1 = x,
                            Y1 = 0,
                            X2 = x,
                            Y2 = height,
                            Stroke = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                            StrokeThickness = 1,
                            StrokeDashArray = new DoubleCollection { 5, 5 }
                        };
                        DrawingCanvas.Children.Add(line);
                    }
                    for (int y = 0; y < height; y += 20)
                    {
                        var line = new Line
                        {
                            X1 = 0,
                            Y1 = y,
                            X2 = width,
                            Y2 = y,
                            Stroke = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                            StrokeThickness = 1,
                            StrokeDashArray = new DoubleCollection { 5, 5 }
                        };
                        DrawingCanvas.Children.Add(line);
                    }
                    break;
                    
                case PageTemplate.Dotted:
                    // Draw dotted grid
                    for (int x = 0; x < width; x += 20)
                    {
                        for (int y = 0; y < height; y += 20)
                        {
                            var ellipse = new Ellipse
                            {
                                Width = 4,
                                Height = 4,
                                Fill = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                                Margin = new Thickness(x - 2, y - 2, 0, 0)
                            };
                            DrawingCanvas.Children.Add(ellipse);
                        }
                    }
                    break;
            }
        }
        
        private void DrawStroke(Stroke stroke)
        {
            if (stroke.Points.Count < 2) return;
            
            var geometry = stroke.GetGeometry();
            var path = new Path
            {
                Data = geometry,
                Stroke = new SolidColorBrush(stroke.Color),
                StrokeThickness = stroke.Width,
                StrokeLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Tag = stroke
            };
            
            // Add pressure sensitivity if available
            if (stroke.Pressures != null && stroke.Pressures.Count > 0)
            {
                // For now, just use the stroke as-is
                // Pressure sensitivity would require more complex rendering
            }
            
            DrawingCanvas.Children.Add(path);
        }
        
        private void HighlightStroke(Stroke stroke)
        {
            // Find the path corresponding to this stroke
            foreach (var child in DrawingCanvas.Children)
            {
                if (child is Path path && path.Tag == stroke)
                {
                    // Add a highlight effect
                    var highlight = new Path
                    {
                        Data = stroke.GetGeometry(),
                        Stroke = new SolidColorBrush(Color.FromArgb(128, 0, 120, 215)),
                        StrokeThickness = stroke.Width + 4,
                        StrokeLineCap = PenLineCap.Round,
                        StrokeLineJoin = PenLineJoin.Round,
                        StrokeDashArray = new DoubleCollection { 5, 3 }
                    };
                    DrawingCanvas.Children.Add(highlight);
                    break;
                }
            }
        }
        
        private void ClearCanvas()
        {
            DrawingCanvas.Children.Clear();
        }
        
        // Canvas Mouse Events
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.CurrentPage == null) return;
            
            _startPoint = e.GetPosition(DrawingCanvas);
            
            if (_viewModel.CurrentTool.Type == ToolType.Lasso)
            {
                // Start lasso selection
                _lassoPath = new Path
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(128, 0, 120, 215)),
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 5, 3 }
                };
                
                var geometry = new PathGeometry();
                var figure = new PathFigure
                {
                    StartPoint = _startPoint,
                    IsClosed = false
                };
                geometry.Figures.Add(figure);
                _lassoPath.Data = geometry;
                
                DrawingCanvas.Children.Add(_lassoPath);
                _isDragging = true;
            }
            else if (_viewModel.CurrentTool.Type == ToolType.Hand)
            {
                // Start panning
                _dragStart = _startPoint;
                _isDragging = true;
                DrawingCanvas.Cursor = Cursors.Hand;
            }
            else
            {
                // Start drawing
                _currentStroke = new Stroke
                {
                    Color = _viewModel.CurrentTool.Color,
                    Width = _viewModel.CurrentTool.Width,
                    PenType = _viewModel.CurrentTool.PenType
                };
                _currentStroke.Points.Add(_startPoint);
                
                // Create temporary path for smooth drawing
                _tempPath = new Path
                {
                    Stroke = new SolidColorBrush(_viewModel.CurrentTool.Color),
                    StrokeThickness = _viewModel.CurrentTool.Width,
                    StrokeLineCap = PenLineCap.Round,
                    StrokeLineJoin = PenLineJoin.Round
                };
                
                DrawingCanvas.Children.Add(_tempPath);
                _isDragging = true;
            }
            
            e.Handled = true;
        }
        
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            
            var currentPoint = e.GetPosition(DrawingCanvas);
            
            if (_viewModel.CurrentTool.Type == ToolType.Lasso && _lassoPath != null)
            {
                // Update lasso path
                var geometry = (PathGeometry)_lassoPath.Data;
                if (geometry.Figures.Count > 0)
                {
                    var figure = geometry.Figures[0];
                    figure.Segments.Add(new LineSegment(currentPoint, true));
                    _lassoPath.Data = geometry;
                }
            }
            else if (_viewModel.CurrentTool.Type == ToolType.Hand)
            {
                // Pan the canvas
                var delta = currentPoint - _dragStart;
                // Implement panning logic here
            }
            else if (_currentStroke != null && _tempPath != null)
            {
                // Continue drawing
                _currentStroke.Points.Add(currentPoint);
                
                // Update temporary path
                var geometry = Stroke.GetGeometryFromPoints(_currentStroke.Points);
                _tempPath.Data = geometry;
            }
            
            e.Handled = true;
        }
        
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging) return;
            
            var endPoint = e.GetPosition(DrawingCanvas);
            
            if (_viewModel.CurrentTool.Type == ToolType.Lasso && _lassoPath != null)
            {
                // Complete lasso selection
                var geometry = (PathGeometry)_lassoPath.Data;
                if (geometry.Figures.Count > 0)
                {
                    var figure = geometry.Figures[0];
                    figure.IsClosed = true;
                    _lassoPath.Data = geometry;
                }
                
                // Find strokes within lasso
                var selectedStrokes = new List<Stroke>();
                foreach (var stroke in _viewModel.CurrentPage.Strokes)
                {
                    // Simple bounding box check for now
                    // (Full lasso selection would require point-in-polygon checks)
                    if (stroke.Bounds.IntersectsWith(new Rect(_startPoint, endPoint)))
                    {
                        selectedStrokes.Add(stroke);
                    }
                }
                
                _viewModel.SelectedStrokes = selectedStrokes;
                
                // Remove lasso path
                DrawingCanvas.Children.Remove(_lassoPath);
                _lassoPath = null;
                
                // Redraw page to show selection
                DrawPage();
            }
            else if (_currentStroke != null && _tempPath != null)
            {
                // Complete drawing
                if (_currentStroke.Points.Count > 1)
                {
                    // Only add stroke if it has at least 2 points
                    _viewModel.CurrentPage.Strokes.Add(_currentStroke);
                    _viewModel.AddStrokeToHistory(_currentStroke);
                }
                
                // Remove temporary path
                DrawingCanvas.Children.Remove(_tempPath);
                _tempPath = null;
                _currentStroke = null;
                
                // Redraw page
                DrawPage();
            }
            
            _isDragging = false;
            DrawingCanvas.Cursor = Cursors.Arrow;
            e.Handled = true;
        }
        
        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Show context menu or select stroke
            var point = e.GetPosition(DrawingCanvas);
            
            // Find stroke at this position
            foreach (var child in DrawingCanvas.Children)
            {
                if (child is Path path && path.Tag is Stroke stroke)
                {
                    // Check if point is on stroke (simplified)
                    var bounds = stroke.Bounds;
                    if (bounds.Contains(point))
                    {
                        // Select this stroke
                        _viewModel.SelectedStrokes = new List<Stroke> { stroke };
                        DrawPage();
                        break;
                    }
                }
            }
            
            e.Handled = true;
        }
        
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Zoom with Ctrl+MouseWheel
                if (e.Delta > 0)
                {
                    _viewModel.ZoomInCommand.Execute(null);
                }
                else
                {
                    _viewModel.ZoomOutCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}
