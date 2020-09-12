using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxxis.Core.Engine
{
    public class GrowthRate
    {
        private int _baseValue;
        private int _baseIncome;
        private int _faktor;
        private int _level;

        /// <summary>
        /// Grown value in 24 hours without base income
        /// </summary>
        public double Value => 
            _level < 1 
                ? 0
                : _baseValue * (_faktor * (_level - 1) ^ 2 + 1);

        /// <summary>
        /// Base income in 24 hours
        /// </summary>
        public double BaseIncome =>
            _baseIncome;

        /// <summary>
        /// A growthrate to calculate values which grow over time
        /// </summary>
        /// <param name="baseValue">Base value for the calculation</param>
        /// <param name="factor">How much growth raises per level</param>
        /// <param name="level">Higher levels produce a higher growth</param>
        /// <param name="baseIncome">Value always grown, even if level is 0</param>
        public GrowthRate(int baseValue, int factor, int level, int baseIncome = 0)
        {
            this._baseValue = baseValue;
            this._baseIncome = baseIncome;
            this._faktor = factor;
            this._level = level;
        }

        /// <summary>
        /// Get the value that has been grown in ticks
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public double GetValueForTicks(long ticks)
        {
            return GetValuePerTick() * ticks; 
        }

        public double GetBaseIncomeForTicks(long ticks)
        {
            return GetBaseIncomePerTick() * ticks;
        }

        /// <summary>
        /// We calculate with 1 tick = 1000 Milliseconds
        /// </summary>
        /// <param name="value24h">Value given for 24 hours</param>
        /// <returns>part of the 24h value in one tick</returns>
        public double GetValuePerTick()
        {
            if (this.Value <= 0)
            {
                return 0;
            }
            double value1h = this.Value / 24;
            double value1m = value1h / 60;
            double value1s = value1m / 60;
            return value1s;
        }

        public double GetBaseIncomePerTick()
        {
            if (this.BaseIncome <= 0)
            {
                return 0;
            }
            double baseIncome1h = this.BaseIncome / 24;
            double baseIncome1m = baseIncome1h / 60;
            double baseIncome1s = baseIncome1m / 60;
            return baseIncome1s;
        }
    }
}
