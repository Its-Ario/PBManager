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

        private Student? _student;
        public Student? Student
        {
            get => _student;
            private set => SetProperty(ref _student, value);
        }

        public StudentDetailViewModel()
        {
            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(StudentSelectedMessage message)
        {
            Student = message.Value;
        }

    }
}
