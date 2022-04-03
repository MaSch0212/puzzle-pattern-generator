using MaSch.Core.Extensions;
using PuzzlePatternGenerator.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PuzzlePatternGenerator.Services
{
    public class PuzzleGenerator : IPuzzleGenerator
    {
        public async Task<Geometry> GenerateGeometryAsync(PuzzleGeneratorOptions options)
        {
            var result = new GeometryGroup();

            var (rowLinePoints, columnLinePoints) = await Task.Run(() => GeneratePoints(options));

            foreach (var rowPoints in rowLinePoints)
                result.Children.Add(MakeBezierPath(rowPoints));
            foreach (var columnPoints in columnLinePoints)
                result.Children.Add(MakeBezierPath(columnPoints));

            if (options.IsBorderEnabled)
            {
                var figure = new PathFigure
                {
                    StartPoint = new Point(0, 0),
                    IsClosed = true,
                    Segments = new PathSegmentCollection
                    {
                        new PolyLineSegment
                        {
                            Points = new PointCollection
                            {
                                new Point(options.Width, 0),
                                new Point(options.Width, options.Height),
                                new Point(0, options.Height)
                            }
                        }
                    }
                };

                var geometry = new PathGeometry();
                geometry.Figures.Add(figure);

                result.Children.Add(geometry);
            }

            return result;
        }

        public async Task SaveGeometryAsSvgAsync(string filePath, Size size, Geometry geometry)
        {
            using var writer = new StreamWriter(filePath, false);

            var strokeWith = Math.Max(size.Width, size.Height) / 1000;
            await writer.WriteLineAsync("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
            await writer.WriteLineAsync("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">");
            await writer.WriteLineAsync($"<svg width=\"{size.Width}\" height=\"{size.Height}\">");
            await writer.WriteLineAsync($"    <g fill=\"none\" stroke=\"black\" stroke-width=\"{strokeWith.ToString(CultureInfo.InvariantCulture)}\">");

            var geometryGroup = (GeometryGroup)geometry;
            foreach (var pg in geometryGroup.Children.OfType<PathGeometry>())
            {
                await writer.WriteLineAsync($"        <path d=\"{pg.ToString(CultureInfo.InvariantCulture)}\"/>");
            }

            await writer.WriteLineAsync("    </g>");
            await writer.WriteLineAsync("</svg>");

            await writer.FlushAsync();
        }

        #region Private Functions
        private (IList<IList<Point>> RowsPoints, IList<IList<Point>> ColumnsPoints) GeneratePoints(PuzzleGeneratorOptions options)
        {
            var rowStepSize = options.Height / options.RowCount;
            var colStepSize = options.Width / options.ColumnCount;

            var rowLinePoints = new List<IList<Point>>();
            var columnLinePoints = new List<IList<Point>>();

            for (int i = 1; i < options.RowCount; i++)
            {
                var swap = !options.IsBumpRandomizationEnabled ? i % 2 == 0 : (bool?)null;
                var points = CreatePuzzleLinePoints(rowStepSize * i, options.ColumnCount, options.Width, rowStepSize, true, swap);
                rowLinePoints.Add(MakeCurvePoints(points, options.Tension));
            }

            for (int i = 1; i < options.ColumnCount; i++)
            {
                var swap = !options.IsBumpRandomizationEnabled ? i % 2 == 1 : (bool?)null;
                var points = CreatePuzzleLinePoints(colStepSize * i, options.RowCount, options.Height, colStepSize, false, swap);
                columnLinePoints.Add(MakeCurvePoints(points, options.Tension));
            }

            return (rowLinePoints, columnLinePoints);
        }

        private static Point[] CreatePuzzleLinePoints(double startOffset, int pieces, double length, double width, bool horizontal, bool? swap)
        {
            IEnumerable<Point> result = Array.Empty<Point>();

            var pieceLength = length / pieces;
            for (var i = 0; i < pieces; i++)
            {
                var s = swap.HasValue ? swap ^ i % 2 == 0 : null;
                var points = CreatePuzzlePiecePoints(i * pieceLength, startOffset, pieceLength, width, horizontal, s);
                result = result.Concat(points.Skip(i > 0 ? 1 : 0));
            }

            return result.ToArray();
        }

        private static IEnumerable<Point> CreatePuzzlePiecePoints(double startX, double startY, double xLength, double yLength, bool horizontal, bool? swap)
        {
            var realSwap = swap ?? Random.Shared.Next(0, 2) == 1;
            var nX = realSwap && !horizontal;
            var nY = realSwap && horizontal;
            return new[]
            {
                CreatePoint(0, 0),
                CreateRandomPoint(0.35, -0.05, 0.05, 0.02),
                CreateRandomPoint(0.3, 0.15, 0.05, 0.04),
                CreateRandomPoint(0.5, 0.25, 0.06, 0.06),
                CreateRandomPoint(0.7, 0.15, 0.05, 0.04),
                CreateRandomPoint(0.65, -0.05, 0.05, 0.02),
                CreatePoint(xLength, 0),
            };

            Point CreateRandomPoint(double x, double y, double maxXVariation, double maxYVariation)
            {
                var realX = (x + (Random.Shared.NextDouble() - 0.5) * 2 * maxXVariation) * xLength;
                var realY = (y + (Random.Shared.NextDouble() - 0.5) * 2 * maxYVariation) * yLength;
                return CreatePoint(realX, realY);
            }
            Point CreatePoint(double x, double y)
            {
                var realX = (horizontal ? startX : startY) + (horizontal ? x : y) * (nX ? -1 : 1);
                var realY = (horizontal ? startY : startX) + (horizontal ? y : x) * (nY ? -1 : 1);
                return new Point(realX, realY);
            }
        }

        private static Geometry MakeBezierPath(IList<Point> points)
        {
            var pathFigure = new PathFigure
            {
                StartPoint = points[0],
                Segments = new PathSegmentCollection
                {
                    new PolyBezierSegment
                    {
                        Points = new PointCollection(points.Count - 1) { points.Skip(1) }
                    }
                }
            };

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }


        private static IList<Point> MakeCurvePoints(IList<Point> points, double tension)
        {
            if (points.Count < 2) return null;
            var controlScale = tension / 0.5 * 0.175;

            var resultPoints = new List<Point>
            {
                points[0],
            };

            for (var i = 0; i < points.Count - 1; i++)
            {
                var ptBefore = points[Math.Max(i - 1, 0)];
                var pt = points[i];
                var ptAfter = points[i + 1];
                var ptAfter2 = points[Math.Min(i + 2, points.Count - 1)];

                var p4 = ptAfter;

                var dx = ptAfter.X - ptBefore.X;
                var dy = ptAfter.Y - ptBefore.Y;
                var p2 = new Point(
                    pt.X + controlScale * dx,
                    pt.Y + controlScale * dy);

                dx = ptAfter2.X - pt.X;
                dy = ptAfter2.Y - pt.Y;
                var p3 = new Point(
                    ptAfter.X - controlScale * dx,
                    ptAfter.Y - controlScale * dy);

                resultPoints.Add(p2);
                resultPoints.Add(p3);
                resultPoints.Add(p4);
            }

            return resultPoints.ToArray();
        }
        #endregion
    }
}
