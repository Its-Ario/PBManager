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

    public async Task<Class> GetClassByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new Exception($"Empty Classname");

        var classEntity = await _classRepository.GetByNameAsync(name);

        if (classEntity == null) throw new Exception($"Class '{name}' not found in database.");
        return classEntity;
    }

    public async Task<Class> GetClassByIdAsync(int id)
    {
        var classEntity = await _classRepository.GetByIdAsync(id);

        if (classEntity == null) throw new Exception($"Class '{id}' not found in database.");
        return classEntity;
    }
}