using System;
using System.Collections.Generic;

#nullable disable

namespace Galaxxis.Data.Models
{
    public partial class Ship
    {
        public int ShipId { get; set; }
        public int ShipUserId { get; set; }
        public int ShipPlanetId { get; set; }
        public string ShipType { get; set; }

        public virtual User ShipUser { get; set; }
    }
}
