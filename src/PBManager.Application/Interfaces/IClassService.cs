using PBManager.Core.Entities;

namespace PBManager.Application.Interfaces;

public interface IClassService
{
    Task<List<Class>> GetClassesAsync();
    Task<int> GetClassCountAsync();
}