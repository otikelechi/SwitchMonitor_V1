using System.Data.Entity;
using SwitchMonitor_v1.Model;

namespace SwitchMonitor_v1.Data
{
    public class ConfigContext : DbContext
    {
        public ConfigContext() : base("SwitchMonitor")
        {
        }

        public DbSet<NodeResult> NodeUpdates { get; set; }
        public DbSet<Node> Nodes { get; set; }
    }
}