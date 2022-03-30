using Microsoft.Extensions.DependencyInjection;
using PuzzlePatternGenerator.Services;
using PuzzlePatternGenerator.ViewModels;
using PuzzlePatternGenerator.Views;
using System;
using System.Windows;

namespace PuzzlePatternGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var serviceProvider = GetServiceProvider();

            using var serviceScope = serviceProvider.CreateScope();
            MainWindow = serviceScope.ServiceProvider.GetService<MainView>();
            MainWindow.Show();
        }

        private IServiceProvider GetServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IPuzzleGenerator, PuzzleGenerator>();

            services.AddScoped<MainViewModel>();
            services.AddScoped<MainView>();

            return services.BuildServiceProvider();
        }
    }
}
