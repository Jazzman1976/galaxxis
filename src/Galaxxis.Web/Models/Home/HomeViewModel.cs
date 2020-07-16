using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Web.Models.Home
{
    public class HomeViewModel
    {
        private GalaxxisContext _context = new GalaxxisContext();

        public User User { get; private set; }
        public Universe Universe { get; private set; }

        public HomeViewModel()
        {
            this.User = _context.User.FirstOrDefault(user => user.UserId.Equals(1));
            this.Universe = _context.Universe.FirstOrDefault(univ => univ.UniverseId.Equals(1));
        }
    }
}
