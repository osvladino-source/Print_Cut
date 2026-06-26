using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenCvSharp;
using LaserPrintCutAddin.Models;

namespace LaserPrintCutAddin.Services
{
    public class ImageProcessingService
    {
        public async Task<Mat> LoadImageFromCorelDrawAsync()
        {
            return await Task.Run(() =>
            {
                var imagePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Resources", "sample_image.png");

                if (!File.Exists(imagePath))
                    CreateSampleImage(imagePath);

                return new Mat(imagePath);
            });
        }

        public async Task<ProcessedImageResult> ProcessImageAsync(
            Mat sourceImage, byte thresholdValue, double offsetValue, bool keepHoles)
        {
            return await Task.Run(() =>
            {
                if (sourceImage == null || sourceImage.Empty())
                {
                    return new ProcessedImageResult
                    {
                        ThresholdValue = thresholdValue,
                        Offset = offsetValue,
                        HasHoles = keepHoles
                    };
                }

                var result = new ProcessedImageResult
                {
                    ThresholdValue = thresholdValue,
                    Offset = offsetValue,
                    HasHoles = keepHoles
                };

                using var gray = new Mat();
                Cv2.CvtColor(sourceImage, gray, ColorConversionCodes.BGR2GRAY);

                Cv2.Threshold(gray, gray, thresholdValue, 255, ThresholdTypes.Binary);

                var mode = keepHoles ? RetrievalModes.CComp : RetrievalModes.External;
                Cv2.FindContours(gray, out var contours, out var hierarchy, mode, ContourApproximationModes.ApproxSimple);

                result.ContourMat = gray.Clone();
                result.ContourPoints = contours.Length > 0 ? contours[0] : Array.Empty<Point>();

                var approxList = new List<Point[]>();
                foreach (var contour in contours)
                {
                    if (contour.Length < 3) continue;

                    var epsilon = 0.02 * Cv2.ArcLength(contour, true);
                    var approx = Cv2.ApproxPolyDP(contour, epsilon, true);
                    approxList.Add(approx);
                }

                result.ApproximatedContours = approxList.ToArray();
                return result;
            });
        }

        public Mat ApplyOffset(Mat binaryImage, double offsetPixels)
        {
            if (Math.Abs(offsetPixels) < 0.01) return binaryImage.Clone();

            var kernel = Cv2.GetStructuringElement(
                MorphShapes.Ellipse,
                new Size((int)Math.Abs(offsetPixels) * 2 + 1, (int)Math.Abs(offsetPixels) * 2 + 1));

            var result = new Mat();
            if (offsetPixels > 0)
                Cv2.Dilate(binaryImage, result, kernel);
            else
                Cv2.Erode(binaryImage, result, kernel);

            return result;
        }

        private void CreateSampleImage(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using var image = new Mat(500, 500, MatType.CV_8UC3, Scalar.White);
            Cv2.Rectangle(image, new Rect(100, 100, 300, 300), Scalar.Black, -1);
            Cv2.Circle(image, new Point(250, 250), 80, Scalar.White, -1);

            Cv2.ImWrite(path, image);
        }
    }
}