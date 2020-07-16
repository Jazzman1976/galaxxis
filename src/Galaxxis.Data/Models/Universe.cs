using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class Universe
    {
        public Universe()
        {
            GalaxyToUniverse = new HashSet<GalaxyToUniverse>();
        }

        public int UniverseId { get; set; }
        public string UniverseName { get; set; }
        public long UniverseTimePerTick { get; set; }
        public long UniverseTicks { get; set; }

        public virtual ICollection<GalaxyToUniverse> GalaxyToUniverse { get; set; }
    }
}
