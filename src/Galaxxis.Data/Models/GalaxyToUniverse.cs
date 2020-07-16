using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class GalaxyToUniverse
    {
        public int GalaxyId { get; set; }
        public int UniverseId { get; set; }

        public virtual Galaxy Galaxy { get; set; }
        public virtual Universe Universe { get; set; }
    }
}
