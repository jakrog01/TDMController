using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using TDMController.Models;
using TDMController.Models.Factories;
using TDMController.Services;
using TDMController.ViewModels.TDMViewModels;

namespace TDMController.ViewModels
{
    internal partial class NewSeriesPageViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        public Action<ViewModelBase> ChangePageAction { get; set; }

        public ObservableCollection<TDMActionButton>? TDMActionButtons { get; private set; }

        public NewSeriesPageViewModel( IServiceProvider serviceProvider, ILastProjectService lastProjectService)
        {
            _serviceProvider = serviceProvider;

            TDMActionButtons = new ObservableCollection<TDMActionButton>
            {
                new ("From File", MaterialIconKind.FileDocument, new RelayCommand(OpenProject)),
                new ("Save&Run", MaterialIconKind.AnimationPlay,  new RelayCommand(CreateNewProject)),
            };
        }
        async void OpenProject()
        {
            var topLevel = GetTopLevel();
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Series",
                AllowMultiple = false,
            });

            if (files.Count >= 1)
            {
                var path = files[0].Path.ToString();
                Uri uri = new Uri(path);
                string filePath = uri.LocalPath;

                var jsonString = await File.ReadAllTextAsync(filePath);
                Series? seriesObject = JsonSerializer.Deserialize<Series>(jsonString);

                if (seriesObject != null)
                {
                    ViewModelBase anotherPageViewModel;
                    var runningPageFactory = _serviceProvider.GetService(typeof(RunningSeriesPageViewModelFactory));
                    if (runningPageFactory is RunningSeriesPageViewModelFactory factory)
                    {
                        anotherPageViewModel = factory.CreateWithSequence(seriesObject);
                        ChangePageAction?.Invoke(anotherPageViewModel);
                    }
                }
            }

        }

        void CreateNewProject()
        {
            return;
        }

        private WindowBase GetTopLevel()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return desktopLifetime.MainWindow;
            }

            return null;
        }
    }
}
