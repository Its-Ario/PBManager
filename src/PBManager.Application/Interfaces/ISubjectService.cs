using PBManager.Core.Entities;

namespace PBManager.Application.Interfaces;

public interface ISubjectService
{
    Task<List<Subject>> GetSubjectsAsync();
    Task<int> GetSubjectCountAsync();
}