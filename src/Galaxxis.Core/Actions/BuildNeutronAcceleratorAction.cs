using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class BuildNeutronAcceleratorAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.BuildNeutronAccelerator;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                // raise level
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetNeutronAccelerator++;
                planet.ClearCache();

                // ensure new level does not cause negative amounts per tick
                double oldUsage = planet.PlanetNeutronAcceleratorUsage;
                while (planet.GetPowerSupply().AmountPerTick < 0
                    || planet.GetMageAcademy().AmountPerTick < 0
                    || planet.GetNeutronAccelerator().AmountPerTick < 0
                    || planet.GetElementiumSynthesizer().AmountPerTick < 0)
                {
                    planet.PlanetNeutronAcceleratorUsage *= 0.9;
                }

                // give feedback
                feedback = $"{DateStamp}: Neutron accelerator " +
                    $"level {planet.PlanetNeutronAccelerator} " +
                    $"build on planet {planet.PlanetId}.";
                if (oldUsage != planet.PlanetNeutronAcceleratorUsage)
                {
                    feedback += $" Usage reduced to {planet.PlanetNeutronAcceleratorUsage} " +
                        $"to prevent negative amounts per tick.";
                }

                // exit
                return true;
            }

            feedback = $"Build Neutron Accelerator Action: no conditions for any change found.";
            return false;
        }
    }
}
