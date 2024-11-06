using System.Collections.Generic;
using System.Collections.ObjectModel;
using TDMController.Models.TDMDevices;

namespace TDMController.Models
{
    internal class Project
    {
        public Project(List<Branch> branches, Branch photoBranch, Branch measureBranch, TLPowerMeter powerMeter)
        {
            Branches = new ObservableCollection<Branch>(branches);
            PhotoBranch = photoBranch;
            MeasureBranch = measureBranch;
            ProjectKey = GetKey(Branches);
            PowerMeter = powerMeter;
        }

        public ObservableCollection<Branch> Branches { get; set; } = [];
        public Branch? PhotoBranch { get; set; }
        public Branch? MeasureBranch { get; set; }

        public TLPowerMeter? PowerMeter { get; set; }

        public string ProjectKey { get; set; }

        public string GetKey(ObservableCollection<Branch> branches)
        {
            string key = "";

            foreach (Branch branch in branches)
            {
                if (branch.RotationDevice is not null)
                {
                    key += ("R" + (branch.BranchIndex).ToString());
                }

                if (branch.PositionDevice is not null)
                {
                    key += ("P" + (branch.BranchIndex).ToString());
                }
            }
            return key;
        }

    }
}
