using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class BuildMageAcademyAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.BuildMageAcademy;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetMageAcademy++;
                planet.ClearCache();

                feedback = $"{DateStamp}: Mage Academy build on planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Build Mage Academy Action: no conditions for any change found.";
            return false;
        }
    }
}
