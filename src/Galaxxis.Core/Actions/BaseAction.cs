using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxxis.Core.Actions
{
    public abstract class BaseAction : IAction
    {
        static readonly string _dateFormat = "HH.mm.ss.fffffff";
        protected static string DateStamp => DateTime.Now.ToString(_dateFormat);

        public abstract EventNameEnum TriggerEvent { get; }
        public abstract bool DoAction(Event @event, GalaxxisContext context, out string feedback);
    }
}
