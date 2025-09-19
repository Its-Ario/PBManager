using PBManager.Core.Interfaces;

namespace PBManager.Application.Interfaces
{
    public interface IDialogService
    {
        void ShowManagementWindow<T>(string title) where T : class, IManagedEntity;
    }
}
