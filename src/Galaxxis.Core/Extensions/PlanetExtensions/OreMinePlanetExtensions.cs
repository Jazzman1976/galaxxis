using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static class OreMinePlanetExtensions
    {
        /// <summary>
        /// The rate ore is gained by the ore mine
        /// </summary>
        internal static GrowthRate OreMine_GetGrowthRate(this Planet planet)
        {
            return new GrowthRate(200, 1, planet.PlanetOreMine, 50);
        }

        /// <summary>
        /// Income in 24 hours
        /// </summary>
        public static double OreMine_GetIncomePerDay(this Planet planet)
        {
            // init growth rate
            GrowthRate growthRateOre = planet.OreMine_GetGrowthRate();
            return growthRateOre.Value + growthRateOre.BaseIncome;
        }

        /// <summary>
        /// Get income of ore by the ore mine per tick
        /// </summary>
        public static double OreMine_GetIncomePerTick(this Planet planet)
        {
            // init growth rate
            GrowthRate growthRateOre = planet.OreMine_GetGrowthRate();
            return growthRateOre.GetValuePerTick() + growthRateOre.GetBaseIncomePerTick();
        }

        /// <summary>
        /// Amount of ore decreased each tick due to consuption in production
        /// </summary>
        public static double OreMine_GetCostsPerTick(this Planet planet)
        {
            // ore is not consumed by any structures!
            // ore is just used to build new structures or ships
            return 0;
        }

        /// <summary>
        /// Costs of the next ore mine level. 
        /// 0: ore, 1: mana, 2: crystals, 3: ticks
        /// </summary>
        public static double[] OreMine_GetBuildCosts(this Planet planet)
        {
            GrowthRate
                constructRateOre = new GrowthRate(
                    10, Constants.FACTOR_BUILD_COST, planet.PlanetOreMine + 1),
                constructRateMana = new GrowthRate(
                    10, Constants.FACTOR_BUILD_COST, planet.PlanetOreMine + 1),
                constructRateCrystals = new GrowthRate(
                    5, Constants.FACTOR_BUILD_COST, planet.PlanetOreMine + 1),
                constructRateTicks = new GrowthRate(
                    5, Constants.FACTOR_BUILD_TIME, planet.PlanetOreMine + 1);

            return new double[]
            {
                constructRateOre.Value,
                constructRateMana.Value,
                constructRateCrystals.Value,
                constructRateTicks.Value
            };
        }

        /// <summary>
        /// True if enough ressources are available to build next level ore mine
        /// </summary>
        public static bool OreMine_CanBuild(this Planet planet)
        {
            double[] oreMineCosts = planet.OreMine_GetBuildCosts();
            return
                oreMineCosts[0] <= planet.PlanetOre
                && oreMineCosts[1] <= planet.PlanetMana
                && oreMineCosts[2] <= planet.PlanetCrystals;
        }

        /// <summary>
        /// Start building the next level of the ore mine. An event is added to the db.
        /// </summary>
        /// <returns>True, if event is added, otherwise false (conditions not fullfilled)</returns>
        public static bool OreMine_BuildOreMine(this Planet planet, out string info)
        {
            // check conditions
            double[] costs = planet.OreMine_GetBuildCosts();
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
                5, Constants.FACTOR_BUILD_TIME, planet.PlanetOreMine + 1);

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
                Data.Enums.EventNameEnum.BuildOreMine,
                universeTicks + timeRate.ValueLong);

            info = "Build ore mine event added";

            return true;
        }
    }
}
