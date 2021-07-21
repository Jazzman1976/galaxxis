using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.Interfaces;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.BaseClasses
{
    public abstract class BasePlanetRessourceBuilding : IPlanetBuilding
    {
        #region Protected Fields ------------------------------------------------------------------

        protected Planet _planet;
        protected double[] _buildCosts;
        protected GrowthRate _ressourceGrowthRate;
        protected GrowthRate _buildTimeRate;
        protected GrowthRate _buildCostOreRate;
        protected GrowthRate _buildCostManaRate;
        protected GrowthRate _buildCostCrystalsRate;

        #endregion


        #region Abstract Properties ---------------------------------------------------------------

        public abstract GrowthRate RessourceGrowthRate { get; }

        public abstract GrowthRate BuildTimeRate { get; }

        public abstract GrowthRate BuildCostOreRate { get; }

        public abstract GrowthRate BuildCostManaRate { get; }

        public abstract GrowthRate BuildCostCrystalsRate { get; }

        public abstract double CostsPerTick { get; }

        #endregion


        #region Public Properties -----------------------------------------------------------------

        public double[] BuildCosts
            => _buildCosts
            ??= new double[]
                {
                    BuildCostOreRate.Value,
                    BuildCostManaRate.Value,
                    BuildCostCrystalsRate.Value,
                    BuildTimeRate.Value
                };        

        public bool CanBuild
            => BuildCosts[0] <= _planet.PlanetOre
            && BuildCosts[1] <= _planet.PlanetMana
            && BuildCosts[2] <= _planet.PlanetCrystals;

        public double IncomePerDay
            => this.RessourceGrowthRate.Value + this.RessourceGrowthRate.BaseIncome;

        public double IncomePerTick
            => this.RessourceGrowthRate.GetValuePerTick() 
            + this.RessourceGrowthRate.GetBaseIncomePerTick();

        public double AmountPerTick => IncomePerTick - CostsPerTick;

        #endregion


        #region Methods ---------------------------------------------------------------------------

        /// <summary>
        /// Start construction of the building's next level. <br/>
        /// Updates ressources! <br/>
        /// Saves changes to database!
        /// </summary>
        public abstract bool Build(out string info);

        /// <summary>
        /// Use this in the Build method of derived classes to build the next building level 
        /// using the correct event.
        /// name.
        /// </summary>
        protected bool Build(EventNameEnum eventName, out string info)
        {
            // check conditions
            if (BuildCosts[0] > _planet.PlanetOre)
            {
                info = "Not enough ore!";
                return false;
            }
            if (BuildCosts[1] > _planet.PlanetMana)
            {
                info = "Not enough mana!";
                return false;
            }
            if (BuildCosts[2] > _planet.PlanetCrystals)
            {
                info = "Not enough crystals!";
                return false;
            }

            // Add build event to DB
            long universeTicks;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                // get planet to edit
                Planet editPlanet = context.Planet
                    .Include(planet => planet.PlanetSystem)
                    .ThenInclude(system => system.SystemGalaxy)
                    .ThenInclude(galaxy => galaxy.GalaxyUniverse)
                    .First(p => p.PlanetId == _planet.PlanetId);

                // get universe ticks
                universeTicks = context.Universe
                    .First(u
                        => u.UniverseId
                        == editPlanet.PlanetSystem.SystemGalaxy.GalaxyUniverse.UniverseId)
                    .UniverseTicks;

                // pay costs                
                editPlanet.UpdateRessources();
                editPlanet.PlanetOre -= BuildCosts[0];
                editPlanet.PlanetMana -= BuildCosts[1];
                editPlanet.PlanetCrystals -= BuildCosts[2];

                // add event
                context.Event.Add(
                    EventProvider.NewEvent(
                        EventLocationTypeEnum.Planet,
                        _planet.PlanetId,
                        eventName,
                        universeTicks + BuildTimeRate.ValueLong));

                // save
                context.SaveChanges();
            }

            // exit
            info = "Construction of the next building level has been started.";
            return true;
        }

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public BasePlanetRessourceBuilding(Planet planet)
        {
            _planet = planet;
        }

        #endregion

    }
}
