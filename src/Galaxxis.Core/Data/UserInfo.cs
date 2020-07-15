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
        private static readonly GalaxxisContext _dbContext = new GalaxxisContext();

        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserFirstname { get; set; }
        public string UserLastname { get; set; }

        public UserInfo(int id)
        {
            User entity = _dbContext.User.FirstOrDefault(user => user.UserId.Equals(id));

            if (entity == null)
            {
                return;
            }

            this.UserId = entity.UserId;
            this.UserEmail = entity.UserEmail;
            this.UserFirstname = entity.UserFirstname;
            this.UserLastname = entity.UserLastname;
        }
    }
}
