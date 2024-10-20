using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMController.Models
{
    internal class Project
    {
        public Project(List<Branch> branches, Branch photoBranch, Branch measureBranch )
        {
            Branches = new ObservableCollection<Branch>(branches);
            PhotoBranch = photoBranch;
            MeasureBranch = measureBranch;
        }

        public ObservableCollection<Branch> Branches { get; set; } = [];

        public Branch? PhotoBranch { get; set; }

        public Branch? MeasureBranch { get; set; }

    }
}
