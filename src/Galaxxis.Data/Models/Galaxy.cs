using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class Galaxy
    {
        public Galaxy()
        {
            GalaxyToUniverse = new HashSet<GalaxyToUniverse>();
            SystemToGalaxy = new HashSet<SystemToGalaxy>();
        }

        public int GalaxyId { get; set; }
        public string GalaxyName { get; set; }

        public virtual ICollection<GalaxyToUniverse> GalaxyToUniverse { get; set; }
        public virtual ICollection<SystemToGalaxy> SystemToGalaxy { get; set; }
    }
}
