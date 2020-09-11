using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxxis.Core.Extensions
{
    public static class PlanetExtensions
    {
        internal static void AddTickRessources(this Planet planet)
        {
            // base income per tick
            planet.PlanetEnergy += Calculations.GetValuePerTick(50);
            planet.PlanetOre += Calculations.GetValuePerTick(50);
            planet.PlanetTools += Calculations.GetValuePerTick(50);
            planet.PlanetAntimatter += Calculations.GetValuePerTick(30);
            planet.PlanetCrystals += Calculations.GetValuePerTick(30);
            planet.PlanetMana += Calculations.GetValuePerTick(30);
            planet.PlanetElementium += Calculations.GetValuePerTick(10);

            // energy income per level
            {
                double costsEnergy = Calculations.GetGrownValuePerTick(0, 2, planet.PlanetPowerSupply);
                double costsMana = Calculations.GetGrownValuePerTick(5, 2, planet.PlanetPowerSupply);
                double costsAntimatter = Calculations.GetGrownValuePerTick(5, 2, planet.PlanetPowerSupply);
                double costsElementium = Calculations.GetGrownValuePerTick(5, 2, planet.PlanetPowerSupply);
                bool hasEnergy = planet.PlanetEnergy - costsEnergy > 0;
                bool hasMana = planet.PlanetMana - costsMana > 0;
                bool hasAntimatter = planet.PlanetAntimatter - costsAntimatter > 0;
                bool hasElementium = planet.PlanetElementium - costsElementium > 0;
                if (hasEnergy && hasMana && hasAntimatter && hasElementium)
                {
                    // reduce production costs
                    planet.PlanetEnergy -= costsEnergy;
                    planet.PlanetMana -= costsMana;
                    planet.PlanetAntimatter -= costsAntimatter;
                    planet.PlanetElementium -= costsElementium;

                    // add income
                    planet.PlanetEnergy += Calculations.GetGrownValuePerTick(200, 1, planet.PlanetPowerSupply);
                }
            }

            // ore income per level
            {
                double costsEnergy = Calculations.GetGrownValuePerTick(5, 2, planet.PlanetOreMine);
                double costsMana = Calculations.GetGrownValuePerTick(5, 2, planet.PlanetOreMine);
                double costsAntimatter = Calculations.GetGrownValuePerTick(0, 2, planet.PlanetOreMine);
                double costsElementium = Calculations.GetGrownValuePerTick(5, 2, planet.PlanetOreMine);
                bool hasEnergy = planet.PlanetEnergy - costsEnergy > 0;
                bool hasMana = planet.PlanetMana - costsMana > 0;
                bool hasAntimatter = planet.PlanetAntimatter - costsAntimatter > 0;
                bool hasElementium = planet.PlanetElementium - costsElementium > 0;
                if (hasEnergy && hasMana && hasAntimatter && hasElementium)
                {
                    // reduce production costs
                    planet.PlanetEnergy -= costsEnergy;
                    planet.PlanetMana -= costsMana;
                    planet.PlanetAntimatter -= costsAntimatter;
                    planet.PlanetElementium -= costsElementium;

                    // add income
                    planet.PlanetOre += Calculations.GetGrownValuePerTick(200, 1, planet.PlanetOreMine);
                }
            }

            // tools income per level
            {
                double costsEnergy = Calculations.GetGrownValuePerTick(5, 2, planet.PlanetFactory);
                double costsMana = Calculations.GetGrownValuePerTick(0, 2, planet.PlanetFactory);
                double costsAntimatter = Calculations.GetGrownValuePerTick(5, 2, planet.PlanetFactory);
                double costsElementium = Calculations.GetGrownValuePerTick(5, 2, planet.PlanetFactory);
                bool hasEnergy = planet.PlanetEnergy - costsEnergy > 0;
                bool hasMana = planet.PlanetMana - costsMana > 0;
                bool hasAntimatter = planet.PlanetAntimatter - costsAntimatter > 0;
                bool hasElementium = planet.PlanetElementium - costsElementium > 0;
                if (hasEnergy && hasMana && hasAntimatter && hasElementium)
                {
                    // reduce production costs
                    planet.PlanetEnergy -= costsEnergy;
                    planet.PlanetMana -= costsMana;
                    planet.PlanetAntimatter -= costsAntimatter;
                    planet.PlanetElementium -= costsElementium;

                    // add income
                    planet.PlanetTools += Calculations.GetGrownValuePerTick(200, 1, planet.PlanetFactory);
                }
            }
        }
    }
}
