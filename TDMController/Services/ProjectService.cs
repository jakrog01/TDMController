using TDMController.Models;
using System.Collections.ObjectModel;
using System.IO.Ports;
using TDMController.Models.TDMDevices;
using System;
using System.Text.Json;
using TDMController.Serializers;
using System.Collections.Generic;
using System.IO;

namespace TDMController.Services
{
    public interface IProjectService
    {
        ObservableCollection<Branch> BranchList { get; }
        
        string Key { get; }

        Branch PhotoBranch { get; }

        Branch MeasureBranch { get; }

        TLPowerMeter PowerMeter { get; }

        void LoadCollectionFromFile(string path);
    }


    public class ProjectService : IProjectService
    {
        public ObservableCollection<Branch> BranchList { get; private set; } = [];

        public Branch PhotoBranch { get; private set; } = null;

        public Branch MeasureBranch { get; private set; } = null;

        public TLPowerMeter PowerMeter { get; private set; } = null;

        public string Key { get; private set; } = null;

        public void LoadCollectionFromFile(string path)
        {
            foreach (Branch branch in BranchList) { 
            
                if (branch.SerialPort.IsOpen)
                {
                   branch.SerialPort.Close();
                }
            }

            BranchList.Clear();

            Uri uri = new Uri(path);
            string filePath = uri.LocalPath;
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new ProjectJsonConverter());
            options.WriteIndented = true;

            Project? project = null;

            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                Console.WriteLine(json);
                project = JsonSerializer.Deserialize<Project>(json, options);
            }
            if (project is not null)
            {
                BranchList = project.Branches;
                PhotoBranch = project.PhotoBranch;
                MeasureBranch = project.MeasureBranch;
                PowerMeter = project.PowerMeter;
                Key = project.GetKey(project.Branches);
            }
        }
    }
}
