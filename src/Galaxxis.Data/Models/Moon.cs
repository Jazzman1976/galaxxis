using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class Moon
    {
        public int MoonId { get; set; }
        public string MoonName { get; set; }
        public int MoonFields { get; set; }
        public int MoonPlanetId { get; set; }

        public virtual Planet MoonPlanet { get; set; }
    }
}
