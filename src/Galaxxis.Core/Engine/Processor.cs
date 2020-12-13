using Galaxxis.Core.Business;
using Galaxxis.Core.Extensions;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxxis.Core.Engine
{
    public sealed class Processor
    {
        private static Processor _instance = null;

        private bool IsProcessing { get; set; } = false;

        public static Processor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Processor();
                }
                return _instance;
            }
        }

        public static async Task Process()
        {
            await Task.Run(() => 
            {
                // wait until done
                if (Instance.IsProcessing)
                {
                    return;
                }

                // mark processing
                Instance.IsProcessing = true;

                // get context and universe
                GalaxxisContext context = Context.Instance.Database;
                Universe universe = context.Universe.First();

                // process all ticks until current tick is reached
                while (universe.UniverseTicksProcessed < universe.UniverseTicks)
                {
                    // raise tick + 1
                    universe.UniverseTicksProcessed++;

                    // Process every 120 ticks (2 Minutes)
                    if (universe.UniverseTicksProcessed % 120 == 0)
                    {
                        foreach (Planet planet in context.Planet)
                        {
                            // Save current ressources to db.
                            // Ressources could be calculated for each tick instead 
                            // reading from db. The field 'PlanetDataTick' shows which 
                            // tick is stored to db.
                            planet.UpdateRessources();
                        }
                    }
                }

                // update database
                context.SaveChanges();

                // mark not processing, unlock processor
                Instance.IsProcessing = false;
            });            
        }
    }
}
