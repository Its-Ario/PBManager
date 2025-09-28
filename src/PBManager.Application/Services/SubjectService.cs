using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;

namespace PBManager.Application.Services;

public class SubjectService(ISubjectRepository subjectRepository) : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository = subjectRepository;

    public Task<List<Subject>> GetSubjectsAsync(bool tracking = false)
    {
        return _subjectRepository.GetAllAsync(tracking);
    }

    public Task<int> GetSubjectCountAsync()
    {
        return _subjectRepository.GetCountAsync();
    }
}