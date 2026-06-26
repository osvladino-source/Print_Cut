using OpenCvSharp;

namespace LaserPrintCutAddin.Models
{
    public class ProcessedImageResult
    {
        public Mat ContourMat { get; set; }
        public Point[] ContourPoints { get; set; }
        public byte ThresholdValue { get; set; }
        public bool HasHoles { get; set; }
        public double Offset { get; set; }
        public Point[][] ApproximatedContours { get; set; }
    }
}