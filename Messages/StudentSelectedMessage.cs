using CommunityToolkit.Mvvm.Messaging.Messages;
using PBManager.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.Messages
{
    public class StudentSelectedMessage : ValueChangedMessage<Student?>
    {
        public StudentSelectedMessage(Student? student) : base(student) { }
    }

}
