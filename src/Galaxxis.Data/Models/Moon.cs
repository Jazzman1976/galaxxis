using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class Moon
    {
        public Moon()
        {
            MoonToPlanet = new HashSet<MoonToPlanet>();
        }

        public int MoonId { get; set; }
        public string MoonName { get; set; }
        public int MoonFields { get; set; }

        public virtual ICollection<MoonToPlanet> MoonToPlanet { get; set; }
    }
}
