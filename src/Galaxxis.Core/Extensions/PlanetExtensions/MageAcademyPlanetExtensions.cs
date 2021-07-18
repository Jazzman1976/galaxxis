using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static class MageAcademyPlanetExtensions
    {
        internal static GrowthRate MageAcademy_GetGrowthRate(this Planet planet)
        {
            return new GrowthRate(175, 1, planet.PlanetMageAcademy, 30);
        }

        /// <summary>
        /// Income in 24 hours
        /// </summary>
        public static double MageAcademy_GetIncomePerDay(this Planet planet)
        {
            // init growth rate
            GrowthRate growthRateMana = planet.MageAcademy_GetGrowthRate();
            return growthRateMana.Value + growthRateMana.BaseIncome;
        }

        /// <summary>
        /// Get income of ore by the ore mine per tick
        /// </summary>
        public static double MageAcademy_GetIncomePerTick(this Planet planet)
        {
            // init growth rate
            GrowthRate growthRateMana = planet.MageAcademy_GetGrowthRate();
            return growthRateMana.GetValuePerTick() + growthRateMana.GetBaseIncomePerTick();
        }

        /// <summary>
        /// Amount of ore decreased each tick due to consuption in production
        /// </summary>
        public static double MageAcademy_GetCostsPerTick(this Planet planet)
        {
            // ore is not consumed by any structures!
            // ore is just used to build new structures or ships
            return 0;
        }

        /// <summary>
        /// Costs of the next mage academy level. 
        /// 0: ore, 1: mana, 2: crystals, 3: ticks
        /// </summary>
        public static double[] MageAcademy_GetBuildCosts(this Planet planet)
        {
            GrowthRate
                constructRateOre = new GrowthRate(
                    15, Constants.FACTOR_BUILD_COST, planet.PlanetMageAcademy + 1),
                constructRateMana = new GrowthRate(
                    15, Constants.FACTOR_BUILD_COST, planet.PlanetMageAcademy + 1),
                constructRateCrystals = new GrowthRate(
                    10, Constants.FACTOR_BUILD_COST, planet.PlanetMageAcademy + 1),
                constructRateTicks = new GrowthRate(
                    5, Constants.FACTOR_BUILD_TIME, planet.PlanetMageAcademy + 1);

            return new double[]
            {
                constructRateOre.Value,
                constructRateMana.Value,
                constructRateCrystals.Value,
                constructRateTicks.Value
            };
        }

        /// <summary>
        /// True if enough ressources are available to build next level mage academy
        /// </summary>
        public static bool MageAcademy_CanBuild(this Planet planet)
        {
            double[] costs = planet.MageAcademy_GetBuildCosts();
            return
                costs[0] <= planet.PlanetOre
                && costs[1] <= planet.PlanetMana
                && costs[2] <= planet.PlanetCrystals;
        }

        public static bool MageAcademy_Build(this Planet planet, out string info)
        {
            // check conditions
            double[] costs = planet.MageAcademy_GetBuildCosts();
            if (costs[0] > planet.PlanetOre)
            {
                info = "Not enough ore!";
                return false;
            }
            if (costs[1] > planet.PlanetMana)
            {
                info = "Not enough mana!";
                return false;
            }
            if (costs[2] > planet.PlanetCrystals)
            {
                info = "Not enough crystals!";
                return false;
            }

            // GrowthRate build time
            GrowthRate timeRate = new GrowthRate(
                5, Constants.FACTOR_BUILD_TIME, planet.PlanetMageAcademy + 1);

            // Get DB context objects
            long universeTicks;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                // get universe ticks
                universeTicks = context.Universe.First().UniverseTicks;

                // pay costs
                Planet editPlanet = context.Planet.Where(p => p.PlanetId == planet.PlanetId).First();
                editPlanet.PlanetOre -= costs[0];
                editPlanet.PlanetMana -= costs[1];
                editPlanet.PlanetCrystals -= costs[2];

                // save
                context.SaveChanges();
            }

            // Add new event
            EventProvider.AddEvent(
                Data.Enums.EventLocationTypeEnum.Planet,
                planet.PlanetId,
                Data.Enums.EventNameEnum.BuildMageAcademy,
                universeTicks + timeRate.ValueLong);

            info = "Build mage academy event added";

            return true;
        }
    }
}
