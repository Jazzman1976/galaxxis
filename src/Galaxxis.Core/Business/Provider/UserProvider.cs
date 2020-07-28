
namespace Galaxxis.Core.Business.Provider
{
    using Galaxxis.Core.Exceptions;
    using Galaxxis.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;

    public class UserProvider
    {
        /// <summary>
        /// Get user object by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>User object</returns>
        /// <exception cref="UserNotFoundException">If the user is not found by id.</exception>
        public User GetUser(int id)
        {
            using (GalaxxisContext context = new GalaxxisContext())
            {
                User user = context.User

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

                if (user == null)
                {
                    throw new UserNotFoundException();
                }

                return user;
            }
        }

        /// <summary>
        /// Get user object by email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>User object</returns>
        /// <exception cref="UserNotFoundException">If the user is not found by email.</exception>
        public User GetUser(string email)
        {
            using (GalaxxisContext context = new GalaxxisContext())
            {
                User user = context.User.FirstOrDefault(u => u.UserEmail.Equals(email));
                if (user == null)
                {
                    throw new UserNotFoundException();
                }

                return GetUser(user.UserId);
            }
        }
    }
}
