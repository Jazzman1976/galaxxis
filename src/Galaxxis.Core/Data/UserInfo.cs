using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Galaxxis.Data;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Data
{
    public class UserInfo
    {
        public User Entity { get; private set; }

        private static readonly GalaxxisContext _dbContext = new GalaxxisContext();

        public UserInfo(int id)
        {
            this.Entity = _dbContext.User.FirstOrDefault(user => user.UserId.Equals(id));
        }
    }
}
