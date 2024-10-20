using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using Material.Icons;
using TDMController.ViewModels.TDMViewModels;
using TDMController.Models;
using TDMController.Models.TDMDevices;
using System.IO.Ports;
using TDMController.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TDMController.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        private readonly IProjectService _branchCollectionService;
        private readonly IServiceProvider _serviceProvider;

        public MainWindowViewModel(IProjectService branchCollectionService, IServiceProvider serviceProvider)
        {
            _branchCollectionService = branchCollectionService;
            _serviceProvider = serviceProvider;
            _branchCollectionService.LoadCollectionFromFile("ddd");

            _currentPage = (ViewModelBase)_serviceProvider.GetRequiredService(typeof(TDMPageViewModel));
        }

        public ObservableCollection<Branch> BranchCollection => _branchCollectionService.BranchList;
        
        [ObservableProperty]
        private bool _isPaneOpen = false;

        [ObservableProperty]
        private ViewModelBase _currentPage;

        [ObservableProperty]
        private PageListItem? _selectedMenuItem;

        partial void OnSelectedMenuItemChanged(PageListItem? value)
        {
            if (value is null) return;
            var instance = _serviceProvider.GetRequiredService(value.ModelType);
            if (instance is null) return;
            CurrentPage = (ViewModelBase)instance;
        }

        public ObservableCollection<PageListItem> MenuItems { get; } =
        [
            new PageListItem(typeof(TDMPageViewModel), MaterialIconKind.Hub),
            new PageListItem(typeof(SeriesPageViewModel), MaterialIconKind.ClipboardList),
            new PageListItem(typeof(ProjectsPageViewModel), MaterialIconKind.FileDocumentMultipleOutline),
        ];

        [RelayCommand]
        private void TriggerPane()
        {
            IsPaneOpen = !IsPaneOpen;
        }
    }

    public class PageListItem
    {
        public PageListItem(Type type, MaterialIconKind icon)
        {
            ModelType = type;
            Label = type.Name.Replace("PageViewModel", "");
            Icon = icon;
        }
        public string Label { get; }
        public Type ModelType { get; }
        public MaterialIconKind Icon { get; }
    }
}
