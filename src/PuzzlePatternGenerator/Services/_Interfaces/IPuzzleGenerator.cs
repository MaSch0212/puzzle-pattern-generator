using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using PuzzlePatternGenerator.Models;

namespace PuzzlePatternGenerator.Services
{
    public interface IPuzzleGenerator
    {
        Task<Geometry> GenerateGeometryAsync(PuzzleGeneratorOptions options);

        Task SaveGeometryAsSvgAsync(string filePath, Size size, Geometry geometry);
    }
}
