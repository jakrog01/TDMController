using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TDMController.ViewModels.TDMViewModels;

namespace TDMController.Views.TDMViews;

public partial class TDMExpandedView : UserControl
{
    public TDMExpandedView()
    {
        InitializeComponent();
        DataContext = App.Services?.GetRequiredService<TDMPageViewModel>();
    }
}