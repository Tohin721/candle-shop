using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GoodNotesClone.Models;
using GoodNotesClone.Services;

namespace GoodNotesClone.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private Notebook _currentNotebook;
        private Page _currentPage;
        private DrawingTool _currentTool;
        private int _currentPageIndex;
        private double _zoomLevel = 1.0;
        private List<Stroke> _selectedStrokes = new List<Stroke>();
        private string _currentNotebookName;
        private string _searchQuery;
        
        public ObservableCollection<Notebook> Notebooks { get; } = new ObservableCollection<Notebook>();
        public UndoManager UndoManager { get; } = new UndoManager();
        
        public event EventHandler CurrentNotebookChanged;
        public event EventHandler CurrentPageChanged;
        public event EventHandler StrokesChanged;
        public event EventHandler ZoomChanged;
        
        public MainViewModel()
        {
            // Initialize with a default notebook
            NewNotebookCommand.Execute(null);
            
            // Set default tool
            CurrentTool = new DrawingTool(ToolType.Pen);
        }
        
        #region Properties
        
        public Notebook CurrentNotebook
        {
            get => _currentNotebook;
            set
            {
                if (_currentNotebook != value)
                {
                    _currentNotebook = value;
                    OnPropertyChanged();
                    CurrentNotebookChanged?.Invoke(this, EventArgs.Empty);
                    
                    if (value != null)
                    {
                        CurrentPageIndex = 0;
                    }
                }
            }
        }
        
        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    CurrentPageChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        
        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set
            {
                if (_currentPageIndex != value && CurrentNotebook != null && 
                    value >= 0 && value < CurrentNotebook.Pages.Count)
                {
                    _currentPageIndex = value;
                    CurrentPage = CurrentNotebook.Pages[value];
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentPage));
                }
            }
        }
        
        public DrawingTool CurrentTool
        {
            get => _currentTool;
            set
            {
                if (_currentTool != value)
                {
                    _currentTool = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentToolDisplay));
                }
            }
        }
        
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (_zoomLevel != value)
                {
                    _zoomLevel = Math.Clamp(value, 0.1, 3.0);
                    OnPropertyChanged();
                    ZoomChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        
        public List<Stroke> SelectedStrokes
        {
            get => _selectedStrokes;
            set
            {
                _selectedStrokes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
            }
        }
        
        public bool HasSelection => SelectedStrokes != null && SelectedStrokes.Count > 0;
        
        public string CurrentToolDisplay
        {
            get
            {
                if (CurrentTool == null) return "None";
                switch (CurrentTool.Type)
                {
                    case ToolType.Pen: return "🖋️ Pen";
                    case ToolType.Highlighter: return "🖍️ Highlighter";
                    case ToolType.Eraser: return "🧽 Eraser";
                    case ToolType.Lasso: return "🎯 Lasso";
                    case ToolType.Hand: return "👋 Hand";
                    default: return CurrentTool.Type.ToString();
                }
            }
        }
        
        public string CurrentNotebookName
        {
            get => _currentNotebookName;
            set
            {
                _currentNotebookName = value;
                OnPropertyChanged();
            }
        }
        
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }
        
        #endregion
        
        #region Commands
        
        // File Commands
        public ICommand NewNotebookCommand => new RelayCommand(NewNotebook);
        public ICommand OpenNotebookCommand => new RelayCommand(OpenNotebook);
        public ICommand SaveNotebookCommand => new RelayCommand(SaveNotebook);
        public ICommand SaveAsNotebookCommand => new RelayCommand(SaveAsNotebook);
        
        // Page Commands
        public ICommand AddPageCommand => new RelayCommand(AddPage);
        public ICommand RemovePageCommand => new RelayCommand(RemovePage, CanRemovePage);
        public ICommand NextPageCommand => new RelayCommand(NextPage, CanNextPage);
        public ICommand PreviousPageCommand => new RelayCommand(PreviousPage, CanPreviousPage);
        public ICommand GotoPageCommand => new RelayCommand<int>(GotoPage);
        
        // Tool Commands
        public ICommand SetPenCommand => new RelayCommand(() => CurrentTool = new DrawingTool(ToolType.Pen));
        public ICommand SetHighlighterCommand => new RelayCommand(() => CurrentTool = new DrawingTool(ToolType.Highlighter));
        public ICommand SetEraserCommand => new RelayCommand(() => CurrentTool = new DrawingTool(ToolType.Eraser));
        public ICommand SetLassoCommand => new RelayCommand(() => CurrentTool = new DrawingTool(ToolType.Lasso));
        public ICommand SetHandCommand => new RelayCommand(() => CurrentTool = new DrawingTool(ToolType.Hand));
        
        // Edit Commands
        public ICommand UndoCommand => new RelayCommand(Undo, CanUndo);
        public ICommand RedoCommand => new RelayCommand(Redo, CanRedo);
        public ICommand CopyCommand => new RelayCommand(Copy, CanCopy);
        public ICommand PasteCommand => new RelayCommand(Paste, CanPaste);
        public ICommand CutCommand => new RelayCommand(Cut, CanCut);
        public ICommand DeleteSelectedCommand => new RelayCommand(DeleteSelected, CanDelete);
        public ICommand SelectAllCommand => new RelayCommand(SelectAll);
        
        // Zoom Commands
        public ICommand ZoomInCommand => new RelayCommand(ZoomIn);
        public ICommand ZoomOutCommand => new RelayCommand(ZoomOut);
        public ICommand ResetZoomCommand => new RelayCommand(ResetZoom);
        
        // Color and Width
        public ICommand SetPenColorCommand => new RelayCommand<Color>(SetPenColor);
        public ICommand SetPenWidthCommand => new RelayCommand<double>(SetPenWidth);
        
        #endregion
        
        #region Command Implementations
        
        // File Operations
        private void NewNotebook()
        {
            var newNotebook = new Notebook
            {
                Name = "Untitled Notebook",
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now
            };
            
            // Add a default page
            newNotebook.AddPage(new Page
            {
                Name = "Page 1",
                TemplateType = PageTemplate.Blank,
                Width = 800,
                Height = 1100
            });
            
            Notebooks.Add(newNotebook);
            CurrentNotebook = newNotebook;
            CurrentPageIndex = 0;
        }
        
        private void OpenNotebook()
        {
            var filePath = FileDialogUtils.ShowOpenDialog(
                "GoodNotes Notebooks (*.gnc)|*.gnc|All Files (*.*)|*.*", 
                "Open Notebook");
            
            if (!string.IsNullOrEmpty(filePath))
            {
                var notebook = FileService.LoadNotebook(filePath);
                if (notebook != null)
                {
                    Notebooks.Add(notebook);
                    CurrentNotebook = notebook;
                    CurrentPageIndex = 0;
                }
            }
        }
        
        private void SaveNotebook()
        {
            if (CurrentNotebook == null) return;
            
            if (string.IsNullOrEmpty(CurrentNotebook.FilePath))
            {
                SaveAsNotebook();
            }
            else
            {
                FileService.SaveNotebook(CurrentNotebook);
                CurrentNotebook.LastModified = DateTime.Now;
            }
        }
        
        private void SaveAsNotebook()
        {
            if (CurrentNotebook == null) return;
            
            var filePath = FileDialogUtils.ShowSaveDialog(
                "GoodNotes Notebooks (*.gnc)|*.gnc|All Files (*.*)|*.*",
                "Save Notebook As",
                CurrentNotebook.Name + ".gnc");
            
            if (!string.IsNullOrEmpty(filePath))
            {
                CurrentNotebook.FilePath = filePath;
                CurrentNotebook.Name = System.IO.Path.GetFileNameWithoutExtension(filePath);
                FileService.SaveNotebook(CurrentNotebook);
                CurrentNotebook.LastModified = DateTime.Now;
            }
        }
        
        // Page Operations
        private void AddPage()
        {
            if (CurrentNotebook == null) return;
            
            var newPage = new Page
            {
                Name = "Page " + (CurrentNotebook.Pages.Count + 1),
                TemplateType = PageTemplate.Blank,
                Width = CurrentNotebook.Pages.Count > 0 ? CurrentNotebook.Pages[0].Width : 800,
                Height = CurrentNotebook.Pages.Count > 0 ? CurrentNotebook.Pages[0].Height : 1100
            };
            
            CurrentNotebook.AddPage(newPage);
            CurrentPageIndex = CurrentNotebook.Pages.Count - 1;
            
            // Add to undo stack
            UndoManager.Push(new UndoAction
            {
                Action = UndoActionType.AddPage,
                Notebook = CurrentNotebook,
                PageIndex = CurrentPageIndex
            });
        }
        
        private bool CanRemovePage() => CurrentNotebook != null && CurrentNotebook.Pages.Count > 1;
        
        private void RemovePage()
        {
            if (CurrentNotebook == null || CurrentNotebook.Pages.Count <= 1) return;
            
            var removedPage = CurrentNotebook.Pages[CurrentPageIndex];
            CurrentNotebook.RemovePage(CurrentPageIndex);
            
            // Adjust current page index
            if (CurrentPageIndex >= CurrentNotebook.Pages.Count)
            {
                CurrentPageIndex = CurrentNotebook.Pages.Count - 1;
            }
            
            // Add to undo stack
            UndoManager.Push(new UndoAction
            {
                Action = UndoActionType.RemovePage,
                Notebook = CurrentNotebook,
                Page = removedPage,
                PageIndex = CurrentPageIndex
            });
        }
        
        private bool CanNextPage() => CurrentNotebook != null && CurrentPageIndex < CurrentNotebook.Pages.Count - 1;
        
        private void NextPage()
        {
            if (CanNextPage())
            {
                CurrentPageIndex++;
            }
        }
        
        private bool CanPreviousPage() => CurrentNotebook != null && CurrentPageIndex > 0;
        
        private void PreviousPage()
        {
            if (CanPreviousPage())
            {
                CurrentPageIndex--;
            }
        }
        
        private void GotoPage(int index)
        {
            if (CurrentNotebook != null && index >= 0 && index < CurrentNotebook.Pages.Count)
            {
                CurrentPageIndex = index;
            }
        }
        
        // Tool Operations
        private void SetPenColor(Color color)
        {
            if (CurrentTool != null)
            {
                CurrentTool.Color = color;
            }
        }
        
        private void SetPenWidth(double width)
        {
            if (CurrentTool != null)
            {
                CurrentTool.Width = width;
            }
        }
        
        // Edit Operations
        private void Undo()
        {
            var action = UndoManager.Undo();
            if (action != null)
            {
                switch (action.Action)
                {
                    case UndoActionType.AddStroke:
                        if (action.Page != null && action.StrokeIndex >= 0 && action.StrokeIndex < action.Page.Strokes.Count)
                        {
                            action.Page.Strokes.RemoveAt(action.StrokeIndex);
                            StrokesChanged?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case UndoActionType.RemoveStroke:
                        if (action.Page != null && action.Stroke != null)
                        {
                            action.Page.Strokes.Insert(action.StrokeIndex, action.Stroke);
                            StrokesChanged?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case UndoActionType.AddPage:
                        if (action.Notebook != null && action.PageIndex >= 0 && action.PageIndex < action.Notebook.Pages.Count)
                        {
                            action.Notebook.RemovePage(action.PageIndex);
                            if (CurrentPageIndex >= action.Notebook.Pages.Count)
                            {
                                CurrentPageIndex = action.Notebook.Pages.Count - 1;
                            }
                        }
                        break;
                    case UndoActionType.RemovePage:
                        if (action.Notebook != null && action.Page != null)
                        {
                            action.Notebook.Pages.Insert(action.PageIndex, action.Page);
                            if (CurrentNotebook == action.Notebook)
                            {
                                CurrentPageIndex = action.PageIndex;
                            }
                        }
                        break;
                    case UndoActionType.ModifyStroke:
                        if (action.Page != null && action.StrokeIndex >= 0 && action.StrokeIndex < action.Page.Strokes.Count && action.OldStroke != null)
                        {
                            action.Page.Strokes[action.StrokeIndex] = action.OldStroke;
                            StrokesChanged?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                }
            }
        }
        
        private bool CanUndo() => UndoManager.CanUndo;
        
        private void Redo()
        {
            var action = UndoManager.Redo();
            if (action != null)
            {
                switch (action.Action)
                {
                    case UndoActionType.AddStroke:
                        if (action.Page != null && action.Stroke != null)
                        {
                            action.Page.Strokes.Insert(action.StrokeIndex, action.Stroke);
                            StrokesChanged?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case UndoActionType.RemoveStroke:
                        if (action.Page != null && action.StrokeIndex >= 0 && action.StrokeIndex < action.Page.Strokes.Count)
                        {
                            action.Page.Strokes.RemoveAt(action.StrokeIndex);
                            StrokesChanged?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case UndoActionType.AddPage:
                        if (action.Notebook != null && action.Page != null)
                        {
                            action.Notebook.Pages.Insert(action.PageIndex, action.Page);
                            if (CurrentNotebook == action.Notebook)
                            {
                                CurrentPageIndex = action.PageIndex;
                            }
                        }
                        break;
                    case UndoActionType.RemovePage:
                        if (action.Notebook != null && action.PageIndex >= 0 && action.PageIndex < action.Notebook.Pages.Count)
                        {
                            action.Notebook.RemovePage(action.PageIndex);
                            if (CurrentPageIndex >= action.Notebook.Pages.Count)
                            {
                                CurrentPageIndex = action.Notebook.Pages.Count - 1;
                            }
                        }
                        break;
                    case UndoActionType.ModifyStroke:
                        if (action.Page != null && action.StrokeIndex >= 0 && action.StrokeIndex < action.Page.Strokes.Count && action.NewStroke != null)
                        {
                            action.Page.Strokes[action.StrokeIndex] = action.NewStroke;
                            StrokesChanged?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                }
            }
        }
        
        private bool CanRedo() => UndoManager.CanRedo;
        
        private void Copy()
        {
            if (HasSelection)
            {
                // Copy selected strokes to clipboard
                var strokes = SelectedStrokes.Select(s => s.Clone()).ToList();
                Clipboard.SetDataObject(strokes, true);
            }
        }
        
        private bool CanCopy() => HasSelection;
        
        private void Paste()
        {
            if (CurrentPage == null) return;
            
            if (Clipboard.ContainsData(Stroke.StrokeDataFormat))
            {
                var strokes = Clipboard.GetData(Stroke.StrokeDataFormat) as List<Stroke>;
                if (strokes != null)
                {
                    // Offset pasted strokes
                    double offsetX = 20, offsetY = 20;
                    foreach (var stroke in strokes)
                    {
                        var newStroke = stroke.Clone();
                        for (int i = 0; i < newStroke.Points.Count; i++)
                        {
                            newStroke.Points[i] = new Point(
                                newStroke.Points[i].X + offsetX,
                                newStroke.Points[i].Y + offsetY);
                        }
                        CurrentPage.Strokes.Add(newStroke);
                        AddStrokeToHistory(newStroke);
                    }
                    StrokesChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        
        private bool CanPaste() => CurrentPage != null && Clipboard.ContainsData(Stroke.StrokeDataFormat);
        
        private void Cut()
        {
            if (HasSelection && CurrentPage != null)
            {
                var strokes = SelectedStrokes.Select(s => s.Clone()).ToList();
                Clipboard.SetDataObject(strokes, true);
                
                // Remove from page
                foreach (var stroke in SelectedStrokes)
                {
                    CurrentPage.Strokes.Remove(stroke);
                }
                
                SelectedStrokes = new List<Stroke>();
                StrokesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private bool CanCut() => HasSelection && CurrentPage != null;
        
        private void DeleteSelected()
        {
            if (HasSelection && CurrentPage != null)
            {
                // Save to undo stack
                var strokesToRemove = new List<Stroke>(SelectedStrokes);
                foreach (var stroke in strokesToRemove)
                {
                    var index = CurrentPage.Strokes.IndexOf(stroke);
                    UndoManager.Push(new UndoAction
                    {
                        Action = UndoActionType.RemoveStroke,
                        Page = CurrentPage,
                        Stroke = stroke.Clone(),
                        StrokeIndex = index
                    });
                }
                
                // Remove from page
                foreach (var stroke in SelectedStrokes)
                {
                    CurrentPage.Strokes.Remove(stroke);
                }
                
                SelectedStrokes = new List<Stroke>();
                StrokesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private bool CanDelete() => HasSelection && CurrentPage != null;
        
        private void SelectAll()
        {
            if (CurrentPage != null)
            {
                SelectedStrokes = new List<Stroke>(CurrentPage.Strokes);
            }
        }
        
        // Zoom Operations
        private void ZoomIn() => ZoomLevel += 0.1;
        private void ZoomOut() => ZoomLevel -= 0.1;
        private void ResetZoom() => ZoomLevel = 1.0;
        
        // Helper Methods
        public void AddStrokeToHistory(Stroke stroke)
        {
            UndoManager.Push(new UndoAction
            {
                Action = UndoActionType.AddStroke,
                Page = CurrentPage,
                Stroke = stroke.Clone(),
                StrokeIndex = CurrentPage.Strokes.Count - 1
            });
        }
        
        #endregion
    }
    
    // Undo/Redo Management
    public enum UndoActionType
    {
        AddStroke,
        RemoveStroke,
        ModifyStroke,
        AddPage,
        RemovePage
    }
    
    public class UndoAction
    {
        public UndoActionType Action { get; set; }
        public Notebook Notebook { get; set; }
        public Page Page { get; set; }
        public Stroke Stroke { get; set; }
        public Stroke OldStroke { get; set; }
        public Stroke NewStroke { get; set; }
        public int StrokeIndex { get; set; }
        public int PageIndex { get; set; }
    }
    
    public class UndoManager
    {
        private Stack<UndoAction> _undoStack = new Stack<UndoAction>();
        private Stack<UndoAction> _redoStack = new Stack<UndoAction>();
        
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        
        public void Push(UndoAction action)
        {
            _undoStack.Push(action);
            _redoStack.Clear(); // Clear redo stack when new action is pushed
        }
        
        public UndoAction Undo()
        {
            if (_undoStack.Count == 0) return null;
            
            var action = _undoStack.Pop();
            _redoStack.Push(action);
            return action;
        }
        
        public UndoAction Redo()
        {
            if (_redoStack.Count == 0) return null;
            
            var action = _redoStack.Pop();
            _undoStack.Push(action);
            return action;
        }
        
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
