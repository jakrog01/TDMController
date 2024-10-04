using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using Material.Icons;
using TDMController.ViewModels.TDMViewModels;
using TDMController.Models;
using TDMController.Models.TDMDevices;
using System.IO.Ports;

namespace TDMController.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        [ObservableProperty]
        private bool _isPaneOpen = false;

        [ObservableProperty]
        private ViewModelBase _currentPage = new TDMPageViewModel();

        [ObservableProperty]
        private PageListItem? _selectedMenuItem;

        partial void OnSelectedMenuItemChanged(PageListItem? value)
        {
            if (value is null) return;
            var instance = Activator.CreateInstance(value.ModelType);
            if (instance is null) return;
            CurrentPage = (ViewModelBase)instance;
        }

        public ObservableCollection<PageListItem> MenuItems { get; } =
        [
            new PageListItem(typeof(TDMPageViewModel), MaterialIconKind.Hub),
            new PageListItem(typeof(SeriesPageViewModel), MaterialIconKind.ClipboardList),
            new PageListItem(typeof(ProjectsPageViewModel), MaterialIconKind.FileDocumentMultipleOutline),
        ];

        public ObservableCollection<Branch> Branches { get; set; } =
            [
                new Branch("COM6", 9600, 1, new RotationDevice(1, new SerialPort("COM6", 9600)), null),
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
