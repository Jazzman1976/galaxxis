using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class System
    {
        public System()
        {
            Planet = new HashSet<Planet>();
        }

        public int SystemId { get; set; }
        public string SystemName { get; set; }
        public int SystemGalaxyId { get; set; }

        public virtual Galaxy SystemGalaxy { get; set; }
        public virtual ICollection<Planet> Planet { get; set; }
    }
}
