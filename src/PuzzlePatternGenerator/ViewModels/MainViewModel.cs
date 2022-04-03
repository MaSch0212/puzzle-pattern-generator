using MaSch.Core.Attributes;
using MaSch.Core.Observable;
using MaSch.Presentation;
using MaSch.Presentation.Services;
using MaSch.Presentation.Wpf.Commands;
using Microsoft.Win32;
using PuzzlePatternGenerator.Models;
using PuzzlePatternGenerator.Services;
using System.Threading.Tasks;
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
        bool IsLoading { get; set; }
    }

    public partial class MainViewModel : ObservableObject, IMainViewModel_Props
    {
        private readonly IPuzzleGenerator _puzzleGenerator;
        private readonly IMessageBoxService _messageBox;
        private Size _currentPuzzlePatternSize;

        [DependsOn(nameof(IsLoading))]
        public ICommand GeneratePuzzlePatternCommand { get; }
        [DependsOn(nameof(IsLoading), nameof(CurrentPuzzlePattern))]
        public ICommand SavePuzzlePatternCommand { get; }
        [DependsOn(nameof(IsLoading))]
        public ICommand GenerateAndSavePuzzlePatternCommand { get; }

        public MainViewModel(IPuzzleGenerator puzzleGenerator, IMessageBoxService messageBox)
        {
            _puzzleGenerator = puzzleGenerator;
            _messageBox = messageBox;
            _generatorOptions = new PuzzleGeneratorOptions();

            GeneratePuzzlePatternCommand = new AsyncDelegateCommand(() => !IsLoading, ExecuteGeneratePuzzlePattern);
            SavePuzzlePatternCommand = new AsyncDelegateCommand(() => !IsLoading && CurrentPuzzlePattern != null, ExecuteSavePuzzlePattern);
            GenerateAndSavePuzzlePatternCommand = new AsyncDelegateCommand(() => !IsLoading, ExecuteGenerateAndSavePuzzlePatternCommand);
        }

        #region Command Handlers
        private async Task ExecuteGeneratePuzzlePattern()
        {

            try
            {
                if (GeneratorOptions.ColumnCount * GeneratorOptions.RowCount > 5000)
                {
                    var msgResult = _messageBox.Show("WARNING: Displaying more than 5000 puzzle pieces will lag the software. It is recommended to use the \"Generate and Save\" button instead to directly save such a large geometry to disk.\n\nDo you want to continue?", null, AlertButton.YesNo, AlertImage.Warning);
                    if (msgResult == AlertResult.No)
                        return;
                }

                IsLoading = true;
                CurrentPuzzlePattern = await _puzzleGenerator.GenerateGeometryAsync(GeneratorOptions);
                _currentPuzzlePatternSize = new Size(GeneratorOptions.Width, GeneratorOptions.Height);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteSavePuzzlePattern()
        {
            try
            {
                var sfd = new SaveFileDialog { Filter = "SVG-File|*.svg" };
                if (sfd.ShowDialog() == true)
                {
                    IsLoading = true;
                    await _puzzleGenerator.SaveGeometryAsSvgAsync(sfd.FileName, _currentPuzzlePatternSize, CurrentPuzzlePattern);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteGenerateAndSavePuzzlePatternCommand()
        {
            try
            {
                var sfd = new SaveFileDialog { Filter = "SVG-File|*.svg" };
                if (sfd.ShowDialog() != true)
                    return;

                IsLoading = true;
                var puzzlePattern = await _puzzleGenerator.GenerateGeometryAsync(GeneratorOptions);
                await _puzzleGenerator.SaveGeometryAsSvgAsync(sfd.FileName, new Size(GeneratorOptions.Width, GeneratorOptions.Height), puzzlePattern);
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
    }
}
