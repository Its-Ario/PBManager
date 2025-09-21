using PBManager.Core.Interfaces;

namespace PBManager.Core.Entities
{
    public class Class : IManagedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
