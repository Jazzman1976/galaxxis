using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class Ship
    {
        public int ShipId { get; set; }
        public int ShipUserId { get; set; }
        public int ShipPlanetId { get; set; }
        public string ShipType { get; set; }
        public string ShipTactical { get; set; }
        public int ShipWeaponSlots { get; set; }
        public int ShipArmorSlots { get; set; }
        public int ShipItemSlots { get; set; }
        public int ShipStructurePoints { get; set; }
        public int ShipDriveStrength { get; set; }
        public int ShipPowerConsumption { get; set; }
        public int ShipStorageCapacity { get; set; }
        public int ShipCpuPower { get; set; }
        public int ShipAccuracy { get; set; }
        public int ShipShieldPower { get; set; }
        public int ShipAttackPower { get; set; }
        public int ShipArmour { get; set; }

        public virtual User ShipUser { get; set; }
    }
}
