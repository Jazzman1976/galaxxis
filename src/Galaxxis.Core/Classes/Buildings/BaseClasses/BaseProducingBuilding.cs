using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.Enums;
using Galaxxis.Core.Classes.Buildings.Interfaces;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Extensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.BaseClasses
{
    public abstract class BaseProducingBuilding : IBuilding, IProducingBuilding, IConsumingBuilding
    {
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

        public abstract GrowthRate BuildTimeRate { get; }

        public abstract GrowthRate BuildCostOreRate { get; }

        public abstract GrowthRate BuildCostManaRate { get; }

        public abstract GrowthRate BuildCostCrystalsRate { get; }

        public abstract GrowthRate BuildCostElementiumRate { get; }

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

        public abstract bool IsProducing { get; }

        public abstract GrowthRate ConsumptionEnergyRate { get; }

        public abstract GrowthRate ConsumptionManaRate { get; }

        public abstract GrowthRate ConsupmtionAntimatter { get; }

        public abstract GrowthRate ConsumptionElementium { get; }

        public EnumConsumingType ConsumingType => EnumConsumingType.production;

        public abstract bool Build(out string info);

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

        public BaseProducingBuilding(Planet planet)
        {
            _planet = planet;
            _owner = planet.PlanetUser ?? UserProvider.GetUser(planet.PlanetUserId);
        }
    }
}
