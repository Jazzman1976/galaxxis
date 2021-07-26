using System;
using System.Collections.Generic;

#nullable disable

namespace Galaxxis.Data.Models
{
    public partial class Universe
    {
        public Universe()
        {
            Galaxy = new HashSet<Galaxy>();
        }

        public int UniverseId { get; set; }
        public string UniverseName { get; set; }
        public long UniverseTimePerTick { get; set; }
        public long UniverseTicks { get; set; }

        public virtual ICollection<Galaxy> Galaxy { get; set; }
    }
}
