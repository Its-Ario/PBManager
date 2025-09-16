using PBManager.Core.Entities;

namespace PBManager.Core.Interfaces;

public interface ISubjectRepository
{
    Task<List<Subject>> GetAllAsync();
    Task<int> GetCountAsync();
}