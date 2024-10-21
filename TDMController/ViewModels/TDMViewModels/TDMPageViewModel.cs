using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using TDMController.Models;
using TDMController.Services;

namespace TDMController.ViewModels.TDMViewModels
{
    public partial class TDMPageViewModel : ViewModelBase
    {

        public TDMPageViewModel(IProjectService projectCollectionService)
        {
            var branchCollection = projectCollectionService.BranchList;
            var branches = new List<BranchItem>();

            for (var i = 0; i < branchCollection.Count; i++)
            {
                var branchItem = new BranchItem(branchCollection[i]);
                branches.Add(branchItem);
            }

            Branches = new ObservableCollection<BranchItem>(branches);
            PhotoBranch = projectCollectionService.PhotoBranch;
            MeasureBranch = projectCollectionService.MeasureBranch;

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
            MeasureBranch?.SendExternalDeviceTrigger();
        }

        private void PhotoBranchAction()
        {
            PhotoBranch?.SendExternalDeviceTrigger();
        }

        private void ResetBranchesAction()
        {
            return;
        }

    }

    public class BranchItem(Branch branch)
    {
        public ViewModelBase ViewModel { get; init; } = new MotorsControllerCollapsedViewModel(branch);
    }
}
