using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMController.Services;
using TDMController.ViewModels;

namespace TDMController.Models.Factories
{
    internal class RunningSeriesPageViewModelFactory
    {
        private readonly IProjectService _projectService;
        private readonly IServiceProvider _serviceProvider;

        public RunningSeriesPageViewModelFactory(IProjectService projectService, IServiceProvider serviceProvider)
        {
            _projectService = projectService;
            _serviceProvider = serviceProvider;
        }

        public RunningSeriesPageViewModel CreateWithSequence (Series series)
        {
            return new RunningSeriesPageViewModel(_projectService, _serviceProvider, series);
        }
    }
}
