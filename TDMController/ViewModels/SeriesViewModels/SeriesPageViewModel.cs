using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using System;
using System.Collections.ObjectModel;
using TDMController.Models;
using TDMController.Services;

namespace TDMController.ViewModels
{
    internal class SeriesPageViewModel : ViewModelBase
    {
        private readonly IProjectService _projectCollectionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILastProjectService _lastProjectService;

        public ObservableCollection<TDMActionButton> TDMActionButtons { get; private set; }
        public ObservableCollection<string> Headers { get; private set; } = ["Repetition"];

        public SeriesPageViewModel(IProjectService projectCollectionService, IServiceProvider serviceProvider, ILastProjectService lastProjectService)
        {
            _projectCollectionService = projectCollectionService;
            _serviceProvider = serviceProvider;
            _lastProjectService = lastProjectService;

            TDMActionButtons = new ObservableCollection<TDMActionButton>
            {
                new ("From File", MaterialIconKind.FileDocument, new RelayCommand(OpenProject)),
                new ("Save&Run", MaterialIconKind.AnimationPlay,  new RelayCommand(CreateNewProject)),
            };

            getHeadersFromKey(projectCollectionService.Key);
        }
        async void OpenProject()
        {
            var topLevel = GetTopLevel();
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Serie",
                AllowMultiple = false,
            });

            if (files.Count >= 1)
            {
                var filePath = files[0].Path.ToString();
                try
                {
                    _projectCollectionService.LoadCollectionFromFile(filePath);
                    _lastProjectService.SaveNewPath(filePath);
                }
                catch (Exception ex)
                {

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

        private void getHeadersFromKey(string key)
        {
            var header = "";
            foreach (char c in key)
            {
                if (c == 'R' || c == 'P')
                {
                    if (!string.IsNullOrEmpty(header))
                    {
                        Headers.Add(header);
                    }

                    header = (c == 'R') ? "Rotation " : "Position ";
                }
                else
                {
                    header += c;
                }
            }

            if (!string.IsNullOrEmpty(header))
            {
                Headers.Add(header);
            }
        }
    }
}
