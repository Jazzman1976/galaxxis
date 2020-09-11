using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxxis.Core.Engine
{
    public static class Calculations
    {
        public static double GetGrownValuePerTick(int baseValue24hours, int faktor, int level)
        {
            if (level < 1)
            {
                return 0;
            }
            return GetValuePerTick(baseValue24hours * (faktor * (level - 1) ^ 2 + 1));
        }

        /// <summary>
        /// We calculate with 1 tick = 1000 Milliseconds
        /// </summary>
        /// <param name="value24h">Value given for 24 hours</param>
        /// <returns>part of the 24h value in one tick</returns>
        public static double GetValuePerTick(double value24h)
        {
            if(value24h <= 0)
            {
                return 0;
            }
            double value1h = value24h / 24;
            double value1m = value1h / 60;
            double value1s = value1m / 60;
            return value1s;
        }
    }
}
