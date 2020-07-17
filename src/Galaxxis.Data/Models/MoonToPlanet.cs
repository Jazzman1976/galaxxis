using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class MoonToPlanet
    {
        public int MoonId { get; set; }
        public int PlanetId { get; set; }

        public virtual Moon Moon { get; set; }
        public virtual Planet Planet { get; set; }
    }
}
