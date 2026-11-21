using System;
using System.IO;
using System.Windows.Media.Imaging;
using GoodNotesClone.Models;

namespace GoodNotesClone.Services
{
    public static class FileService
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GoodNotesClone");
        
        public static void Initialize()
        {
            if (!Directory.Exists(AppDataFolder))
                Directory.CreateDirectory(AppDataFolder);
        }
        
        public static string GetNotebookFilePath(string notebookName)
        {
            return Path.Combine(AppDataFolder, notebookName + ".gnc");
        }
        
        public static void SaveNotebook(Notebook notebook, string filePath = null)
        {
            filePath = filePath ?? GetNotebookFilePath(notebook.Name);
            notebook.FilePath = filePath;
            notebook.ModifiedDate = DateTime.Now;
            JsonHelper.Save(filePath, notebook);
        }
        
        public static Notebook LoadNotebook(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Notebook file not found", filePath);
            
            var notebook = JsonHelper.Load<Notebook>(filePath);
            notebook.FilePath = filePath;
            return notebook;
        }
        
        public static void ExportToPdf(Notebook notebook, string filePath)
        {
            // Placeholder for PDF export
            // In a real implementation, use a PDF library like iTextSharp or PdfPig
        }
        
        public static void ExportToImage(Notebook notebook, string filePath, ImageFormat format)
        {
            // Placeholder for image export
        }
        
        public static Notebook ImportFromPdf(string filePath)
        {
            // Placeholder for PDF import
            var notebook = new Notebook(Path.GetFileNameWithoutExtension(filePath));
            notebook.AddPage(PageTemplate.Blank);
            return notebook;
        }
        
        public static Notebook ImportFromImage(string filePath)
        {
            // Placeholder for image import
            var notebook = new Notebook(Path.GetFileNameWithoutExtension(filePath));
            notebook.AddPage(PageTemplate.Blank);
            return notebook;
        }
    }
    
    public enum ImageFormat
    {
        PNG,
        JPEG,
        BMP,
        GIF
    }
}
