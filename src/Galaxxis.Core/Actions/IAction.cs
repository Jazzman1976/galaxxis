using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxxis.Core.Actions
{
    public interface IAction
    {
        EventNameEnum TriggerEvent { get; }
        bool DoAction(Event @event, GalaxxisContext context, out string feedback);
     }
}
