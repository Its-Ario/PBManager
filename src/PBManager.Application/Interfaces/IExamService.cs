using PBManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.Application.Interfaces
{
    public interface IExamService
    {
        Task<List<Exam>> GetAllExamsWithSubjectsAsync();
    }
}
