using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TDMController.ViewModels.TDMViewModels;

namespace TDMController.Views.TDMViews;

public partial class TDMFullyCollapsedView : UserControl
{
    public TDMFullyCollapsedView()
    {
        InitializeComponent();
        DataContext = App.Services?.GetRequiredService<TDMPageViewModel>();
    }
}