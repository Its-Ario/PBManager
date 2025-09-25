using CommunityToolkit.Mvvm.ComponentModel;
using PBManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.UI.MVVM.ViewModel.Helpers
{
    public partial class GradeEntry : ObservableObject
    {
        public Subject Subject { get; }

        [ObservableProperty]
        private string _score;

        public GradeEntry(Subject subject)
        {
            Subject = subject;
        }
    }
}
