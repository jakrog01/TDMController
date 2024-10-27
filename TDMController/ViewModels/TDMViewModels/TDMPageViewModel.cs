using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TDMController.Models;
using TDMController.Services;

namespace TDMController.ViewModels.TDMViewModels
{
    public partial class TDMPageViewModel : ViewModelBase
    {
        private readonly IProjectService _projectService;
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

            TDMActionButtons = new ObservableCollection<TDMActionButton>
            {
                new TDMActionButton("Measure", MaterialIconKind.Finance, new RelayCommand(MeasureBranchAction)),
                new TDMActionButton("Photo", MaterialIconKind.Camera,  new RelayCommand(PhotoBranchAction)),
                new TDMActionButton("Reset", MaterialIconKind.Restart,  new RelayCommand(ResetBranchesAction)),
            };
        }

        public ObservableCollection<BranchItem> Branches { get; private set; }
        public Branch PhotoBranch { get; private set; }

        public Branch MeasureBranch { get; private set; }

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
        }

    }

    public class BranchItem(Branch branch)
    {
        public ViewModelBase ViewModel { get; init; } = new MotorsControllerCollapsedViewModel(branch);
    }
}
