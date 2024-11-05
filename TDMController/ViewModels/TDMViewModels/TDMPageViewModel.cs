using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;
using TDMController.Models;
using TDMController.Models.TDMDevices;
using TDMController.Services;

namespace TDMController.ViewModels.TDMViewModels
{
    public partial class TDMPageViewModel : ViewModelBase, IDisposable
    {
        private readonly IProjectService _projectService;
        private Timer? _timer;
        private int ndCounter;

        public TDMPageViewModel(IProjectService projectService)
        {
            _projectService = projectService;
            var branchCollection = projectService.BranchList;
            var branches = new List<BranchItem>();

            for (var i = 0; i < branchCollection.Count; i++)
            {
                var branchItem = new BranchItem(branchCollection[i]);
                branches.Add(branchItem);
            }

            Branches = new ObservableCollection<BranchItem>(branches);
            PhotoBranch = projectService.PhotoBranch;
            MeasureBranch = projectService.MeasureBranch;

            TDMActionButtons =
            [
                new ("Measure", MaterialIconKind.Finance, Branches.Count == 0 ? null: new RelayCommand(MeasureBranchAction)),
                new ("Photo", MaterialIconKind.Camera, Branches.Count == 0 ?  null : new RelayCommand(PhotoBranchAction)),
                new ("Reset", MaterialIconKind.Restart, Branches.Count == 0 ?  null : new RelayCommand(ResetBranchesAction)),
            ];

            Task.Run(StartPowerMeasurement);
        }

        public ObservableCollection<BranchItem> Branches { get; private set; }
        public Branch PhotoBranch { get; private set; }

        public Branch MeasureBranch { get; private set; }

        private TLPowerMeter _powerMeter;

        [ObservableProperty]
        private String? _measuredPower = "ND";

        public ObservableCollection<TDMActionButton> TDMActionButtons { get; private set; }

        private void MeasureBranchAction()
        {
            Task.Run(() => MeasureBranch?.SendExternalDeviceTrigger());
        }

        private void PhotoBranchAction()
        {
            Task.Run(() => PhotoBranch?.SendExternalDeviceTrigger());
        }

        private void ResetBranchesAction()
        {
            foreach (Branch branch in _projectService.BranchList)
            {
                Task.Run(() => branch.HomeDevices());
            }

            ndCounter = 0;
            Task.Run(StartPowerMeasurement);
        }

        public Task StartPowerMeasurement()
        {
            _powerMeter = new TLPowerMeter("USB0::0x1313::0x8078::P0028387::INSTR");
            _timer = new Timer(MeasurePower, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
            return Task.CompletedTask;
        }

        public Task StopPowerMeasurement()
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void MeasurePower(Object? state)
        {
            if (_powerMeter is not null)
            {
                MeasuredPower = _powerMeter.MeasurePowerWithUnit();
            }

            if (MeasuredPower == "ND")
            {
                ndCounter++;
                if (ndCounter == 10)
                {
                    {
                        Task.Run(StopPowerMeasurement);
                    }
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

    }

    public class BranchItem(Branch branch)
    {
        public ViewModelBase ViewModel { get; init; } = new MotorsControllerCollapsedViewModel(branch);
    }
}
