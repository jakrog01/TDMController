using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using TDMController.Models;
using TDMController.Services;

namespace TDMController.ViewModels
{
    internal class ProjectsPageViewModel: ViewModelBase
    {
        private readonly IProjectService _projectCollectionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILastProjectService _lastProjectService;

        public ObservableCollection<TDMActionButton> TDMActionButtons { get; private set; }

        public ProjectsPageViewModel(IProjectService branchCollectionService, IServiceProvider serviceProvider, ILastProjectService lastProjectService)
        {
            _projectCollectionService = branchCollectionService;
            _serviceProvider = serviceProvider;
            _lastProjectService = lastProjectService;

            TDMActionButtons = new ObservableCollection<TDMActionButton>
            {
                new TDMActionButton("Open", MaterialIconKind.FileDocument, new RelayCommand(OpenProject)),
                new TDMActionButton("New", MaterialIconKind.FileDocumentAdd,  new RelayCommand(CreateNewProject)),
            };
        }
        async void OpenProject()
        {
            var topLevel = GetTopLevel();
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Project",
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
    }
}
