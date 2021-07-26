using System;
using System.Collections.Generic;

#nullable disable

namespace Galaxxis.Data.Models
{
    public partial class Galaxy
    {
        public Galaxy()
        {
            System = new HashSet<System>();
        }

        public int GalaxyId { get; set; }
        public string GalaxyName { get; set; }
        public int GalaxyUniverseId { get; set; }
        public int GalaxyCoordinateX { get; set; }
        public int GalaxyCoordinateY { get; set; }
        public int GalaxyCoordinateZ { get; set; }

        public virtual Universe GalaxyUniverse { get; set; }
        public virtual ICollection<System> System { get; set; }
    }
}
