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
                    .Include(user => user.PlanetToUser)
                    .ThenInclude(p2u => p2u.Planet)

                    // get systems of planets
                    .ThenInclude(planet => planet.PlanetToSystem)
                    .ThenInclude(p2s => p2s.System)

                    // get galaxies of systems
                    .ThenInclude(system => system.SystemToGalaxy)
                    .ThenInclude(s2g => s2g.Galaxy)

                    // return user with data
                    .FirstOrDefault(user => user.UserId.Equals(id));
            }
        }
    }
}
