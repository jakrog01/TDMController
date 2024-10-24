using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using TDMController.ViewModels.TDMViewModels;

namespace TDMController.ViewModels
{
    internal partial class SeriesPageViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ViewModelBase _currentPage;

        public SeriesPageViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _currentPage = (ViewModelBase)_serviceProvider.GetRequiredService(typeof(NewSeriesPageViewModel));

            if (_currentPage is NewSeriesPageViewModel newSeriePageViewModel)
            {
                newSeriePageViewModel.ChangePageAction = ChangePage;
            }
        }

        private void ChangePage(ViewModelBase newPage)
        {
            CurrentPage = newPage;
        }
    }
}
