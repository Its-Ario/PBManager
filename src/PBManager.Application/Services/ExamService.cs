using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.Application.Services
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;

        public ExamService(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public Task<List<Exam>> GetAllExamsWithSubjectsAsync()
        {
            return _examRepository.GetAllWithSubjectsAsync();
        }
    }
}
