using PBManager.Core.Interfaces;

namespace PBManager.Core.Entities
{
    public class Class : IManagedEntity
    {
        public required int Id { get; set; }
        public string Name { get; set; }
    }
}
