using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class Planet
    {
        public Planet()
        {
            MoonToPlanet = new HashSet<MoonToPlanet>();
            PlanetToSystem = new HashSet<PlanetToSystem>();
            PlanetToUser = new HashSet<PlanetToUser>();
        }

        public int PlanetId { get; set; }
        public string PlanetName { get; set; }
        public int PlanetFields { get; set; }

        public virtual ICollection<MoonToPlanet> MoonToPlanet { get; set; }
        public virtual ICollection<PlanetToSystem> PlanetToSystem { get; set; }
        public virtual ICollection<PlanetToUser> PlanetToUser { get; set; }
    }
}
