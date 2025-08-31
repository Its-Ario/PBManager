using PBManager.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace PBManager.MVVM.ViewModel
{
    public class AddStudyRecordViewModel : ObservableObject
    {
        public ObservableCollection<SubjectEntry> SubjectEntries { get; set; }
        public IRelayCommand SubmitCommand { get; set; }
        private Student _student;

        public AddStudyRecordViewModel(Student student)
        {
            _student = student;
            SubjectEntries = new ObservableCollection<SubjectEntry>();
            LoadSubjects();

            SubmitCommand = new RelayCommand(async () => await SubmitAsync());
        }

        private void LoadSubjects()
        {
            var subjectsFromDb = App.Db.Subjects.ToList();
            foreach (var s in subjectsFromDb)
            {
                SubjectEntries.Add(new SubjectEntry { Subject = s });
            }
        }

        private async Task SubmitAsync()
        {
            bool hasError = false;
            foreach (var entry in SubjectEntries)
            {
                entry.Validate();
                if (entry.HasErrors)
                    hasError = true;
            }

            if (hasError)
            {
                MessageBox.Show(".بعضی داده ها نامعتبر هستند، لطفا دوباره بررسی کنید", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var records = SubjectEntries
                .Select(entry => new StudyRecord
                {
                    Student = _student,
                    Subject = entry.Subject,
                    MinutesStudied = int.TryParse(entry.MinutesStudied, out var minutes) ? minutes : 0,
                    Date = DateTime.Now
                })
                .ToList();

            App.Db.StudyRecords.AddRange(records);
            await App.Db.SaveChangesAsync();

            MessageBox.Show("!اطلاعات مطالعه با موفقیت ثبت شد", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            foreach (var entry in SubjectEntries)
            {
                entry.MinutesStudied = string.Empty;
            }
        }
    }
}
