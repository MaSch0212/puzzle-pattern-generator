using MaSch.Core.Attributes;
using MaSch.Core.Observable;

namespace PuzzlePatternGenerator.Models
{
    [ObservablePropertyDefinition]
    internal interface IPuzzleGeneratorOptions_Props
    {
        double Width { get; set; }
        double Height { get; set; }
        int ColumnCount { get; set; }
        int RowCount { get; set; }
        double Tension { get; set; }
        bool IsBorderEnabled { get; set; }
        bool IsBumpRandomizationEnabled { get; set; }
    }

    public partial class PuzzleGeneratorOptions : ObservableObject, IPuzzleGeneratorOptions_Props
    {
        public PuzzleGeneratorOptions()
        {
            _width = 100;
            _height = 100;
            _columnCount = 10;
            _rowCount = 10;
            _tension = 0.64;
            _isBorderEnabled = true;
            _isBumpRandomizationEnabled = true;
        }
    }
}
