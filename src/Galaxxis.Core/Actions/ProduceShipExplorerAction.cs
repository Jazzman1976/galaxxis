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
    internal class ProduceShipExplorerAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.ProduceShipExplorer;

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
                    ShipType = ShipTypeEnum.Explorer.ToString(),
                    ShipTactical = user.IsOffensive()
                        ? TacticalEnum.Offensive.ToString()
                        : TacticalEnum.Defensive.ToString(),
                    ShipWeaponSlots = user.IsOffensive() ? 2 : 1,
                    ShipArmorSlots = user.IsOffensive() ? 1 : 2,
                    ShipItemSlots = 2,
                    ShipStructurePoints = 50,
                    ShipDriveStrength = 1000,
                    ShipPowerConsumption = 1,
                    ShipStorageCapacity = 50,
                    ShipCpuPower = 9,
                    ShipAccuracy = user.IsOffensive() ? 45 : 40,
                    ShipShieldPower = user.IsOffensive() ? 20 : 25,
                    ShipAttackPower = user.IsOffensive() ? 40 : 35,
                    ShipArmour = user.IsOffensive() ? 10 : 15
                });

                feedback = $"{DateStamp}: Ship Explorer produced on planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Produce Ship Explorer: no conditions for any change found.";
            return false;
        }
    }
}
