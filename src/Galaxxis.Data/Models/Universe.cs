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
            Job = new HashSet<Job>();
        }

        public int UniverseId { get; set; }
        public string UniverseName { get; set; }
        public long UniverseTimePerTick { get; set; }
        public long UniverseTicks { get; set; }
        public long UniverseTicksProcessed { get; set; }

        public virtual ICollection<Galaxy> Galaxy { get; set; }
        public virtual ICollection<Job> Job { get; set; }
    }
}
