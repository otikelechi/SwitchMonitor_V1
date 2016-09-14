using SwitchMonitor_v1.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitchMonitor_v1.Data
{
    public class ConfigContext : DbContext
    {
        public DbSet<NodeResult> NodeUpdates { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public ConfigContext() : base("SwitchMonitor")
        {
        }
    }
}
