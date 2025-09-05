using CommunityToolkit.Mvvm.Messaging.Messages;
using PBManager.MVVM.Model;

namespace PBManager.Messages
{
    public class StudentSelectedMessage : ValueChangedMessage<Student?>
    {
        public StudentSelectedMessage(Student? student) : base(student) { }
    }

}
