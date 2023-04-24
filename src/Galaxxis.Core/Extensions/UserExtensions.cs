using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;

namespace Galaxxis.Core.Extensions
{
    public static class UserExtensions
    {
        public static bool IsDefensive(this User user)
        {
            UserRacesEnum race = (UserRacesEnum)Enum.Parse(typeof(UserRacesEnum), user.UserRace);
            return
                race == UserRacesEnum.Human ||
                race == UserRacesEnum.Elf ||
                race == UserRacesEnum.Troll;
        }

        public static bool IsOffensive(this User user)
        {
            return !user.IsDefensive();
        }

        public static TacticalEnum GetTacticalEnum(this User user)
        {
            return user.IsDefensive()
                ? TacticalEnum.Defensive
                : TacticalEnum.Offensive;
        }
    }
}
