using PBManager.MVVM.Model;
using PBManager.Services;
using PBManager.Messages;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace PBManager.MVVM.ViewModel
{
    public class StudentDetailViewModel : ObservableObject, IRecipient<StudentSelectedMessage>
    {
        private StudyRecordService _studyRecordService;

        private Student? _student;
        public Student? Student
        {
            get => _student;
            private set => SetProperty(ref _student, value);
        }

        private double _avgWeeklyStudy;
        public double AvgWeeklyStudy {
            get => _avgWeeklyStudy;
            set => SetProperty(ref _avgWeeklyStudy, value);
        }

        private int _classRank;
        public int ClassRank
        {
            get => _classRank;
            set => SetProperty(ref _classRank, value);
        }

        private int _globalRank;
        public int GlobalRank
        {
            get => _globalRank;
            set => SetProperty(ref _globalRank, value);
        }

        public StudentDetailViewModel()
        {
            _studyRecordService = new();
            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(StudentSelectedMessage message)
        {
            Student = message.Value;
            _ = LoadAsync(Student.Id);
        }

        public async Task LoadAsync(int studentId)
        {
            AvgWeeklyStudy = await _studyRecordService.GetWeeklyAverageAsync(studentId);
            ClassRank = await _studyRecordService.GetClassRankAsync(studentId);
            GlobalRank = await _studyRecordService.GetGlobalRankAsync(studentId);
        }
    }
}
