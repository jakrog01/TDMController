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

        Branch PhotoBranch { get; }

        Branch MeasureBranch { get; }

        void LoadCollectionFromFile(string path);
    }


    public class ProjectService : IProjectService
    {
        public ObservableCollection<Branch> BranchList { get; private set; } = [];

        public Branch PhotoBranch { get; private set; } = null;

        public Branch MeasureBranch { get; private set; } = null;

        public void LoadCollectionFromFile(string path)
        {
            BranchList.Clear();
            string filePath = System.IO.Path.GetFullPath(Environment.CurrentDirectory + @"\UserProjects\Project2.json");
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
            }
        }
    }
}
