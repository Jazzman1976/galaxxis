using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class Universe
    {
        public int UniverseId { get; set; }
        public string UniverseName { get; set; }
        public long UniverseTimePerTick { get; set; }
        public long UniverseTicks { get; set; }
    }
}
