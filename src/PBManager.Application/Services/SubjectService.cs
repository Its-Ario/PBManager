using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;

namespace PBManager.Application.Services;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;

    public SubjectService(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public Task<List<Subject>> GetSubjectsAsync()
    {
        return _subjectRepository.GetAllAsync();
    }

    public Task<int> GetSubjectCountAsync()
    {
        return _subjectRepository.GetCountAsync();
    }
}