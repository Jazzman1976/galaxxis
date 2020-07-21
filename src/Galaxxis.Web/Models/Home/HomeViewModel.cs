using Galaxxis.Core.Business;
using Galaxxis.Core.Business.Provider;

using Galaxxis.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Galaxxis.Web.Models.Home
{
    public class HomeViewModel
    {
        #region Properties ------------------------------------------------------------------------

        public User User { get; private set; }

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
        }

        #endregion
    }
}
