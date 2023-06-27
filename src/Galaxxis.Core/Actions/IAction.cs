using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Actions
{
    public interface IAction
    {
        /// <summary>
        /// The TriggerEvent is used to identify this action to be executed
        /// </summary>
        EventNameEnum TriggerEvent { get; }

        /// <summary>
        /// Performs database changes when the event triggers
        /// </summary>
        bool DoAction(Event @event, GalaxxisContext context, out string feedback);
     }
}
