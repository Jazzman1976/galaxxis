using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class BuildCrystalMineAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.BuildCrystalMine;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                // raise level
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetCrystalMine++;
                planet.ClearCache();

                // ensure new level does not cause negative amounts per tick
                double oldUsage = planet.PlanetCrystalMineUsage;
                while (planet.GetPowerSupply().AmountPerTick < 0
                    || planet.GetMageAcademy().AmountPerTick < 0
                    || planet.GetNeutronAccelerator().AmountPerTick < 0
                    || planet.GetElementiumSynthesizer().AmountPerTick < 0)
                { 
                    planet.PlanetCrystalMineUsage *= 0.9;
                }

                // give feedback
                feedback = $"{DateStamp}: Crystal mine level {planet.PlanetCrystalMine} build " +
                    $"on planet {planet.PlanetId}.";
                if (oldUsage != planet.PlanetCrystalMineUsage)
                {
                    feedback += $" Usage reduced to {planet.PlanetCrystalMineUsage} " +
                        $"to prevent negative amounts per tick.";
                }

                // exit
                return true;
            }

            feedback = $"Build Crystal Mine Action: no conditions for any change found.";
            return false;
        }
    }
}
