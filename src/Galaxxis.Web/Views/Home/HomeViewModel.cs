using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;

namespace Galaxxis.Web.Views.Home
{
    public class HomeViewModel
    {
        #region Properties ------------------------------------------------------------------------

        public User User { get; private set; }
        public Ticker Ticker { get; private set; }

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public HomeViewModel()
        {
            // create helper objects
            UserProvider userProvider = new UserProvider();

            // get current user
            // for now: user with id 1
            // TODO: Login and use Login User
            this.User = userProvider.GetUser(1);

            this.Ticker = Ticker.Instance;
        }

        #endregion
    }
}
