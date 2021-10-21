using Galaxxis.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Galaxxis.Data.Models
{
    public partial class Event
    {
        internal Event() { }

        /// <summary>
        /// Length in ticks until this event is executed (timespan).
        /// The exact tick when it is executed will be calculated from this value (time).
        /// </summary>
        [NotMapped]
        public long EventTicksToEventTick { get; set; }
    }
}
