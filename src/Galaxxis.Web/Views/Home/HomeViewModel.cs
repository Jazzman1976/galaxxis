using Galaxxis.Core.Provider;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;
using System;
using Microsoft.AspNetCore.Mvc;

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
            // for now: user with id 1
            // TODO: Login and use Login User

            this.User = UserProvider.GetUser(user.Id, true); //UserProvider.GetUser(1, true);

            this.Ticker = Ticker.Instance;
        }

        public HomeViewModel(Code.Identity.AppUser user, string info) : this(user)
        {
            this.Info = info;
        }

        #endregion
    }
}
