using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.Application.Interfaces;
using PBManager.Core.Interfaces;
using PBManager.UI.MVVM.View;
using System.Collections.ObjectModel;
using System.Windows;

namespace PBManager.UI.MVVM.ViewModel;

public partial class ManagementViewModel<T>(IManagementService<T> service) : ObservableObject where T : class, IManagedEntity
{
    private readonly IManagementService<T> _service = service;

    [ObservableProperty] private string? _windowTitle;
    [ObservableProperty] private T? _selectedItem;
    public ObservableCollection<T> Items { get; } = [];

    public async Task LoadAsync(string title)
    {
        WindowTitle = title;
        var items = await _service.GetAllAsync();
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        var inputDialog = new InputDialog("اضافه کردن آیتم", "نام:");
        if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.Answer))
        {
            var newItem = await _service.AddAsync(inputDialog.Answer);
            Items.Add(newItem);
        }
    }
    
    [RelayCommand]
    private async Task EditItemAsync()
    {
        if (SelectedItem == null) return;
        var inputDialog = new InputDialog( "ویرایش آیتم", "نام:", SelectedItem.Name);
        if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.Answer))
        {
            SelectedItem.Name = inputDialog.Answer;
            await _service.UpdateAsync(SelectedItem);
            var index = Items.IndexOf(SelectedItem);
            Items[index] = SelectedItem;
        }
    }

    [RelayCommand]
    private async Task DeleteItemAsync()
    {
        if (SelectedItem == null) return;
        if (MessageBox.Show($"آیا از حذف '{SelectedItem.Name}' مطمئن هستید؟", "Confirm Delete",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            await _service.DeleteAsync(SelectedItem.Id);
            Items.Remove(SelectedItem);
        }
    }
}