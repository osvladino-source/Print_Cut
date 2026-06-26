using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using OpenCvSharp;

namespace LaserPrintCutAddin.Services
{
    public class CoreldrawIntegrationService
    {
        private readonly dynamic _corelApp;

        public CoreldrawIntegrationService(dynamic corelApp)
        {
            _corelApp = corelApp ?? throw new ArgumentNullException(nameof(corelApp));
        }

        public string GetSelectedImagePath()
        {
            try
            {
                var doc = _corelApp.ActiveDocument;
                var sel = doc.SelectionRange;
                if (sel == null || sel.Count == 0)
                    throw new Exception("Nenhum objeto selecionado.");

                var shape = sel.Item(1);
                var tempFile = Path.GetTempFileName() + ".bmp";

                var filter = doc.SupportedExportFilters.Find("BMP - Windows Bitmap");
                if (filter == null)
                    filter = doc.SupportedExportFilters.Find("BMP");

                var exportOptions = filter.ExportOptions;
                exportOptions.Resolution = 96;
                exportOptions.Antialiasing = true;

                shape.Export(tempFile, filter, exportOptions);

                return tempFile;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao exportar selecao: {ex.Message}");
            }
        }

        public CreateContourResult CreateOnlyContour(
            OpenCvSharp.Point[] contourPoints,
            OpenCvSharp.Point[][] approximatedContours)
        {
            try
            {
                var activeLayer = _corelApp.ActiveLayer;
                var curve = activeLayer.CreateCurve();

                var points = approximatedContours?[0] ?? contourPoints;
                if (points == null || points.Length < 2)
                    return new CreateContourResult { Success = false, Message = "Pontos insuficientes" };

                var segment = curve.CreateSegment(points.Length);
                for (int i = 0; i < points.Length; i++)
                {
                    var x = ConvertToDocumentUnits(points[i].X);
                    var y = ConvertToDocumentUnits(points[i].Y);
                    segment.Append(2, x, y);
                }
                segment.Append(2,
                    ConvertToDocumentUnits(points[0].X),
                    ConvertToDocumentUnits(points[0].Y));

                curve.Fill.Delete();
                var pen = curve.Outline;
                pen.Color.RGBA = Color.Black.ToArgb();
                pen.Width = ConvertToDocumentUnits(0.0762);
                pen.LineCaps = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                return new CreateContourResult { Success = true, Message = "Contorno criado com sucesso" };
            }
            catch (Exception ex)
            {
                return new CreateContourResult { Success = false, Message = $"Erro: {ex.Message}" };
            }
        }

        public CreatePrintCutResult CreatePrintCutMode(
            Bitmap sourceImage,
            OpenCvSharp.Point[] contourPoints,
            OpenCvSharp.Point[][] approximatedContours)
        {
            try
            {
                var doc = _corelApp.ActiveDocument;
                var page = doc.ActivePage;
                var printLayer = page.CreateLayer("Print");
                var cutLayer = page.CreateLayer("Cut");

                var importedShape = printLayer.ImportBitmap(sourceImage);

                var imageWidth = ConvertToDocumentUnits(sourceImage.Width);
                var imageHeight = ConvertToDocumentUnits(sourceImage.Height);

                var boundingRect = printLayer.CreateRectangle(0, 0, imageWidth, imageHeight);
                boundingRect.Outline.Color.RGBA = Color.Black.ToArgb();
                boundingRect.Fill.Delete();

                var marks = GenerateRegistrationMarks(0, 0, imageWidth, imageHeight, printLayer);
                var mark1 = GenerateRegistrationMarks(imageWidth, imageHeight, 0, 0, printLayer);

                var points = approximatedContours?[0] ?? contourPoints;
                if (points != null && points.Length >= 2)
                {
                    var cutCurve = cutLayer.CreateCurve();
                    var segment = cutCurve.CreateSegment(points.Length);
                    for (int i = 0; i < points.Length; i++)
                    {
                        segment.Append(2,
                            ConvertToDocumentUnits(points[i].X),
                            ConvertToDocumentUnits(points[i].Y));
                    }
                    segment.Append(2,
                        ConvertToDocumentUnits(points[0].X),
                        ConvertToDocumentUnits(points[0].Y));

                    cutCurve.Fill.Delete();
                    var cutPen = cutCurve.Outline;
                    cutPen.Color.RGBA = Color.Black.ToArgb();
                    cutPen.Width = ConvertToDocumentUnits(0.0762);
                }

                var cutMarks = GenerateRegistrationMarks(0, 0, imageWidth, imageHeight, cutLayer);

                return new CreatePrintCutResult
                {
                    Success = true,
                    Message = "Modo Print & Cut criado com sucesso"
                };
            }
            catch (Exception ex)
            {
                return new CreatePrintCutResult { Success = false, Message = $"Erro: {ex.Message}" };
            }
        }

        private dynamic GenerateRegistrationMarks(double x, double y, double width, double height, dynamic layer)
        {
            var group = layer.CreateGroup();
            var diameter = ConvertToDocumentUnits(10.0);

            var circle = group.CreateEllipse2(x, y, diameter, diameter);
            circle.Fill.Delete();
            circle.Outline.Color.RGBA = Color.Red.ToArgb();
            circle.Outline.Width = ConvertToDocumentUnits(0.2);

            var hLine = group.CreateLineSegment(
                x - diameter, y,
                x + diameter, y);
            hLine.Outline.Color.RGBA = Color.Red.ToArgb();
            hLine.Outline.Width = ConvertToDocumentUnits(0.2);

            var vLine = group.CreateLineSegment(
                x, y - diameter,
                x, y + diameter);
            vLine.Outline.Color.RGBA = Color.Red.ToArgb();
            vLine.Outline.Width = ConvertToDocumentUnits(0.2);

            return group;
        }

        private double ConvertToDocumentUnits(double pixels)
        {
            return pixels * 0.352778;
        }


    }

    public class CreateContourResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class CreatePrintCutResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
