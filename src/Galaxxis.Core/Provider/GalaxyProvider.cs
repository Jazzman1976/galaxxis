using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Galaxxis.Core.Provider
{
    public static class GalaxyProvider
    {
        /// <summary>
        /// Get a galaxy with all systems and it's universe
        /// </summary>
        /// <param name="galaxyId">Id of the galaxy to get</param>
        public static Galaxy GetGalaxy(int galaxyId)
        {
            Galaxy galaxy = null;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                galaxy = context.Galaxy
                    .Include(galaxy => galaxy.System)
                    .Include(galaxy => galaxy.GalaxyUniverse)
                    .FirstOrDefault(galaxy => galaxy.GalaxyId == galaxyId);
            }
            return galaxy;
        }
    }
}
