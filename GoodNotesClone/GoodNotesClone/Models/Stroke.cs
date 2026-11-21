using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace GoodNotesClone.Models
{
    public class Stroke : ICloneable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<Point> Points { get; set; } = new List<Point>();
        public Color Color { get; set; } = Colors.Black;
        public double Width { get; set; } = 2.0;
        public PenType PenType { get; set; } = PenType.BallPen;
        public bool IsHighlighter { get; set; } = false;
        public List<float> Pressures { get; set; } = new List<float>();
        public long CreatedTimestamp { get; set; } = DateTime.UtcNow.Ticks;
        public int Layer { get; set; } = 0;
        public bool IsSelected { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        
        public Stroke() { }
        
        public Stroke(IEnumerable<Point> points, Color color, double width, PenType penType = PenType.BallPen, bool isHighlighter = false)
        {
            Points = new List<Point>(points);
            Color = color;
            Width = width;
            PenType = penType;
            IsHighlighter = isHighlighter;
            Pressures = new List<float>(new float[Points.Count]);
            for (int i = 0; i < Pressures.Count; i++)
                Pressures[i] = 0.5f;
        }
        
        public void AddPoint(Point point, float pressure = 0.5f)
        {
            Points.Add(point);
            Pressures.Add(pressure);
        }
        
        public Rect GetBounds()
        {
            if (Points.Count == 0) return Rect.Empty;
            double minX = Points[0].X, maxX = Points[0].X;
            double minY = Points[0].Y, maxY = Points[0].Y;
            foreach (var p in Points)
            {
                minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
            }
            double padding = Width / 2;
            return new Rect(minX - padding, minY - padding, maxX - minX + padding * 2, maxY - minY + padding * 2);
        }
        
        public Geometry GetGeometry()
        {
            if (Points.Count < 2) return Geometry.Empty;
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = Points[0], IsClosed = false, IsFilled = false };
            var segments = new PathSegmentCollection();
            for (int i = 1; i < Points.Count; i++)
                segments.Add(new LineSegment(Points[i], true));
            figure.Segments = segments;
            geometry.Figures.Add(figure);
            return geometry;
        }
        
        public void Translate(double dx, double dy)
        {
            for (int i = 0; i < Points.Count; i++)
                Points[i] = new Point(Points[i].X + dx, Points[i].Y + dy);
        }
        
        public Stroke Clone()
        {
            return new Stroke
            {
                Id = Guid.NewGuid(),
                Points = new List<Point>(Points),
                Color = Color,
                Width = Width,
                PenType = PenType,
                IsHighlighter = IsHighlighter,
                Pressures = new List<float>(Pressures),
                CreatedTimestamp = CreatedTimestamp,
                Layer = Layer,
                IsSelected = false,
                IsVisible = IsVisible
            };
        }
        
        object ICloneable.Clone() => Clone();
    }
}
