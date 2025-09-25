using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.Core.Entities;

namespace PBManager.UI.MVVM.ViewModel;

[ObservableObject]
public partial class StudentDetailViewModel
{
    [ObservableProperty]
    private Student _student;

    [ObservableProperty]
    private object? _currentSubViewModel;

    public StudyOverviewViewModel StudyOverviewVM { get; }
    public GradeOverviewViewModel GradeOverviewVM { get; }

    public StudentDetailViewModel(
        StudyOverviewViewModel studyOverviewViewModel,
        GradeOverviewViewModel gradeOverviewViewModel)
    {
        StudyOverviewVM = studyOverviewViewModel;
        GradeOverviewVM = gradeOverviewViewModel;
    }

    public async Task InitializeAsync(Student student)
    {
        Student = student;
        await StudyOverviewVM.InitializeAsync(student);
        await GradeOverviewVM.InitializeAsync(student);

        CurrentSubViewModel = StudyOverviewVM;
    }

    [RelayCommand]
    private void ShowStudyOverview() => CurrentSubViewModel = StudyOverviewVM;

    [RelayCommand]
    private void ShowGrades() => CurrentSubViewModel = GradeOverviewVM;
}