using Galaxxis.Core.Engine;
using Galaxxis.Core.Provider;
using Galaxxis.Data.Models;

namespace Galaxxis.Web.Views.Home
{
    public class HomeViewModel
    {
        #region Properties ------------------------------------------------------------------------

        public User User { get; private set; }
        public Ticker Ticker { get; private set; }
        public string Info { get; set; }

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public HomeViewModel(Code.Identity.AppUser user)
        {
            // get current user
            this.User = UserProvider.GetUser(user.Email, true);

            // get ticker
            this.Ticker = Ticker.Instance;
        }

        public HomeViewModel(Code.Identity.AppUser user, string info) : this(user)
        {
            this.Info = info;
        }

        #endregion
    }
}
