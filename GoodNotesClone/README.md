# GoodNotes Clone for Windows

A professional note-taking application for Windows, inspired by GoodNotes for iPad.

## Features

### Core Features
- **Handwriting & Drawing**: Smooth, natural drawing with stylus or mouse support
- **Multiple Tools**: Pen, Highlighter, Eraser, Lasso selection, Hand tool
- **Page Management**: Create, delete, and navigate between pages
- **Notebook Organization**: Create and manage multiple notebooks
- **Undo/Redo**: Full undo and redo support
- **Zoom**: Zoom in/out on the canvas
- **Save/Load**: Save notebooks to files and load them later

### Drawing Features
- **Pressure Sensitivity**: Variable stroke width based on pressure (if stylus supports it)
- **Stroke Smoothing**: Smooth strokes for better appearance
- **Multiple Colors**: Choose from a variety of colors
- **Adjustable Width**: Customize pen and highlighter widths
- **Stroke Selection**: Select and manipulate individual strokes
- **Lasso Selection**: Select multiple strokes with lasso tool

### Page Features
- **Page Templates**: Blank, Lined, Grid, and Dotted templates
- **Page Navigation**: Easy navigation between pages
- **Clear Page**: Remove all strokes from a page

## Requirements

- Windows 7 or later
- .NET 7.0 or later
- Visual Studio 2022 (for development)

## Installation

1. Clone this repository
2. Open the solution in Visual Studio 2022
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)

## Usage

### Basic Operations
- **New Notebook**: File > New or Ctrl+N
- **Open Notebook**: File > Open or Ctrl+O
- **Save Notebook**: File > Save or Ctrl+S
- **Save As**: File > Save As

### Drawing Tools
- **Pen**: Draw with various pen types and colors (P)
- **Highlighter**: Highlight text and drawings (H)
- **Eraser**: Remove strokes (E)
- **Lasso**: Select multiple strokes (L)
- **Hand**: Pan the canvas

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+N | New Notebook |
| Ctrl+O | Open Notebook |
| Ctrl+S | Save Notebook |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Ctrl+P | Select Pen |
| Ctrl+H | Select Highlighter |
| Ctrl+E | Select Eraser |
| Ctrl+L | Select Lasso |
| Ctrl+A | Select All |
| Delete | Delete Selected |
| Ctrl+C | Copy Selected |
| Ctrl+V | Paste |
| Ctrl+X | Cut Selected |
| Ctrl+Left | Previous Page |
| Ctrl+Right | Next Page |
| Ctrl++ | Zoom In |
| Ctrl+- | Zoom Out |
| Ctrl+0 | Reset Zoom |

### Mouse/Gesture Controls

- **Left Click + Drag**: Draw with current tool
- **Right Click**: Select stroke / show context menu
- **Mouse Wheel**: Scroll canvas
- **Ctrl + Mouse Wheel**: Zoom in/out

## Project Structure

```
GoodNotesClone/
├── GoodNotesClone.sln          # Visual Studio Solution
├── GoodNotesClone/
│   ├── GoodNotesClone.csproj   # WPF Project
│   ├── App.xaml                # Application definition
│   ├── App.xaml.cs             # Application code-behind
│   ├── app.manifest            # Application manifest
│   ├── Models/
│   │   ├── Stroke.cs           # Stroke model
│   │   ├── Page.cs             # Page model
│   │   ├── Notebook.cs         # Notebook model
│   │   └── DrawingTool.cs      # Drawing tool model
│   ├── ViewModels/
│   │   └── MainViewModel.cs    # Main view model
│   ├── Views/
│   │   └── MainWindow.xaml     # Main window
│   │   └── MainWindow.xaml.cs  # Main window code-behind
│   ├── Services/
│   │   └── FileService.cs      # File I/O service
│   ├── Converters/
│   │   └── ToolTypeToCheckedConverter.cs  # Tool type converters
│   └── Resources/
│       ├── Styles.xaml         # Application styles
│       └── app.ico             # Application icon
└── README.md                   # This file
```

## Building

### Prerequisites
- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (optional, for GUI development)

### Build from Command Line

```bash
# Navigate to the project directory
cd GoodNotesClone

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

### Build with Visual Studio
1. Open `GoodNotesClone.sln` in Visual Studio
2. Select `Debug` or `Release` configuration
3. Build the solution (Ctrl+Shift+B)
4. Run (F5) or Start Without Debugging (Ctrl+F5)

## Configuration

The application stores settings in:
- `%APPDATA%\GoodNotesClone\settings.json` - Application settings
- `%APPDATA%\GoodNotesClone\` - Notebook files (default save location)

## Extending the Application

### Adding New Features
1. **New Tools**: Add new tool types in the `ToolType` enum and implement the logic in `MainViewModel`
2. **New Page Templates**: Add new templates in the `PageTemplate` enum and implement rendering in the canvas
3. **New File Formats**: Add support for new file formats in `FileService`
4. **New Commands**: Add new commands in `MainViewModel` and bind them in the XAML

### Customizing Styles
Edit the `Resources/Styles.xaml` file to customize the appearance of the application.

## Troubleshooting

### Common Issues

1. **Application won't start**: Make sure .NET 7.0 is installed
2. **Strokes don't appear**: Check if the canvas is properly bound to the view model
3. **Save/Load not working**: Check file permissions in the save location
4. **Keyboard shortcuts not working**: Make sure the window has focus

### Debugging
- Use Visual Studio's debugger (F5)
- Check the Output window for errors
- Enable exception handling in `App.xaml.cs`

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

## License

This project is licensed under the MIT License.

## Credits

Inspired by [GoodNotes](https://www.goodnotes.com/) for iPad.

## Contact

For questions or feedback, please open an issue on GitHub.

---

**Version**: 1.0.0  
**Last Updated**: 2024
