using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class System
    {
        public System()
        {
            PlanetToSystem = new HashSet<PlanetToSystem>();
            SystemToGalaxy = new HashSet<SystemToGalaxy>();
        }

        public int SystemId { get; set; }
        public string SystemName { get; set; }

        public virtual ICollection<PlanetToSystem> PlanetToSystem { get; set; }
        public virtual ICollection<SystemToGalaxy> SystemToGalaxy { get; set; }
    }
}
