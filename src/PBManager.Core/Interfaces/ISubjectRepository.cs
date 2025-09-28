using PBManager.Core.Entities;

namespace PBManager.Core.Interfaces;

public interface ISubjectRepository
{
    Task<List<Subject>> GetAllAsync(bool tracking = false);
    Task<int> GetCountAsync();
}