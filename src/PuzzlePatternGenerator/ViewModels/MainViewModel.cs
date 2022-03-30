using MaSch.Core.Attributes;
using MaSch.Core.Observable;
using MaSch.Presentation.Wpf.Commands;
using Microsoft.Win32;
using PuzzlePatternGenerator.Models;
using PuzzlePatternGenerator.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PuzzlePatternGenerator.ViewModels
{
    [ObservablePropertyDefinition]
    internal interface IMainViewModel_Props
    {
        PuzzleGeneratorOptions GeneratorOptions { get; set; }
        Geometry CurrentPuzzlePattern { get; set; }
    }

    public partial class MainViewModel : ObservableObject, IMainViewModel_Props
    {
        private readonly IPuzzleGenerator _puzzleGenerator;
        private Size _currentPuzzlePatternSize;

        public ICommand GeneratePuzzlePatternCommand { get; }
        [DependsOn(nameof(CurrentPuzzlePattern))]
        public ICommand SavePuzzlePatternCommand { get; }

        public MainViewModel(IPuzzleGenerator puzzleGenerator)
        {
            _puzzleGenerator = puzzleGenerator;
            _generatorOptions = new PuzzleGeneratorOptions();

            GeneratePuzzlePatternCommand = new DelegateCommand(ExecuteGeneratePuzzlePattern);
            SavePuzzlePatternCommand = new DelegateCommand(() => CurrentPuzzlePattern != null, ExecuteSavePuzzlePattern);
        }

        #region Command Handlers
        private void ExecuteGeneratePuzzlePattern()
        {
            CurrentPuzzlePattern = _puzzleGenerator.GenerateGeometry(GeneratorOptions);
            _currentPuzzlePatternSize = new Size(GeneratorOptions.Width, GeneratorOptions.Height);
        }

        private void ExecuteSavePuzzlePattern()
        {
            var sfd = new SaveFileDialog { Filter = "SVG-File|*.svg" };
            if (sfd.ShowDialog() == true)
            {
                _puzzleGenerator.SaveGeometryAsSvg(sfd.FileName, _currentPuzzlePatternSize, CurrentPuzzlePattern);
            }
        }
        #endregion
    }
}
