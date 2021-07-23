using System;
using System.Collections.Generic;

#nullable disable

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
        public long PlanetDataTick { get; set; }
        public double PlanetOre { get; set; }
        public double PlanetEnergy { get; set; }
        public double PlanetTools { get; set; }
        public double PlanetMana { get; set; }
        public double PlanetCrystals { get; set; }
        public double PlanetAntimatter { get; set; }
        public double PlanetElementium { get; set; }
        public int PlanetOreMine { get; set; }
        public double PlanetOreMineUsage { get; set; }
        public int PlanetPowerSupply { get; set; }
        public double PlanetPowerSupplyUsage { get; set; }
        public int PlanetFactory { get; set; }
        public double PlanetFactoryUsage { get; set; }
        public int PlanetMageAcademy { get; set; }
        public double PlanetMageAcademyUsage { get; set; }
        public int PlanetCrystalMine { get; set; }
        public double PlanetCrystalMineUsage { get; set; }
        public int PlanetNeutronAccelerator { get; set; }
        public double PlanetNeutronAcceleratorUsage { get; set; }
        public int PlanetElementiumSynthesizer { get; set; }
        public double PlanetElementiumSynthesizerUsage { get; set; }
        public long PlanetCreated { get; set; }

        public virtual System PlanetSystem { get; set; }
        public virtual User PlanetUser { get; set; }
        public virtual ICollection<Moon> Moon { get; set; }
    }
}
