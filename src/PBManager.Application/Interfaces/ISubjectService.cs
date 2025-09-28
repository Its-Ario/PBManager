using PBManager.Core.Entities;

namespace PBManager.Application.Interfaces;

public interface ISubjectService
{
    Task<List<Subject>> GetSubjectsAsync(bool tracking = false);
    Task<int> GetSubjectCountAsync();
}