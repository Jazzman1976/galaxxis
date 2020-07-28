
namespace Galaxxis.Core.Exceptions
{
    using System;

    class UserNotFoundException : Exception
    {
        public UserNotFoundException() : base("User not found!") { }
    }
}
