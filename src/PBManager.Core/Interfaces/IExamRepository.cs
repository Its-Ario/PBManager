using PBManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.Core.Interfaces
{
    public interface IExamRepository
    {
        Task<Exam?> GetByIdAsync(int id);
        Task<List<Exam>> GetAllWithSubjectsAsync();
        Task AddAsync(Exam exam);
        void Update(Exam exam);
        void Delete(Exam exam);
        Task<int> SaveChangesAsync();
    }
}
