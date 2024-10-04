using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TDMController.ViewModels;
using TDMController.ViewModels.TDMViewModels;
using TDMController.Views;
using TDMController.Services;
using System;

namespace TDMController
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);

                var serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton<IBranchCollectionService, BranchCollectionService>();
                serviceCollection.AddTransient<MainWindowViewModel>();
                serviceCollection.AddTransient<TDMPageViewModel>();
                serviceCollection.AddTransient<SeriesPageViewModel>();
                serviceCollection.AddTransient<ProjectsPageViewModel>();
                serviceCollection.AddSingleton<IServiceProvider>(sp => sp);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}