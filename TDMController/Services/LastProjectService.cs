using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TDMController.Models;
using TDMController.Serializers;

namespace TDMController.Services
{

    internal class ProjectInfo
    {
        public string Path { get; set; }
    }

    public interface ILastProjectService
    {
        public string? LastProject { get; set; }
        public void SaveNewPath(string path);

        public void LoadPathFromFile();
    }


    public class LastProjectService : ILastProjectService
    {
        public string LastProjectFilePath = System.IO.Path.GetFullPath(Environment.CurrentDirectory + @"\Settings\LastProject.json");
        public string? LastProject { get; set; }

        public void SaveNewPath(string path)
        {
            Uri uri = new Uri(path);
            string filePath = uri.LocalPath;

            var projectInfo = new ProjectInfo { Path = filePath};
            LastProject = filePath;

            string json = JsonSerializer.Serialize(projectInfo);
            File.WriteAllText(LastProjectFilePath, json);
        }

        public void LoadPathFromFile()
        {
            if (!File.Exists(LastProjectFilePath))
            {
                return;
            }

            string json = File.ReadAllText(LastProjectFilePath);
            try
            {
                LastProject = JsonSerializer.Deserialize<ProjectInfo>(json).Path;
            }
            catch (JsonException)
            {
                return;
            }
        }
    }
}
