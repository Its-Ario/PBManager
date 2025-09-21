using Microsoft.Extensions.DependencyInjection;
using PBManager.Application.Interfaces;
using PBManager.Core.Interfaces;
using PBManager.UI.MVVM.ViewModel;
using PBManager.UI.MVVM.View;

namespace PBManager.UI.Services;

public class DialogService(IServiceProvider serviceProvider) : IDialogService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void ShowManagementWindow<T>(string title) where T : class, IManagedEntity
    {
        var service = _serviceProvider.GetRequiredService<IManagementService<T>>();
        var viewModel = new ManagementViewModel<T>(service);
        _ = viewModel.LoadAsync(title);

        var view = new ManagementView { DataContext = viewModel };
        view.Show();
    }
}