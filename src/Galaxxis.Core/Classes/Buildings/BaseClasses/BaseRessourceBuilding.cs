using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.Interfaces;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Extensions;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.BaseClasses
{
    public abstract class BaseRessourceBuilding : IBuilding, IRessourceBuilding
    {
        #region Protected Fields ------------------------------------------------------------------

        protected Planet _planet;
        protected User _owner;
        protected double[] _buildCosts;
        protected bool? _canBuild;
        protected GrowthRate _ressourceGrowthRate;
        protected GrowthRate _buildTimeRate;
        protected GrowthRate _buildCostOreRate;
        protected GrowthRate _buildCostManaRate;
        protected GrowthRate _buildCostCrystalsRate;
        protected GrowthRate _buildCostElementiumRate;
        protected double _incomePerDay = -1;
        protected double _incomePerTick = -1;
        protected double? _costsPerTick;
        protected string _timeFormat = "HH.mm.ss.fffffff";

        #endregion


        #region Abstract Properties ---------------------------------------------------------------

        public abstract GrowthRate RessourceGrowthRate { get; }

        public abstract GrowthRate BuildTimeRate { get; }

        public abstract GrowthRate BuildCostOreRate { get; }

        public abstract GrowthRate BuildCostManaRate { get; }

        public abstract GrowthRate BuildCostCrystalsRate { get; }

        public abstract GrowthRate BuildCostElementiumRate { get; }

        public abstract double RessourceCostsPerTick { get; }

        public abstract double Usage { get; }

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
        {
            get
            {
                if (_canBuild == null)
                {
                    _canBuild = BuildCosts[0] <= _planet.PlanetOre
                        && BuildCosts[1] <= _planet.PlanetMana
                        && BuildCosts[2] <= _planet.PlanetCrystals
                        && BuildCosts[3] <= _planet.PlanetElementium
                        && !EventProvider
                            .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Build)
                            .Any();
                }
                return _canBuild == true;
            }
        }

        public double IncomePerDay => _incomePerDay < 0 
            ? (_incomePerDay = ApplyRaceFactor(
                this.RessourceGrowthRate.Value + this.RessourceGrowthRate.BaseIncome) * Usage) 
            : _incomePerDay;

        public double IncomePerTick => _incomePerTick < 0
            ? (_incomePerTick = ApplyRaceFactor(
                this.RessourceGrowthRate.GetValuePerTick()
                + this.RessourceGrowthRate.GetBaseIncomePerTick())
                * Usage)
            : _incomePerTick;

        public double AmountPerTick => IncomePerTick - RessourceCostsPerTick;

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public abstract double ApplyRaceFactor(double value);

        public abstract bool Build(out string info);

        public abstract bool ChangeUsage(double newValue, out string info);

        /// <summary>
        /// Use this in the Build method of derived classes to build the next building level 
        /// using the correct event name.
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
        /// Use this in the ChangeUsage method of derived classes to change the building's usage
        /// using the correct event name.
        /// </summary>
        protected bool ChangeUsage(EventNameEnum eventName, double newValue, out string info)
        {
            // check conditions
            if (newValue < 0 || newValue > 1)
            {
                info = "The new value has to be between 0 and 1!";
                return false;
            }

            // queue event
            Processor.Instance.EventQueue
                .Enqueue(EventProvider.NewEvent(
                    EventLocationEnum.Planet,
                    _planet.PlanetId,
                    eventName,
                    1,
                    EventTypeEnum.Change,
                    newValue.ToString()));

            // exit
            info = "Usage of the building has been changed.";
            return true;
        }

        /// <summary>
        /// Get the consumption of this building's ressource by other buildings per tick <br/>
        /// supplyCosts parameter: <br/>
        /// <br/>
        /// 0: ore mine, 1: power supply, 2: factory, <br/>
        /// 3: mage academy, 4: crystal mine, 5: neutron accelerator, <br/>
        /// 6: elementium synthesizer <br/>
        /// <br />
        /// 7: war forge, 8: technology center, 9: high tech laboratory <br/>
        /// <br/>
        /// 10: research laboratory, 11: shipyard, 12: defense facility
        /// </summary>
        protected double GetCostsPerTick(int[] supplyCosts)
        {
            DateTime tStartTime = DateTime.Now;

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
                    supplyCosts[6], Constants.FACTOR_SUPPLY_COST, _planet.PlanetElementiumSynthesizer),

                decreaseRateShipyard = new GrowthRate(
                    supplyCosts[11], Constants.FACTOR_SUPPLY_COST, _planet.PlanetShipyard);

            double ressourceCostPerTick
                = decreaseRateOreMine.GetValuePerTick() * _planet.GetOreMine().Usage
                + decreaseRatePowerSupply.GetValuePerTick() * _planet.GetPowerSupply().Usage
                + decreaseRateFactory.GetValuePerTick() * _planet.GetFactory().Usage
                + decreaseRateNeutronAccelerator.GetValuePerTick()
                    * _planet.GetNeutronAccelerator().Usage
                + decreaseRateCrystalMine.GetValuePerTick() * _planet.GetCrystalMine().Usage
                + decreaseRateMageAcademy.GetValuePerTick() * _planet.GetMageAcademy().Usage
                + decreaseRateElementiumSynthesizer.GetValuePerTick()
                    * _planet.GetElementiumSynthesizer().Usage;


            List<Event> events = EventProvider
                .GetEvents(_planet.PlanetId, EventLocationEnum.Planet);

            Debug.WriteLine($"{DateStamp}: GetCostsPerTick events loaded in "
                        + $"{tStartTime.MsGone()} ms.");

            if (events
                .Any(e
                    => Enum.Parse(typeof(EventTypeEnum), e.EventType) is EventTypeEnum.Produce
                    && Enum.Parse(typeof(EventNameEnum), e.EventName)
                        is EventNameEnum.ProduceShip))
            {
                ressourceCostPerTick += decreaseRateShipyard.GetValuePerTick();
            }

            Debug.WriteLine($"{DateStamp}: GetCostsPerTick events checked in "
                        + $"{tStartTime.MsGone()} ms.");

            return ressourceCostPerTick;
        }

        private string DateStamp => DateTime.Now.ToString("HH.mm.ss.fffffff");

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
