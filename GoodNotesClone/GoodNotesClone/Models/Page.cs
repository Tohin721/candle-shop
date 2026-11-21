using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GoodNotesClone.Models
{
    public class Page : ICloneable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Untitled Page";
        public int Index { get; set; } = 0;
        public PageTemplate TemplateType { get; set; } = PageTemplate.Blank;
        public double Width { get; set; } = 800;
        public double Height { get; set; } = 1131;
        public Color BackgroundColor { get; set; } = Colors.White;
        public StrokeCollection Strokes { get; set; } = new StrokeCollection();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public bool IsVisible { get; set; } = true;
        
        // For backward compatibility
        public PageTemplate Template
        {
            get => TemplateType;
            set => TemplateType = value;
        }
        
        public List<Line> TemplateLines { get; set; } = new List<Line>();
        
        public void GenerateTemplateLines()
        {
            TemplateLines.Clear();
            
            switch (TemplateType)
            {
                case PageTemplate.Lined:
                    for (double y = 0; y < Height; y += 30)
                    {
                        TemplateLines.Add(new Line
                        {
                            X1 = 0,
                            Y1 = y,
                            X2 = Width,
                            Y2 = y,
                            Stroke = Brushes.LightGray,
                            StrokeThickness = 1,
                            StrokeDashArray = new DoubleCollection { 5, 5 }
                        });
                    }
                    break;
                    
                case PageTemplate.Grid:
                    for (double x = 0; x < Width; x += 20)
                    {
                        TemplateLines.Add(new Line
                        {
                            X1 = x,
                            Y1 = 0,
                            X2 = x,
                            Y2 = Height,
                            Stroke = Brushes.LightGray,
                            StrokeThickness = 1,
                            StrokeDashArray = new DoubleCollection { 5, 5 }
                        });
                    }
                    for (double y = 0; y < Height; y += 20)
                    {
                        TemplateLines.Add(new Line
                        {
                            X1 = 0,
                            Y1 = y,
                            X2 = Width,
                            Y2 = y,
                            Stroke = Brushes.LightGray,
                            StrokeThickness = 1,
                            StrokeDashArray = new DoubleCollection { 5, 5 }
                        });
                    }
                    break;
                    
                case PageTemplate.Dotted:
                    // Dots don't need lines, they'll be drawn as ellipses
                    break;
            }
        }
        
        public Page() { }
        
        public Page(string name, PageTemplate template, double width, double height)
        {
            Name = name;
            Template = template;
            Width = width;
            Height = height;
        }
        
        public Rect GetBounds()
        {
            if (Strokes.Count == 0) return new Rect(0, 0, Width, Height);
            return Strokes.GetBounds();
        }
        
        public void AddStroke(Stroke stroke)
        {
            Strokes.Add(stroke);
            ModifiedDate = DateTime.Now;
        }
        
        public void RemoveStroke(Stroke stroke)
        {
            Strokes.Remove(stroke);
            ModifiedDate = DateTime.Now;
        }
        
        public void Clear()
        {
            Strokes.Clear();
            ModifiedDate = DateTime.Now;
        }
        
        public Page Clone()
        {
            var clone = new Page
            {
                Id = Guid.NewGuid(),
                Name = Name + " (Copy)",
                Index = Index,
                Template = Template,
                Width = Width,
                Height = Height,
                BackgroundColor = BackgroundColor,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsVisible = IsVisible
            };
            foreach (var stroke in Strokes)
                clone.Strokes.Add(stroke.Clone());
            return clone;
        }
        
        object ICloneable.Clone() => Clone();
    }
    
    public class StrokeCollection : List<Stroke>
    {
        public StrokeCollection() { }
        public StrokeCollection(IEnumerable<Stroke> strokes) : base(strokes) { }
        
        public List<Stroke> GetStrokesInRect(Rect rect)
        {
            var result = new List<Stroke>();
            foreach (var stroke in this)
            {
                if (stroke.GetBounds().IntersectsWith(rect))
                    result.Add(stroke);
            }
            return result;
        }
        
        public List<Stroke> GetSelectedStrokes()
        {
            return FindAll(s => s.IsSelected);
        }
        
        public Rect GetBounds()
        {
            if (Count == 0) return Rect.Empty;
            Rect bounds = this[0].GetBounds();
            for (int i = 1; i < Count; i++)
                bounds = Rect.Union(bounds, this[i].GetBounds());
            return bounds;
        }
        
        public void SelectAll() => ForEach(s => s.IsSelected = true);
        public void DeselectAll() => ForEach(s => s.IsSelected = false);
        public void DeleteSelected() => RemoveAll(s => s.IsSelected);
        public void TranslateAll(double dx, double dy) => ForEach(s => s.Translate(dx, dy));
    }
}
