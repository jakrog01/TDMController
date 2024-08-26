using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMController.ViewModels.TDMViewModels
{
    public partial class MotorsControllerCollapsedViewModel : ViewModelBase
    {
        [ObservableProperty]
        private String? _branchLabel = "BRANCH X";

        public ObservableCollection<MotorControllerItem> MotorControllers{ get; } = new()
        {
            new MotorControllerItem("ROTATION", MaterialIconKind.ChevronRight, MaterialIconKind.ChevronLeft),
            new MotorControllerItem("POSITION", MaterialIconKind.ChevronRight, MaterialIconKind.ChevronLeft)
        };
    }

    public class MotorControllerItem (string label, MaterialIconKind iconForward, MaterialIconKind iconBackward)
    {
        public string Label { get; } = label;
        public MaterialIconKind IconForward { get; } = iconForward;

        public MaterialIconKind IconBackward { get; } = iconBackward;
    }
}
