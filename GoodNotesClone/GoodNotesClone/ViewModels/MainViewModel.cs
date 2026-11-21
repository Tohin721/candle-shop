using static GoodNotesClone.ColorUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GoodNotesClone.Models;

namespace GoodNotesClone.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private Notebook _currentNotebook;
        private Page _currentPage;
        private DrawingTool _currentTool;
        private double _zoomLevel = 1.0;
        private bool _isDrawing;
        private Point _lastPoint;
        private Stroke _currentStroke;
        public Stroke CurrentStroke => _currentStroke;
        private List<Stroke> _selectedStrokes = new List<Stroke>();
        private UndoManager _undoManager;
        
        public ObservableCollection<Notebook> Notebooks { get; } = new ObservableCollection<Notebook>();
        public ObservableCollection<Page> Pages { get; } = new ObservableCollection<Page>();
        
        public Notebook CurrentNotebook
        {
            get => _currentNotebook;
            set => SetField(ref _currentNotebook, value, nameof(CurrentNotebook));
        }
        
        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetField(ref _currentPage, value, nameof(CurrentPage)))
                {
                    OnPropertyChanged(nameof(PageName));
                    OnPropertyChanged(nameof(PageCount));
                }
            }
        }
        
        public DrawingTool CurrentTool
        {
            get => _currentTool;
            set => SetField(ref _currentTool, value, nameof(CurrentTool));
        }
        
        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetField(ref _zoomLevel, value.Clamp(0.1, 4.0), nameof(ZoomLevel));
        }
        
        public string PageName => CurrentPage != null ? CurrentPage.Name : "No Page";
        public string PageCount => CurrentNotebook != null ? $"{CurrentNotebook.Pages.Count}" : "0";
        public int CurrentPageNumber => CurrentNotebook != null ? CurrentNotebook.CurrentPageIndex + 1 : 0;
        
        public ICommand NewNotebookCommand { get; }
        public ICommand OpenNotebookCommand { get; }
        public ICommand SaveNotebookCommand { get; }
        public ICommand SaveAsNotebookCommand { get; }
        public ICommand AddPageCommand { get; }
        public ICommand RemovePageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetZoomCommand { get; }
        public ICommand SelectPenCommand { get; }
        public ICommand SelectHighlighterCommand { get; }
        public ICommand SelectEraserCommand { get; }
        public ICommand SelectLassoCommand { get; }
        public ICommand SelectHandCommand { get; }
        public ICommand ClearPageCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand SetPenColorCommand { get; }
        public ICommand SetPenWidthCommand { get; }
        
        public MainViewModel()
        {
            _currentTool = new DrawingTool(ToolType.Pen, Colors.Black, 2.0);
            _undoManager = new UndoManager(100);
            
            // Create a default notebook
            CreateNewNotebook();
            
            // Initialize commands
            NewNotebookCommand = new RelayCommand(() => CreateNewNotebook());
            OpenNotebookCommand = new RelayCommand(() => OpenNotebook());
            SaveNotebookCommand = new RelayCommand(() => SaveNotebook(), () => CurrentNotebook != null);
            SaveAsNotebookCommand = new RelayCommand(() => SaveNotebookAs());
            AddPageCommand = new RelayCommand(() => AddPage(PageTemplate.Blank));
            RemovePageCommand = new RelayCommand(() => RemoveCurrentPage(), () => CurrentNotebook != null && CurrentNotebook.Pages.Count > 1);
            NextPageCommand = new RelayCommand(() => NextPage(), () => CurrentNotebook != null && CurrentNotebook.CurrentPageIndex < CurrentNotebook.Pages.Count - 1);
            PreviousPageCommand = new RelayCommand(() => PreviousPage(), () => CurrentNotebook != null && CurrentNotebook.CurrentPageIndex > 0);
            UndoCommand = new RelayCommand(() => _undoManager.Undo(), () => _undoManager.CanUndo);
            RedoCommand = new RelayCommand(() => _undoManager.Redo(), () => _undoManager.CanRedo);
            ZoomInCommand = new RelayCommand(() => ZoomLevel += 0.1);
            ZoomOutCommand = new RelayCommand(() => ZoomLevel -= 0.1);
            ResetZoomCommand = new RelayCommand(() => ZoomLevel = 1.0);
            SelectPenCommand = new RelayCommand(() => CurrentTool.SetPen(CurrentTool.Color, CurrentTool.Width, CurrentTool.PenType));
            SelectHighlighterCommand = new RelayCommand(() => CurrentTool.SetHighlighter(Colors.Yellow, 8.0));
            SelectEraserCommand = new RelayCommand(() => CurrentTool.SetEraser(10.0));
            SelectLassoCommand = new RelayCommand(() => CurrentTool.SetLasso());
            SelectHandCommand = new RelayCommand(() => CurrentTool.SetHand());
            ClearPageCommand = new RelayCommand(() => ClearCurrentPage(), () => CurrentPage != null);
            DeleteSelectedCommand = new RelayCommand(() => DeleteSelectedStrokes(), () => _selectedStrokes.Count > 0);
            SelectAllCommand = new RelayCommand(() => SelectAllStrokes(), () => CurrentPage != null);
            DeselectAllCommand = new RelayCommand(() => DeselectAllStrokes());
            CopyCommand = new RelayCommand(() => CopySelected(), () => _selectedStrokes.Count > 0);
            PasteCommand = new RelayCommand(() => PasteStrokes(), () => Clipboard.ContainsData(DataFormats.Text));
            CutCommand = new RelayCommand(() => CutSelected(), () => _selectedStrokes.Count > 0);
            SetPenColorCommand = new RelayCommand<string>(colorHex => SetPenColor(ColorUtils.FromHex(colorHex)));
            SetPenWidthCommand = new RelayCommand<double>(width => SetPenWidth(width));
            
            // Update command states
            _undoManager.CanUndoChanged += (s, e) => { (UndoCommand as RelayCommand)?.OnCanExecuteChanged(); };
            _undoManager.CanRedoChanged += (s, e) => { (RedoCommand as RelayCommand)?.OnCanExecuteChanged(); };
        }
        
        // Notebook operations
        public void CreateNewNotebook()
        {
            var notebook = new Notebook("Untitled Notebook");
            notebook.AddPage(PageTemplate.Blank);
            Notebooks.Add(notebook);
            CurrentNotebook = notebook;
            CurrentPage = notebook.CurrentPage;
            UpdatePagesCollection();
        }
        
        public void OpenNotebook()
        {
            string filePath = FileDialogUtils.ShowOpenDialog("GoodNotes Clone Files (*.gnc)|*.gnc|All Files (*.*)|*.*", "Open Notebook");
            if (string.IsNullOrEmpty(filePath)) return;
            
            try
            {
                var notebook = JsonHelper.Load<Notebook>(filePath);
                if (notebook != null)
                {
                    notebook.FilePath = filePath;
                    Notebooks.Add(notebook);
                    CurrentNotebook = notebook;
                    CurrentPage = notebook.CurrentPage;
                    UpdatePagesCollection();
                }
            }
            catch (Exception ex)
            {
                DialogUtils.ShowMessage($"Error opening notebook: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public void SaveNotebook()
        {
            if (CurrentNotebook == null) return;
            
            if (string.IsNullOrEmpty(CurrentNotebook.FilePath))
            {
                SaveNotebookAs();
                return;
            }
            
            SaveNotebookInternal(CurrentNotebook.FilePath);
        }
        
        public void SaveNotebookAs()
        {
            if (CurrentNotebook == null) return;
            
            string filePath = FileDialogUtils.ShowSaveDialog("GoodNotes Clone Files (*.gnc)|*.gnc", "Save Notebook As", CurrentNotebook.Name);
            if (string.IsNullOrEmpty(filePath)) return;
            
            CurrentNotebook.FilePath = filePath;
            CurrentNotebook.Name = Path.GetFileNameWithoutExtension(filePath);
            SaveNotebookInternal(filePath);
        }
        
        private void SaveNotebookInternal(string filePath)
        {
            try
            {
                CurrentNotebook.ModifiedDate = DateTime.Now;
                JsonHelper.Save(filePath, CurrentNotebook);
                DialogUtils.ShowMessage("Notebook saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DialogUtils.ShowMessage($"Error saving notebook: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void UpdatePagesCollection()
        {
            Pages.Clear();
            if (CurrentNotebook != null)
            {
                foreach (var page in CurrentNotebook.Pages)
                    Pages.Add(page);
            }
            OnPropertyChanged(nameof(PageCount));
        }
        
        // Page operations
        public void AddPage(PageTemplate template)
        {
            if (CurrentNotebook == null) return;
            
            var oldPageIndex = CurrentNotebook.CurrentPageIndex;
            CurrentNotebook.AddPage(template);
            CurrentNotebook.CurrentPageIndex = CurrentNotebook.Pages.Count - 1;
            CurrentPage = CurrentNotebook.CurrentPage;
            UpdatePagesCollection();
            
            // Record undo action
            _undoManager.AddAction(() =>
            {
                CurrentNotebook.RemovePageAt(CurrentNotebook.Pages.Count - 1);
                CurrentNotebook.CurrentPageIndex = oldPageIndex;
                CurrentPage = CurrentNotebook.CurrentPage;
                UpdatePagesCollection();
            });
        }
        
        public void RemoveCurrentPage()
        {
            if (CurrentNotebook == null || CurrentNotebook.Pages.Count <= 1) return;
            
            var page = CurrentPage;
            var pageIndex = CurrentNotebook.CurrentPageIndex;
            
            CurrentNotebook.RemovePageAt(pageIndex);
            CurrentPage = CurrentNotebook.CurrentPage;
            UpdatePagesCollection();
            
            // Record undo action
            _undoManager.AddAction(() =>
            {
                CurrentNotebook.Pages.Insert(pageIndex, page);
                CurrentNotebook.UpdatePageIndices();
                CurrentNotebook.CurrentPageIndex = pageIndex;
                CurrentPage = CurrentNotebook.CurrentPage;
                UpdatePagesCollection();
            });
        }
        
        public void NextPage()
        {
            if (CurrentNotebook == null) return;
            CurrentNotebook.GoToNextPage();
            CurrentPage = CurrentNotebook.CurrentPage;
        }
        
        public void PreviousPage()
        {
            if (CurrentNotebook == null) return;
            CurrentNotebook.GoToPreviousPage();
            CurrentPage = CurrentNotebook.CurrentPage;
        }
        
        public void ClearCurrentPage()
        {
            if (CurrentPage == null) return;
            
            var strokes = CurrentPage.Strokes.ToList();
            CurrentPage.Clear();
            
            // Record undo action
            _undoManager.AddAction(() =>
            {
                foreach (var stroke in strokes)
                    CurrentPage.AddStroke(stroke);
            });
        }
        
        // Drawing operations
        public void StartDrawing(Point point)
        {
            if (CurrentPage == null || CurrentTool.Type == ToolType.Hand || CurrentTool.Type == ToolType.Lasso) return;
            
            _isDrawing = true;
            _lastPoint = point;
            
            if (CurrentTool.Type == ToolType.Eraser)
            {
                // Eraser mode: select and delete strokes under the point
                var strokesToDelete = CurrentPage.Strokes.GetStrokesInRect(new Rect(point.X - 5, point.Y - 5, 10, 10));
                if (strokesToDelete.Count > 0)
                {
                    foreach (var stroke in strokesToDelete)
                        CurrentPage.RemoveStroke(stroke);
                }
            }
            else
            {
                // Create a new stroke
                _currentStroke = CurrentTool.CreateStroke();
                _currentStroke.AddPoint(point);
            }
        }
        
        public void ContinueDrawing(Point point)
        {
            if (!_isDrawing || CurrentPage == null || _currentStroke == null) return;
            
            if (CurrentTool.Type == ToolType.Eraser)
            {
                // Eraser: continue deleting
                var lineRect = new Rect(
                    Math.Min(_lastPoint.X, point.X) - 5,
                    Math.Min(_lastPoint.Y, point.Y) - 5,
                    Math.Abs(_lastPoint.X - point.X) + 10,
                    Math.Abs(_lastPoint.Y - point.Y) + 10);
                
                var strokesToDelete = CurrentPage.Strokes.GetStrokesInRect(lineRect);
                foreach (var stroke in strokesToDelete)
                    CurrentPage.RemoveStroke(stroke);
            }
            else
            {
                // Add point to current stroke
                _currentStroke.AddPoint(point);
            }
            
            _lastPoint = point;
        }
        
        public void EndDrawing(Point point)
        {
            if (!_isDrawing || CurrentPage == null || _currentStroke == null) return;
            
            if (CurrentTool.Type == ToolType.Eraser)
            {
                // Final eraser stroke
                var lineRect = new Rect(
                    Math.Min(_lastPoint.X, point.X) - 5,
                    Math.Min(_lastPoint.Y, point.Y) - 5,
                    Math.Abs(_lastPoint.X - point.X) + 10,
                    Math.Abs(_lastPoint.Y - point.Y) + 10);
                
                var strokesToDelete = CurrentPage.Strokes.GetStrokesInRect(lineRect);
                if (strokesToDelete.Count > 0)
                {
                    // Record undo action for eraser
                    _undoManager.AddAction(() =>
                    {
                        foreach (var stroke in strokesToDelete)
                            CurrentPage.AddStroke(stroke);
                    });
                }
            }
            else if (_currentStroke.Points.Count > 1)
            {
                // Add stroke to page
                CurrentPage.AddStroke(_currentStroke);
                
                // Record undo action
                _undoManager.AddAction(() =>
                {
                    CurrentPage.RemoveStroke(_currentStroke);
                });
            }
            
            _isDrawing = false;
            _currentStroke = null;
        }
        
        // Stroke selection
        public void SelectStroke(Stroke stroke, bool multiSelect = false)
        {
            if (CurrentPage == null) return;
            
            if (!multiSelect)
                DeselectAllStrokes();
            
            stroke.IsSelected = true;
            if (!_selectedStrokes.Contains(stroke))
                _selectedStrokes.Add(stroke);
            
            OnPropertyChanged(nameof(HasSelectedStrokes));
        }
        
        public void SelectStrokesInRect(Rect rect)
        {
            if (CurrentPage == null) return;
            
            DeselectAllStrokes();
            
            var strokes = CurrentPage.Strokes.GetStrokesInRect(rect);
            foreach (var stroke in strokes)
            {
                stroke.IsSelected = true;
                _selectedStrokes.Add(stroke);
            }
            
            OnPropertyChanged(nameof(HasSelectedStrokes));
        }
        
        public void SelectAllStrokes()
        {
            if (CurrentPage == null) return;
            
            DeselectAllStrokes();
            foreach (var stroke in CurrentPage.Strokes)
            {
                stroke.IsSelected = true;
                _selectedStrokes.Add(stroke);
            }
            
            OnPropertyChanged(nameof(HasSelectedStrokes));
        }
        
        public void DeselectAllStrokes()
        {
            foreach (var stroke in _selectedStrokes)
                stroke.IsSelected = false;
            _selectedStrokes.Clear();
            OnPropertyChanged(nameof(HasSelectedStrokes));
        }
        
        public void DeleteSelectedStrokes()
        {
            if (_selectedStrokes.Count == 0) return;
            
            var strokesToDelete = _selectedStrokes.ToList();
            
            // Record undo action
            _undoManager.AddAction(() =>
            {
                foreach (var stroke in strokesToDelete)
                {
                    CurrentPage.AddStroke(stroke);
                    stroke.IsSelected = true;
                }
                _selectedStrokes = strokesToDelete;
                OnPropertyChanged(nameof(HasSelectedStrokes));
            });
            
            foreach (var stroke in strokesToDelete)
                CurrentPage.RemoveStroke(stroke);
            
            _selectedStrokes.Clear();
            OnPropertyChanged(nameof(HasSelectedStrokes));
        }
        
        public bool HasSelectedStrokes => _selectedStrokes.Count > 0;
        
        // Clipboard operations
        public void CopySelected()
        {
            if (_selectedStrokes.Count == 0) return;
            
            var strokesJson = JsonHelper.Serialize(_selectedStrokes);
            Clipboard.SetText(strokesJson);
        }
        
        public void PasteStrokes()
        {
            if (!Clipboard.ContainsText()) return;
            
            try
            {
                var strokesJson = Clipboard.GetText();
                var strokes = JsonHelper.Deserialize<List<Stroke>>(strokesJson);
                if (strokes != null && CurrentPage != null)
                {
                    // Offset pasted strokes
                    double offsetX = 20, offsetY = 20;
                    foreach (var stroke in strokes)
                    {
                        var cloned = stroke.Clone();
                        cloned.Translate(offsetX, offsetY);
                        CurrentPage.AddStroke(cloned);
                        cloned.IsSelected = true;
                        _selectedStrokes.Add(cloned);
                    }
                    
                    // Record undo action
                    _undoManager.AddAction(() =>
                    {
                        foreach (var stroke in _selectedStrokes.ToList())
                            CurrentPage.RemoveStroke(stroke);
                        _selectedStrokes.Clear();
                        OnPropertyChanged(nameof(HasSelectedStrokes));
                    });
                    
                    OnPropertyChanged(nameof(HasSelectedStrokes));
                }
            }
            catch (Exception ex)
            {
                DialogUtils.ShowMessage($"Error pasting: {ex.Message}", "Error");
            }
        }
        
        public void CutSelected()
        {
            CopySelected();
            DeleteSelectedStrokes();
        }
        
        // Tool operations
        public void SetPenColor(Color color)
        {
            CurrentTool.Color = color;
            if (CurrentTool.Type == ToolType.Pen || CurrentTool.Type == ToolType.Highlighter)
                CurrentTool.IsHighlighter = CurrentTool.Type == ToolType.Highlighter;
        }
        
        public void SetPenWidth(double width)
        {
            CurrentTool.Width = width.Clamp(Constants.MinStrokeWidth, Constants.MaxStrokeWidth);
        }
        
        public void SetTool(ToolType tool)
        {
            switch (tool)
            {
                case ToolType.Pen:
                    CurrentTool.SetPen(CurrentTool.Color, CurrentTool.Width, CurrentTool.PenType);
                    break;
                case ToolType.Highlighter:
                    CurrentTool.SetHighlighter(Colors.Yellow, 8.0);
                    break;
                case ToolType.Eraser:
                    CurrentTool.SetEraser(10.0);
                    break;
                case ToolType.Lasso:
                    CurrentTool.SetLasso();
                    break;
                case ToolType.Hand:
                    CurrentTool.SetHand();
                    break;
                case ToolType.Text:
                    CurrentTool.SetText();
                    break;
            }
        }
    }
}
