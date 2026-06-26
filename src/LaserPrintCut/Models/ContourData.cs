using System.Collections.Generic;
using System.Drawing;

namespace LaserPrintCutAddin.Models
{
    public class ContourData
    {
        public List<PointF> Points { get; set; } = new List<PointF>();
        public List<List<PointF>> Holes { get; set; } = new List<List<PointF>>();
        public RectangleF BoundingBox { get; set; }
        public bool IsClosed { get; set; } = true;
        public double Area { get; set; }
        public double Perimeter { get; set; }
        public int NodeCount => Points.Count;
    }
}