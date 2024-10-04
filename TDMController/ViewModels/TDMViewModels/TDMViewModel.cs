using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System;
using System.Collections.ObjectModel;

namespace TDMController.ViewModels.TDMViewModels
{
    public partial class TDMViewModel : ViewModelBase
    {

        [ObservableProperty]
        private String? _measuredPower= "0W";

        public ObservableCollection<TDMActionButton> TDMActionButtons { get; } = new()
        {
            new TDMActionButton("Measure",  MaterialIconKind.Finance),
            new TDMActionButton("Photo",  MaterialIconKind.Camera),
            new TDMActionButton("Reset",  MaterialIconKind.Restart),
        };
    }
}


