using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMController.Models;
using TDMController.Services;

namespace TDMController.ViewModels.TDMViewModels
{
    public partial class TDMPageViewModel : ViewModelBase
    {

        public TDMPageViewModel(IBranchCollectionService branchCollectionService)
        {
            var branchCollection = branchCollectionService.BranchList;
            var branches = new List<BranchItem>();

            for (var i = 0; i < branchCollection.Count; i++)
            {
                var branchItem = new BranchItem(branchCollection[i]);
                branches.Add(branchItem);
            }

            Branches = new ObservableCollection<BranchItem>(branches);
        }

        public ObservableCollection<BranchItem> Branches { get; private set; }

        [ObservableProperty]
        private String? _measuredPower = "ND";

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

    public class BranchItem(Branch branch)
    {
        public Branch Branch { get; } = branch;
        public String Label { get; } = $"Branch {branch.BranchIndex}";
    }
}
