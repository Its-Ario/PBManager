using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using System.Collections.ObjectModel;
using Mohsen;

namespace PBManager.UI.MVVM.ViewModel;

[ObservableObject]
public partial class HistoryViewModel
{
    private readonly IAuditLogRepository _auditRepository;
    private readonly IUserRepository _userRepository;

    private int _currentPage = 1;
    private const int PageSize = 50;

    public ObservableCollection<AuditLog> Logs { get; } = new();
    public ObservableCollection<string> AvailableEntityTypes { get; } = new();
    public ObservableCollection<User> AvailableUsers { get; } = new();

    [ObservableProperty] private string? _entityTypeFilter;
    [ObservableProperty] private int? _userFilter;
    [ObservableProperty] private bool _canLoadMore = true;

    public HistoryViewModel(IAuditLogRepository auditRepository, IUserRepository userRepository)
    {
        _auditRepository = auditRepository;
        _userRepository = userRepository;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadFilterDataAsync();
        await LoadHistoryCommand.ExecuteAsync(null);
    }

    private async Task LoadFilterDataAsync()
    {
        AvailableEntityTypes.Add("Student");
        AvailableEntityTypes.Add("Class");
        AvailableEntityTypes.Add("Subject");
        AvailableEntityTypes.Add("User");
        AvailableEntityTypes.Add("StudyRecord");

        var users = await _userRepository.GetAllAsync();
        AvailableUsers.Add(new User { Id = 0, Username = "All Users" });
        foreach (var user in users)
        {
            AvailableUsers.Add(user);
        }
    }

    partial void OnEntityTypeFilterChanged(string? value) => _ = LoadHistoryAsync();
    partial void OnUserFilterChanged(int? value) => _ = LoadHistoryAsync();

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        _currentPage = 1;
        Logs.Clear();
        CanLoadMore = true;
        await LoadMoreLogsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanLoadMore))]
    private async Task LoadMoreLogsAsync()
    {
        string? entityFilter = EntityTypeFilter == "All" ? null : EntityTypeFilter;
        int? userFilter = UserFilter == 0 ? null : UserFilter;

        var newLogs = await _auditRepository.GetAsync(entityFilter, userFilter, _currentPage, PageSize);

        foreach (var log in newLogs)
        {
            Logs.Add(log);
        }

        if (newLogs.Count < PageSize)
        {
            CanLoadMore = false;
        }

        _currentPage++;
        LoadMoreLogsCommand.NotifyCanExecuteChanged();
    }
}