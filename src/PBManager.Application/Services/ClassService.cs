using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Application.Interfaces;

namespace PBManager.Application.Services;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;

    public ClassService(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<List<Class>> GetClassesAsync()
    {
        return await _classRepository.GetAllAsync();
    }

    public async Task<int> GetClassCountAsync()
    {
        return await _classRepository.GetCountAsync();
    }
}