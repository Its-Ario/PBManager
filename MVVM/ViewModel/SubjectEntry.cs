using CommunityToolkit.Mvvm.ComponentModel;
using PBManager.MVVM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.MVVM.ViewModel
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class SubjectEntry : ObservableValidator
    {
        public Subject Subject { get; set; }

        private string _minutesStudied;

        [RegularExpression(@"^\d+$", ErrorMessage = "Must be a number")]
        public string MinutesStudied
        {
            get => _minutesStudied;
            set => SetProperty(ref _minutesStudied, value, true);
        }

        public void Validate() => base.ValidateAllProperties();
    }
}
