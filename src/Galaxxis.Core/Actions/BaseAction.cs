using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;

namespace Galaxxis.Core.Actions
{
    public abstract class BaseAction : IAction
    {
        protected static string DateStamp => DateTime.Now.ToString(Static.Constants.DEBUG_TIME_FORMAT);

        public abstract EventNameEnum TriggerEvent { get; }
        public abstract bool DoAction(Event @event, GalaxxisContext context, out string feedback);
    }
}
