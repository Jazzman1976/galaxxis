using Galaxxis.Core.Classes.Buildings.Interfaces;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events;
using Galaxxis.Data.Events.BuildEvents;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System;

namespace Galaxxis.Core.Classes.Buildings.BaseClasses
{
    public abstract class BaseRessourceBuilding<TBuildEvent, TChangeUsageEvent> 
        : BaseBuilding<TBuildEvent>, IRessourceBuilding, IConsumingBuilding
            where TBuildEvent : BaseBuildEvent
            where TChangeUsageEvent : Event
    {
        #region Protected Fields ------------------------------------------------------------------

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
        protected GrowthRate _consumptionEnergyRate;
        protected GrowthRate _consumptionManaRate;
        protected GrowthRate _consupmtionAntimatter;
        protected GrowthRate _consumptionElementium;

        #endregion


        #region Abstract Properties ---------------------------------------------------------------

        public abstract GrowthRate RessourceGrowthRate { get; }

        public abstract double RessourceCostsPerTick { get; }

        public abstract double Usage { get; }

        public abstract GrowthRate ConsumptionEnergyRate { get; }

        public abstract GrowthRate ConsumptionManaRate { get; }

        public abstract GrowthRate ConsupmtionAntimatter { get; }

        public abstract GrowthRate ConsumptionElementium { get; }

        #endregion


        #region Public Properties -----------------------------------------------------------------

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


        /// <summary>
        /// Use this in the ChangeUsage method of derived classes to change the building's usage
        /// using the correct event name.
        /// </summary>
        public bool ChangeUsage(double newValue, out string info)
        {
            // check conditions
            if (newValue < 0 || newValue > 1)
            {
                info = "The new value has to be between 0 and 1!";
                return false;
            }

            // get event
            //Event @event = null;
            ChangeUsageEventTypeData data = new ChangeUsageEventTypeData(newValue);
            int id = _planet.PlanetId;
            TChangeUsageEvent @event = (TChangeUsageEvent)Activator.CreateInstance(
                typeof(TChangeUsageEvent), EventLocationEnum.Planet, id, data);


            //switch (eventName)
            //{
            //    case EventNameEnum.ChangeOreMineUsage:
            //        @event = new ChangeOreMineUsageEvent(EventLocationEnum.Planet, id, data);
            //        break;
            //}

            // queue event
            if (@event != null)
            {
                Processor.Instance.EventQueue.Enqueue(@event);
            }

            // exit
            info = "Usage of the building has been changed.";
            return true;
        }

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public BaseRessourceBuilding(Planet planet) : base(planet)
        {
        }

        #endregion

    }
}
