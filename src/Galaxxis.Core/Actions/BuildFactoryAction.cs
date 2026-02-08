using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class BuildFactoryAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.BuildFactory;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                // raise level
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetFactory++;
                planet.ClearCache();

                // ensure new level does not cause negative amounts per tick
                double oldUsage = planet.PlanetFactoryUsage;
                while (planet.GetPowerSupply().AmountPerTick < 0
                    || planet.GetMageAcademy().AmountPerTick < 0
                    || planet.GetNeutronAccelerator().AmountPerTick < 0
                    || planet.GetElementiumSynthesizer().AmountPerTick < 0)
                {
                    planet.PlanetFactoryUsage *= 0.9;
                }

                // give feedback
                feedback = $"{DateStamp}: Factory level {planet.PlanetFactory} " +
                    $"build on planet {planet.PlanetId}.";
                if (oldUsage != planet.PlanetFactoryUsage)
                {
                    feedback += $" Usage reduced to {planet.PlanetFactoryUsage} " +
                        $"to prevent negative amounts per tick.";
                }

                // exit
                return true;
            }

            feedback = $"Build Factory Action: no conditions for any change found.";
            return false;
        }
    }
}
