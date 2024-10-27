
namespace Galaxxis.Core.Provider
{
    using Galaxxis.Core.Exceptions;
    using Galaxxis.Data.Enums;
    using Galaxxis.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;

    public static class UserProvider
    {
        /// <summary>
        /// Get user object by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>User object</returns>
        /// <exception cref="UserNotFoundException">If the user is not found by id.</exception>
        public static User GetUser(int id, bool includeData = false)
        {
            User user = null;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                // init query
                IQueryable<User> query = context.User.Where(u => u.UserId.Equals(id));

                // add data to query
                if (includeData)
                {
                    query = query
                        .Include(user => user.Planet)
                            .ThenInclude(planet => planet.Moon)
                        .Include(user => user.Planet)
                            .ThenInclude(planet => planet.PlanetSystem)
                                .ThenInclude(system => system.SystemGalaxy)
                                    .ThenInclude(galaxy => galaxy.GalaxyUniverse)
                        .Include(user => user.Ship)
                        .Include(user => user.Defense);
                }

                // get user
                user = query.FirstOrDefault();
            }

            // not found exception
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            // exit
            return user;
        }

        /// <summary>
        /// Get user object by email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>User object</returns>
        /// <exception cref="UserNotFoundException">If the user is not found by email.</exception>
        public static User GetUser(string email, bool includeData = false)
        {
            User user = null;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                user = context.User.FirstOrDefault(u => u.UserEmail.Equals(email));
            }
            if (user == null)
            {
                throw new UserNotFoundException();
            }
            return GetUser(user.UserId, includeData);
        }

        public static bool AddNewUser(
            string userEmail, string userFirstname, string userLastname, int startingPlanetId)
        {
            // init new context for operation
            using GalaxxisContext context = new ();

            // get the planet where to change the user
            Planet planet = PlanetProvider.GetPlanet(startingPlanetId, context: context);

            // add new user
            User user = new()
            {
                UserEmail = userEmail,
                UserFirstname = userFirstname,
                UserLastname = userLastname,
                UserRace = UserRacesEnum.Human.ToString() //TODO: add race selection step!
            };
            context.User.Add(user);
            context.SaveChanges(); // save to get userId through incremental autoset when added

            //  and set planet as first home planet
            planet.PlanetUserId = user.UserId;
            context.SaveChanges();

            // return success flag
            return true;
        }
    }
}
