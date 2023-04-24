using System;
using System.Collections.Generic;

namespace Galaxxis.Data.Models
{
    public partial class Defense
    {
        public int DefenseId { get; set; }
        public int DefenseUserId { get; set; }
        public int DefensePlanetId { get; set; }
        public string DefenseType { get; set; }
        public string DefenseTactical { get; set; }
        public int DefenseWeaponSlots { get; set; }
        public int DefenseArmorSlots { get; set; }
        public int DefenseItemSlots { get; set; }
        public int DefenseStructurePoints { get; set; }
        public int DefenseCpuPower { get; set; }
        public int DefenseAccuracy { get; set; }
        public int DefenseShieldPower { get; set; }
        public int DefenseAttackPower { get; set; }
        public int DefenseArmour { get; set; }

        public virtual User DefenseUser { get; set; }
    }
}
