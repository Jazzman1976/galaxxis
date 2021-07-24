using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxxis.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static int MsGone(this DateTime tStart)
        {
            return new TimeSpan(DateTime.Now.Ticks - tStart.Ticks).Milliseconds;
        }
    }
}
