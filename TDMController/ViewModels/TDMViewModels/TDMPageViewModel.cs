using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMController.Models;

namespace TDMController.ViewModels.TDMViewModels
{
    public partial class TDMPageViewModel : ViewModelBase
    {

        [ObservableProperty]
        private String? _measuredPower = "0W";

        public ObservableCollection<TDMActionButton> TDMActionButtons { get; } = new()
        {
            new TDMActionButton("Measure",  MaterialIconKind.Finance),
            new TDMActionButton("Photo",  MaterialIconKind.Camera),
            new TDMActionButton("Reset",  MaterialIconKind.Restart),
        };
    }

    public class TDMActionButton(string label, MaterialIconKind icon)
    {
        public string Label { get; } = label;
        public MaterialIconKind Icon { get; } = icon;
    }
}
