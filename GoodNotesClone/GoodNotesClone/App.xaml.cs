using System;
using System.Windows;
using System.Windows.Media;

namespace GoodNotesClone
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                MessageBox.Show($"Fatal Error: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

// Enums
public enum ToolType { Pen, Highlighter, Eraser, Lasso, Text, Hand }
public enum PageTemplate { Blank, Lined, Grid, Dotted }
public enum PenType { BallPen, FountainPen, BrushPen, Marker }

// Constants
public static class Constants
{
    public const double MinStrokeWidth = 0.5;
    public const double MaxStrokeWidth = 50;
    public const double DefaultStrokeWidth = 2.0;
}

// Utility classes
public static class MathUtils
{
    public static double Distance(Point p1, Point p2)
    {
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;
        return System.Math.Sqrt(dx * dx + dy * dy);
    }
}

public static class ColorUtils
{
    public static Color FromHex(string hex)
    {
        hex = hex.Replace("#", "");
        if (hex.Length == 6)
        {
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromRgb(r, g, b);
        }
        return Colors.Black;
    }
    
    public static string ToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}

// RelayCommand for MVVM
public class RelayCommand : System.Windows.Input.ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    
    public event EventHandler CanExecuteChanged
    {
        add { System.Windows.Input.CommandManager.RequerySuggested += value; }
        remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
    }
    
    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
    public void Execute(object parameter) => _execute();
}

public class RelayCommand<T> : System.Windows.Input.ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;
    
    public event EventHandler CanExecuteChanged
    {
        add { System.Windows.Input.CommandManager.RequerySuggested += value; }
        remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
    }
    
    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
    public void Execute(object parameter) => _execute((T)parameter);
}

// ViewModel base
public abstract class ViewModelBase : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetField<T>(ref T field, T value, string propertyName)
    {
        if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

// Dialog utilities
public static class DialogUtils
{
    public static bool? ShowMessage(string message, string title = "Message", 
        MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
    {
        return MessageBox.Show(message, title, buttons, icon);
    }
    
    public static string ShowInputDialog(string prompt, string title = "Input", string defaultValue = "")
    {
        var window = new Window
        {
            Title = title, Width = 400, Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            WindowStyle = WindowStyle.ToolWindow, ResizeMode = ResizeMode.NoResize
        };
        
        var stack = new System.Windows.Controls.StackPanel { Margin = new Thickness(20) };
        var textBlock = new System.Windows.Controls.TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 10), TextWrapping = TextWrapping.Wrap };
        var textBox = new System.Windows.Controls.TextBox { Text = defaultValue, Margin = new Thickness(0, 0, 0, 10), MinHeight = 40 };
        
        var btnPanel = new System.Windows.Controls.StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var okBtn = new System.Windows.Controls.Button { Content = "OK", Width = 80, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
        var cancelBtn = new System.Windows.Controls.Button { Content = "Cancel", Width = 80, IsCancel = true };
        
        okBtn.Click += (s, e) => { window.DialogResult = true; window.Close(); };
        cancelBtn.Click += (s, e) => { window.DialogResult = false; window.Close(); };
        
        btnPanel.Children.Add(okBtn);
        btnPanel.Children.Add(cancelBtn);
        stack.Children.Add(textBlock);
        stack.Children.Add(textBox);
        stack.Children.Add(btnPanel);
        window.Content = stack;
        
        return window.ShowDialog() == true ? textBox.Text : null;
    }
}

// File dialog utilities
public static class FileDialogUtils
{
    public static string ShowOpenDialog(string filter = "All Files (*.*)|*.*", string title = "Open")
    {
        var dialog = new Microsoft.Win32.OpenFileDialog { Filter = filter, Title = title, Multiselect = false };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
    
    public static string ShowSaveDialog(string filter = "All Files (*.*)|*.*", string title = "Save", string defaultName = null)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog { Filter = filter, Title = title, FileName = defaultName, AddExtension = true };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}

// Serialization
public static class JsonHelper
{
    public static string Serialize<T>(T obj) => Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
    public static T Deserialize<T>(string json) => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
    public static void Save<T>(string path, T obj) => System.IO.File.WriteAllText(path, Serialize(obj));
    public static T Load<T>(string path) => System.IO.File.Exists(path) ? Deserialize<T>(System.IO.File.ReadAllText(path)) : default;
}

// Extension methods
public static class Extensions
{
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0) return min;
        if (value.CompareTo(max) > 0) return max;
        return value;
    }
}
