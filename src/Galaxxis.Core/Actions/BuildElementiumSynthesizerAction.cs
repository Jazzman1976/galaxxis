using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.ComponentModel;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class BuildElementiumSynthesizerAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.BuildElementiumSynthesizer;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                // raise level
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetElementiumSynthesizer++;
                planet.ClearCache();

                // ensure new level does not cause negative amounts per tick
                double oldUsage = planet.PlanetElementiumSynthesizerUsage;
                while (planet.GetPowerSupply().AmountPerTick < 0
                    || planet.GetMageAcademy().AmountPerTick < 0
                    || planet.GetNeutronAccelerator().AmountPerTick < 0
                    || planet.GetElementiumSynthesizer().AmountPerTick < 0)
                { 
                    planet.PlanetElementiumSynthesizerUsage *= 0.9;
                }

                // give feedback
                feedback = $"{DateStamp}: Elementium synthesizer " +
                    $"level {planet.PlanetElementiumSynthesizer} " +
                    $"build on planet {planet.PlanetId}.";
                if (oldUsage != planet.PlanetElementiumSynthesizerUsage)
                {
                    feedback += $" Usage reduced to {planet.PlanetElementiumSynthesizerUsage} " +
                        $"to prevent negative amounts per tick.";
                }

                // exit
                return true;
            }

            feedback = $"Build Elementium Synthesizer Action: no conditions for any change found.";
            return false;
        }
    }
}
