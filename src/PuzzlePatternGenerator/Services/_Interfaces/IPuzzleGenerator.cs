using System.Windows;
using System.Windows.Media;
using PuzzlePatternGenerator.Models;

namespace PuzzlePatternGenerator.Services
{
    public interface IPuzzleGenerator
    {
        Geometry GenerateGeometry(PuzzleGeneratorOptions options);

        void SaveGeometryAsSvg(string filePath, Size size, Geometry geometry);
    }
}
