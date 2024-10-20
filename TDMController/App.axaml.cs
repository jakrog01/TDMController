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
using TDMController.Views.TDMViews;

namespace TDMController
{
    public partial class App : Application
    {

        public static IServiceProvider? Services { get; private set; }

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
                ConfigureServices(serviceCollection);
                Services = serviceCollection.BuildServiceProvider();
                var serviceProvider = serviceCollection.BuildServiceProvider();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = Services.GetRequiredService<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IProjectService, ProjectService>();

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<TDMPageViewModel>();
            services.AddTransient<SeriesPageViewModel>();
            services.AddTransient<ProjectsPageViewModel>();

            services.AddSingleton<IServiceProvider>(sp => sp);

        }

    }
}