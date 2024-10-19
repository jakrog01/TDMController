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
        }

        [ObservableProperty]
        private String? _branchLabel;

        [ObservableProperty]
        private MaterialIconKind _iconForward = MaterialIconKind.ChevronRight;

        [ObservableProperty]
        private MaterialIconKind _iconBackward= MaterialIconKind.ChevronLeft;

        [ObservableProperty]
        private double _rotationMoveValue = 0;

        [ObservableProperty]
        private double _positionMoveValue = 0;

        public void MoveRotationDeviceForeward()
        {
            var value = Convert.ToInt32(RotationMoveValue);
            Branch.MoveRotationDevice(value);
        }

        public void MoveRotationDeviceBackward()
        {
            var value = - Convert.ToInt32(RotationMoveValue);
            Branch.MoveRotationDevice(value);
        }

        public void MovePositionDeviceForeward()
        {
            var value = Convert.ToInt32(PositionMoveValue);
            Branch.MovePositionDevice(value);
        }

        public void MovePositionDeviceBackward()
        {
            var value = -Convert.ToInt32(PositionMoveValue);
            Branch.MovePositionDevice(value);
        }
    }
}
