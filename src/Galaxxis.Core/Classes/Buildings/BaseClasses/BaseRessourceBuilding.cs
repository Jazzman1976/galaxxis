using Galaxxis.Core.Classes.Buildings.Interfaces;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Provider;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events;
using Galaxxis.Data.Events.BuildEvents;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.BaseClasses
{
    public abstract class BaseRessourceBuilding : IBuilding, IRessourceBuilding, IConsumingBuilding
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
        protected GrowthRate _consumptionEnergyRate;
        protected GrowthRate _consumptionManaRate;
        protected GrowthRate _consupmtionAntimatter;
        protected GrowthRate _consumptionElementium;

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

        // IConsumingBuilding

        public abstract GrowthRate ConsumptionEnergyRate { get; }

        public abstract GrowthRate ConsumptionManaRate { get; }

        public abstract GrowthRate ConsupmtionAntimatter { get; }

        public abstract GrowthRate ConsumptionElementium { get; }

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
        protected bool Build<TBuildEvent>(out string info) where TBuildEvent : BaseBuildEvent
        {
            return BuildingsHelper.BuildOnPlanet<TBuildEvent>(
                _planet, BuildCosts, BuildTimeRate.ValueLong, out info);
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

            // get event
            Event @event = null;
            ChangeUsageEventTypeData data = new ChangeUsageEventTypeData(newValue);
            int id = _planet.PlanetId;
            switch (eventName)
            {
                case EventNameEnum.ChangeOreMineUsage:
                    @event = new ChangeOreMineUsageEvent(EventLocationEnum.Planet, id, data);
                    break;
            }

            // queue event
            if (@event != null)
            {
                Processor.Instance.EventQueue.Enqueue(@event);
            }

            // exit
            info = "Usage of the building has been changed.";
            return true;
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
