using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.Interfaces;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Extensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.BaseClasses
{
    public abstract class BaseRessourceBuilding : IBuilding, IRessourceBuilding
    {
        #region Protected Fields ------------------------------------------------------------------

        protected Planet _planet;
        protected User _owner;
        protected double[] _buildCosts;
        protected GrowthRate _ressourceGrowthRate;
        protected GrowthRate _buildTimeRate;
        protected GrowthRate _buildCostOreRate;
        protected GrowthRate _buildCostManaRate;
        protected GrowthRate _buildCostCrystalsRate;
        protected GrowthRate _buildCostElementiumRate;
        protected double _costsPerTick = -1;

        #endregion


        #region Abstract Properties ---------------------------------------------------------------

        public abstract GrowthRate RessourceGrowthRate { get; }

        public abstract GrowthRate BuildTimeRate { get; }

        public abstract GrowthRate BuildCostOreRate { get; }

        public abstract GrowthRate BuildCostManaRate { get; }

        public abstract GrowthRate BuildCostCrystalsRate { get; }

        public abstract GrowthRate BuildCostElementiumRate { get; }

        public abstract double RessourceCostsPerTick { get; }

        #endregion


        #region Public Properties -----------------------------------------------------------------

        public double[] BuildCosts
            => _buildCosts
            ??= new double[]
                {
                    BuildCostOreRate.Value,
                    BuildCostManaRate.Value,
                    BuildCostCrystalsRate.Value,
                    BuildCostElementiumRate.Value,
                    BuildTimeRate.Value
                };

        public bool CanBuild
            => BuildCosts[0] <= _planet.PlanetOre
            && BuildCosts[1] <= _planet.PlanetMana
            && BuildCosts[2] <= _planet.PlanetCrystals
            && BuildCosts[3] <= _planet.PlanetElementium
            && !EventProvider
                .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Build)
                .Any();

        public double IncomePerDay
            => ApplyRaceFactor(
                this.RessourceGrowthRate.Value
                + this.RessourceGrowthRate.BaseIncome);

        public double IncomePerTick
            => ApplyRaceFactor(
                this.RessourceGrowthRate.GetValuePerTick()
                + this.RessourceGrowthRate.GetBaseIncomePerTick());

        public double AmountPerTick => IncomePerTick - RessourceCostsPerTick;


        #endregion


        #region Methods ---------------------------------------------------------------------------

        public abstract double ApplyRaceFactor(double value);

        public abstract bool Build(out string info);

        /// <summary>
        /// Use this in the Build method of derived classes to build the next building level 
        /// using the correct event.
        /// name.
        /// </summary>
        protected bool Build(EventNameEnum eventName, out string info)
        {
            // check conditions
            if (EventProvider
                .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Build)
                .Any())
            {
                info = "A build is still in progress!";
                return false;
            }
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
            if (BuildCosts[3] > _planet.PlanetElementium)
            {
                info = "Not enough elementium!";
                return false;
            }

            // queue event
            Processor.Instance.EventQueue
                .Enqueue(EventProvider.NewEvent(
                    EventLocationEnum.Planet,
                    _planet.PlanetId,
                    eventName,
                    BuildTimeRate.ValueLong,
                    EventTypeEnum.Build,
                    BuildCosts.ToValueString()));

            // exit
            info = "Construction of the next building level has been started.";
            return true;
        }

        /// <summary>
        /// Get the consumption of this building's ressource by other buildings per tick <br/>
        /// supplyCosts parameter: <br/>
        /// 0: ore mine, 1: power supply, 2: factory, <br/>
        /// 3: mage academy, 4: crystal mine, 5: neutron accelerator, <br/>
        /// 6: elementium synthesizer
        /// </summary>
        protected double GetCostsPerTick(int[] supplyCosts)
        {
            GrowthRate
                decreaseRateOreMine = new GrowthRate(
                    supplyCosts[0], Constants.FACTOR_SUPPLY_COST, _planet.PlanetOreMine),
                decreaseRatePowerSupply = new GrowthRate(
                    supplyCosts[1], Constants.FACTOR_SUPPLY_COST, _planet.PlanetPowerSupply),
                decreaseRateFactory = new GrowthRate(
                    supplyCosts[2], Constants.FACTOR_SUPPLY_COST, _planet.PlanetFactory),
                decreaseRateNeutronAccelerator = new GrowthRate(
                    supplyCosts[3], Constants.FACTOR_SUPPLY_COST, _planet.PlanetMageAcademy),
                decreaseRateCrystalMine = new GrowthRate(
                    supplyCosts[4], Constants.FACTOR_SUPPLY_COST, _planet.PlanetCrystalMine),
                decreaseRateMageAcademy = new GrowthRate(
                    supplyCosts[5], Constants.FACTOR_SUPPLY_COST, _planet.PlanetNeutronAccelerator),
                decreaseRateElementiumSynthesizer = new GrowthRate(
                    supplyCosts[6], Constants.FACTOR_SUPPLY_COST, _planet.PlanetElementiumSynthesizer);

            double ressourceCostPerTick
                = decreaseRateOreMine.GetValuePerTick()
                + decreaseRatePowerSupply.GetValuePerTick()
                + decreaseRateFactory.GetValuePerTick()
                + decreaseRateNeutronAccelerator.GetValuePerTick()
                + decreaseRateCrystalMine.GetValuePerTick()
                + decreaseRateMageAcademy.GetValuePerTick()
                + decreaseRateElementiumSynthesizer.GetValuePerTick();

            return ressourceCostPerTick;
        }

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public BaseRessourceBuilding(Planet planet)
        {
            _planet = planet;
            _owner = planet.PlanetUser ?? UserProvider.GetUser(planet.PlanetUserId);
        }

        #endregion

    }
}
