using Galaxxis.Core.Extensions;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxxis.Core.Actions
{
    internal class ProduceShipCarrierAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.ProduceShipCarrier;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                User user = context.User.FirstOrDefault(u => u.UserId.Equals(planet.PlanetUserId));
                planet.UpdateRessources();
                context.Ship.Add(new Ship
                {
                    ShipUserId = planet.PlanetUserId,
                    ShipPlanetId = planetId,
                    ShipType = ShipTypeEnum.Carrier.ToString(),
                    ShipTactical = user.IsOffensive()
                        ? TacticalEnum.Offensive.ToString()
                        : TacticalEnum.Defensive.ToString(),
                    ShipWeaponSlots = user.IsOffensive() ? 2 : 1,
                    ShipArmorSlots = user.IsOffensive() ? 2 : 3,
                    ShipItemSlots = 3,
                    ShipStructurePoints = 80,
                    ShipDriveStrength = 800,
                    ShipPowerConsumption = 2,
                    ShipStorageCapacity = 500,
                    ShipCpuPower = 8,
                    ShipAccuracy = user.IsOffensive() ? 40 : 35,
                    ShipShieldPower = user.IsOffensive() ? 20 : 25,
                    ShipAttackPower = user.IsOffensive() ? 64 : 56,
                    ShipArmour = user.IsOffensive() ? 16 : 24
                });

                feedback = $"{DateStamp}: Ship Carrier produced on planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Produce Ship Carrier: no conditions for any change found.";
            return false;
        }
    }
}
