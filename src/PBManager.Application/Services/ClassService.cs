using Microsoft.Extensions.Caching.Memory;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Enums;
using PBManager.Core.Interfaces;

namespace PBManager.Application.Services;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly IMemoryCache _cache;
    private readonly IAuditLogService _auditLogService;

    public ClassService(IClassRepository classRepository, IMemoryCache cache, IAuditLogService auditLogService)
    {
        _classRepository = classRepository;
        _cache = cache;
        _auditLogService = auditLogService;
    }

    public async Task<List<Class>> GetClassesAsync()
    {
        const string cacheKey = "AllClasses";
        if (!_cache.TryGetValue(cacheKey, out List<Class> classes))
        {
            classes = await _classRepository.GetAllAsync();
            _cache.Set(cacheKey, classes, TimeSpan.FromMinutes(30));
        }
        return classes;
    }

    public async Task<int> GetClassCountAsync()
    {
        const string cacheKey = "ClassesCount";
        if (!_cache.TryGetValue(cacheKey, out int count))
        {
            count = await _classRepository.GetCountAsync();
            _cache.Set(cacheKey, count, TimeSpan.FromMinutes(30));
        }
        return count;
    }

    public Task<Class?> GetClassByIdAsync(int id)
    {
        string cacheKey = $"Class_{id}";
        return _cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return _classRepository.GetByIdAsync(id);
        });
    }

    public Task<Class?> GetClassByNameAsync(string name)
    {
        string cacheKey = $"ClassByName_{name.ToUpper()}";
        return _cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return _classRepository.GetByNameAsync(name);
        });
    }

    public async Task<Class> AddClassAsync(string name)
    {
        if (await _classRepository.GetByNameAsync(name) != null)
        {
            throw new InvalidOperationException("A class with this name already exists.");
        }

        var newClass = new Class { Name = name };
        await _classRepository.AddAsync(newClass);
        await _classRepository.SaveChangesAsync();

        _cache.Remove("AllClasses");
        _cache.Remove("ClassesCount");

        await _auditLogService.LogAsync(ActionType.Create, nameof(Class), newClass.Id, $"کلاس جدید ایجاد شد: '{name}'");

        return newClass;
    }

    public async Task UpdateClassAsync(Class classToUpdate)
    {
        _classRepository.Update(classToUpdate);
        await _classRepository.SaveChangesAsync();

        _cache.Remove("AllClasses");
        _cache.Remove($"Class_{classToUpdate.Id}");
        _cache.Remove($"ClassByName_{classToUpdate.Name.ToUpper()}");

        await _auditLogService.LogAsync(ActionType.Update, nameof(Class), classToUpdate.Id, $"کلاس ویرایش شد: '{classToUpdate.Name}'");
    }

    public async Task<bool> DeleteClassAsync(int id)
    {
        var classToDelete = await _classRepository.GetByIdAsync(id);
        if (classToDelete == null) return false;

        _classRepository.Delete(classToDelete);
        await _classRepository.SaveChangesAsync();

        _cache.Remove("AllClasses");
        _cache.Remove("ClassesCount");
        _cache.Remove($"Class_{id}");
        _cache.Remove($"ClassByName_{classToDelete.Name.ToUpper()}");

        await _auditLogService.LogAsync(ActionType.Delete, nameof(Class), id, $"کلاس حذف شد: '{classToDelete.Name}'");

        return true;
    }
}