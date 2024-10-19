using TDMController.Models;
using System.Collections.ObjectModel;
using System.IO.Ports;
using TDMController.Models.TDMDevices;

namespace TDMController.Services
{
    public interface IBranchCollectionService
    {
        ObservableCollection<Branch> BranchList { get; }

        void LoadCollectionFromFile(string path);
    }


    public class BranchCollectionService : IBranchCollectionService
    {
        public ObservableCollection<Branch> BranchList { get; private set; } = [];

        public void LoadCollectionFromFile(string path)
        {
            BranchList.Clear();
            BranchList = [
                new Branch("COM6", 9600, 1, new RotationDevice(1), null),
                new Branch("COM7", 9600, 2, null, new PODLDevice()),
            ];
        }
    }
}
