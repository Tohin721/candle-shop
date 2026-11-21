using System.Windows;
using System.Windows.Media;

namespace GoodNotesClone.Models
{
    public class DrawingTool
    {
        public ToolType Type { get; set; } = ToolType.Pen;
        public Color Color { get; set; } = Colors.Black;
        public double Width { get; set; } = 2.0;
        public PenType PenType { get; set; } = PenType.BallPen;
        public bool IsHighlighter { get; set; } = false;
        
        public DrawingTool() { }
        
        public DrawingTool(ToolType type, Color color, double width)
        {
            Type = type;
            Color = color;
            Width = width;
        }
        
        public void SetPen(Color color, double width, PenType penType)
        {
            Type = ToolType.Pen;
            Color = color;
            Width = width;
            PenType = penType;
            IsHighlighter = false;
        }
        
        public void SetHighlighter(Color color, double width)
        {
            Type = ToolType.Highlighter;
            Color = color;
            Width = width;
            IsHighlighter = true;
        }
        
        public void SetEraser(double width)
        {
            Type = ToolType.Eraser;
            Width = width;
        }
        
        public void SetLasso()
        {
            Type = ToolType.Lasso;
        }
        
        public void SetText()
        {
            Type = ToolType.Text;
        }
        
        public void SetHand()
        {
            Type = ToolType.Hand;
        }
        
        public Stroke CreateStroke()
        {
            return new Stroke { Color = Color, Width = Width, PenType = PenType, IsHighlighter = IsHighlighter };
        }
    }
    
    public class ToolSettings
    {
        public static readonly Color[] DefaultColors = new Color[]
        {
            Colors.Black,
            Colors.Red,
            Colors.Blue,
            Colors.Green,
            Colors.Purple,
            Colors.Orange,
            Colors.Yellow,
            Colors.Pink,
            Colors.Brown,
            Colors.Gray
        };
        
        public static readonly Color[] HighlighterColors = new Color[]
        {
            Colors.Yellow,
            Colors.Pink,
            Colors.Orange,
            Colors.Green,
            Colors.Blue,
            Colors.Purple
        };
        
        public static readonly double[] DefaultWidths = new double[] { 0.5, 1, 2, 3, 4, 5, 8, 10, 15, 20 };
    }
}
