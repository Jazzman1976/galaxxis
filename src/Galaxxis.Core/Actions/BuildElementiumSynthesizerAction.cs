using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
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
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetElementiumSynthesizer++;
                planet.ClearCache();
                
                feedback = $"{DateStamp}: Elementium Synthesizer build on planet {planet.PlanetId}.";
                return true;
            }

            feedback = $"Build Elementium Synthesizer Action: no conditions for any change found.";
            return false;
        }
    }
}
