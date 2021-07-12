using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxxis.Web.Models.Admin
{
    public class AdminViewModel
    {
        public string TickerStatus { get; set; }

        public AdminViewModel()
        {
            this.TickerStatus = Galaxxis.Core.Engine.Ticker.Instance.Status;
        }
    }
}
