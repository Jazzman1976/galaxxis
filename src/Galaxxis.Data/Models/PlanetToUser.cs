using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class PlanetToUser
    {
        public int PlanetId { get; set; }
        public int UserId { get; set; }

        public virtual Planet Planet { get; set; }
        public virtual User User { get; set; }
    }
}
