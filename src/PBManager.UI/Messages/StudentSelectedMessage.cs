using CommunityToolkit.Mvvm.Messaging.Messages;
using PBManager.Core.Entities;

namespace PBManager.UI.Messages
{
    public class StudentSelectedMessage(Student? student) : ValueChangedMessage<Student?>(student)
    {
    }
}
