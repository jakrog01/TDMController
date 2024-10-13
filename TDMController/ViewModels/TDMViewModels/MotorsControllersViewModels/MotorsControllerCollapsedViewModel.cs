using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public partial class MotorsControllerCollapsedViewModel : ViewModelBase
    {

        public Branch Branch { get;}

        public MotorsControllerCollapsedViewModel(Branch branch) { 
            Branch = branch;
            BranchLabel = $"BRANCH {branch.BranchIndex}";
            MotorControllers = new ObservableCollection<MotorControllerItem>
        {
            new("ROTATION", MaterialIconKind.ChevronRight, MaterialIconKind.ChevronLeft, () => Branch.MoveRotationDevice(180), () => Branch.MoveRotationDevice(-180)),
        };
        }

        [ObservableProperty]
        private String? _branchLabel;

        public ObservableCollection<MotorControllerItem> MotorControllers{ get; }
    }

    public class MotorControllerItem (string label, MaterialIconKind iconForward, MaterialIconKind iconBackward, Action moveForeward, Action moveBackward)
    {
        public string Label { get; } = label;
        public MaterialIconKind IconForward { get; } = iconForward;

        public MaterialIconKind IconBackward { get; } = iconBackward;

        public RelayCommand MoveForeward { get; } = new RelayCommand(moveForeward);

        public RelayCommand MoveBackward { get; } = new RelayCommand(moveBackward);
    }
}
