using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class Planet
    {
        public Planet()
        {
            Moon = new HashSet<Moon>();
        }

        public int PlanetId { get; set; }
        public string PlanetName { get; set; }
        public int PlanetFields { get; set; }
        public int PlanetSystemId { get; set; }
        public int PlanetUserId { get; set; }

        public virtual System PlanetSystem { get; set; }
        public virtual User PlanetUser { get; set; }
        public virtual ICollection<Moon> Moon { get; set; }
    }
}
