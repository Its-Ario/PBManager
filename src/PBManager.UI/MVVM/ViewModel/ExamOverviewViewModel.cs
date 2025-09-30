using CommunityToolkit.Mvvm.ComponentModel;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class ExamOverviewViewModel : ObservableObject
    {
        private readonly IExamService _examService;

        [ObservableProperty]
        private Exam? _exam;

        [ObservableProperty]
        private int _participants;

        public ExamOverviewViewModel(IExamService examService)
        {
            _examService = examService;
        }

        public async Task InitializeAsync(Exam exam)
        {
            if (exam == null) return;
            Exam = exam;
            Participants = await _examService.GetParticipantCountAsync(exam.Id);
        }
    }
}
