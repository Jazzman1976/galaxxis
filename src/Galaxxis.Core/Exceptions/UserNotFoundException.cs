
namespace Galaxxis.Core.Exceptions
{
    using System;

    class UserNotFoundException : Exception
    {
        public UserNotFoundException() : base("User not found!") { }
    }

    class PlanetNotFoundException : Exception
    {
        public PlanetNotFoundException() : base("Planet not found!") { }
    }

    class EventNotFoundException : Exception
    {
        public EventNotFoundException() : base("Event not found!") { }
    }
}
