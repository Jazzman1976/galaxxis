using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Galaxxis.Core.Business.Provider
{
    public class UserProvider
    {
        public User GetUser(int id)
        {
            using (GalaxxisContext context = new GalaxxisContext())
            {
                return context.User

                    // get planets
                    .Include(user => user.Planet)

                    // get moons
                    .ThenInclude(planet => planet.Moon)

                    // get system
                    .Include(user => user.Planet)
                    .ThenInclude(planet => planet.PlanetSystem)
                    
                    // get galaxy
                    .ThenInclude(system => system.SystemGalaxy)

                    // get universe
                    .ThenInclude(galaxy => galaxy.GalaxyUniverse)

                    // return user with data
                    .FirstOrDefault(user => user.UserId.Equals(id));
            }
        }
    }
}
