using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class PlanetToSystem
    {
        public int PlanetId { get; set; }
        public int SystemId { get; set; }

        public virtual Planet Planet { get; set; }
        public virtual System System { get; set; }
    }
}
