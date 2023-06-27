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
    internal class ProduceDefenseLightManualCannonAction : BaseAction
    {
        public override EventNameEnum TriggerEvent 
            => EventNameEnum.ProduceDefenseLightManualCannon;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                User user = context.User.FirstOrDefault(u => u.UserId.Equals(planet.PlanetUserId));
                planet.UpdateRessources();
                context.Defense.Add(new Defense
                {
                    DefenseUserId = planet.PlanetUserId,
                    DefensePlanetId = planetId,
                    DefenseType = DefenseTypeEnum.LightManualCannon.ToString(),
                    DefenseTactical = user.GetTacticalEnum().ToString(),
                    DefenseWeaponSlots = user.IsOffensive() ? 2 : 1,
                    DefenseArmorSlots = user.IsOffensive() ? 1 : 2,
                    DefenseItemSlots = 2,
                    DefenseStructurePoints = 50,
                    DefenseCpuPower = 9,
                    DefenseAccuracy = user.IsOffensive() ? 45 : 40,
                    DefenseShieldPower = user.IsOffensive() ? 20 : 25,
                    DefenseAttackPower = user.IsOffensive() ? 40 : 35,
                    DefenseArmour = user.IsOffensive() ? 10 : 15
                });

                feedback = 
                    $"{DateStamp}: Defense Light Manual Cannon produced " +
                    $"on planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Produce Defense Light Manual Cannon: no conditions for any change found.";
            return false;
        }
    }
}
