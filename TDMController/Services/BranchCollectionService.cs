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
            string filePath = System.IO.Path.GetFullPath(Environment.CurrentDirectory + @"\UserProjects\Project2.json");
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new BranchListJsonConverter());
            options.WriteIndented = true;

            var branchList = new List<Branch>();

            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                Console.WriteLine(json);
                branchList = JsonSerializer.Deserialize<List<Branch>>(json, options);
            }
            if (branchList is not null)
            {
                BranchList = new ObservableCollection<Branch>(branchList);
            }
        }
    }
}
