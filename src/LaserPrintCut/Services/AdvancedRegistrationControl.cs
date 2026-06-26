using System;
using System.Collections.Generic;
using System.Drawing;

namespace LaserPrintCutAddin.Services
{
    public class RegistrationMarkDefinition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double DiameterMm { get; set; } = 10.0;
        public RegistrationMarkType MarkType { get; set; } = RegistrationMarkType.CrosshairCircle;
    }

    public enum RegistrationMarkType
    {
        CrosshairCircle,
        CrosshairOnly,
        CircleOnly,
        CornerL,
        Custom
    }

    public class AdvancedRegistrationControl
    {
        private const double DefaultMarkDiameter = 10.0;
        private const double RegistrationMarkOffset = 5.0;

        public List<RegistrationMarkDefinition> GenerateRegistrationMarks(
            RectangleF boundingBox,
            int markCount = 2,
            double markDiameter = DefaultMarkDiameter,
            RegistrationMarkType markType = RegistrationMarkType.CrosshairCircle)
        {
            var marks = new List<RegistrationMarkDefinition>();

            if (markCount < 2)
                markCount = 2;

            if (markCount > 4)
                markCount = 4;

            if (markCount == 2)
            {
                marks.Add(new RegistrationMarkDefinition
                {
                    X = boundingBox.Left - RegistrationMarkOffset,
                    Y = boundingBox.Top - RegistrationMarkOffset,
                    DiameterMm = markDiameter,
                    MarkType = markType
                });

                marks.Add(new RegistrationMarkDefinition
                {
                    X = boundingBox.Right + RegistrationMarkOffset,
                    Y = boundingBox.Bottom + RegistrationMarkOffset,
                    DiameterMm = markDiameter,
                    MarkType = markType
                });
            }
            else if (markCount == 3)
            {
                marks.Add(new RegistrationMarkDefinition
                {
                    X = boundingBox.Left - RegistrationMarkOffset,
                    Y = boundingBox.Top - RegistrationMarkOffset,
                    DiameterMm = markDiameter,
                    MarkType = markType
                });

                marks.Add(new RegistrationMarkDefinition
                {
                    X = boundingBox.Right + RegistrationMarkOffset,
                    Y = boundingBox.Top - RegistrationMarkOffset,
                    DiameterMm = markDiameter,
                    MarkType = markType
                });

                marks.Add(new RegistrationMarkDefinition
                {
                    X = boundingBox.Right + RegistrationMarkOffset,
                    Y = boundingBox.Bottom + RegistrationMarkOffset,
                    DiameterMm = markDiameter,
                    MarkType = markType
                });
            }
            else if (markCount == 4)
            {
                marks.Add(new RegistrationMarkDefinition
                {
                    X = boundingBox.Left - RegistrationMarkOffset,
                    Y = boundingBox.Top - RegistrationMarkOffset,
                    DiameterMm = markDiameter,
                    MarkType = markType
                });

                marks.Add(new RegistrationMarkDefinition
                {
                    X = boundingBox.Right + RegistrationMarkOffset,
                    Y = boundingBox.Top - RegistrationMarkOffset,
                    DiameterMm = markDiameter,
                    MarkType = markType
                });

                marks.Add(new RegistrationMarkDefinition
                {
                    X = boundingBox.Left - RegistrationMarkOffset,
                    Y = boundingBox.Bottom + RegistrationMarkOffset,
                    DiameterMm = markDiameter,
                    MarkType = markType
                });

                marks.Add(new RegistrationMarkDefinition
                {
                    X = boundingBox.Right + RegistrationMarkOffset,
                    Y = boundingBox.Bottom + RegistrationMarkOffset,
                    DiameterMm = markDiameter,
                    MarkType = markType
                });
            }

            return marks;
        }

        public void ValidateRegistrationMarks(List<RegistrationMarkDefinition> marks)
        {
            if (marks == null || marks.Count < 2)
                throw new ArgumentException("Pelo menos 2 marcas de registro são necessárias");

            foreach (var mark in marks)
            {
                if (mark.DiameterMm <= 0)
                    throw new ArgumentException("Diâmetro da marca deve ser maior que zero");
            }

            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;

            foreach (var mark in marks)
            {
                minX = Math.Min(minX, mark.X);
                minY = Math.Min(minY, mark.Y);
                maxX = Math.Max(maxX, mark.X);
                maxY = Math.Max(maxY, mark.Y);
            }
        }

        public (double width, double height) CalculateRegistrationArea(
            List<RegistrationMarkDefinition> marks)
        {
            if (marks == null || marks.Count < 2)
                return (0, 0);

            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;

            foreach (var mark in marks)
            {
                minX = Math.Min(minX, mark.X);
                minY = Math.Min(minY, mark.Y);
                maxX = Math.Max(maxX, mark.X);
                maxY = Math.Max(maxY, mark.Y);
            }

            return (maxX - minX, maxY - minY);
        }
    }
}