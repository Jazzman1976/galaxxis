using Galaxxis.Data.Models;
using System;
using System.Linq;
using System.Timers;

namespace Galaxxis.Core.Engine
{
    public sealed class Ticker
    {
        #region Fields ----------------------------------------------------------------------------

        // singleton instance
        private static Ticker _instance = null;
        private Timer _timer;

        #endregion


        #region Properties ------------------------------------------------------------------------

        public long Ticks { get; set; }

        public string Status { get; set; }

        public static Ticker Instance
        {
            get
            {
                if (_instance == null)
                {
                    using (GalaxxisContext context = new GalaxxisContext())
                    {
                        // init instance
                        long timePerTick = context.Universe.First().UniverseTimePerTick;
                        long ticks = context.Universe.First().UniverseTicks;

                        _instance = new Ticker(ticks);
                        _instance._timer = new Timer(timePerTick);
                        _instance._timer.Elapsed += OnTickerElapsed;
                        _instance._timer.AutoReset = true;
                        _instance._timer.Enabled = true;
                        _instance.Stop();
                    }
                }
                return _instance;
            }
        }

        #endregion


        #region constructors ----------------------------------------------------------------------

        private Ticker(long tick) 
        {
            this.Ticks = tick;
        }

        #endregion


        #region methods ---------------------------------------------------------------------------

        public void Start()
        {
            Instance._timer.Start();
            Instance.Status = "running";
        }

        public void Stop()
        {
            Instance._timer.Stop();
            Instance.Status = "stopped";
        }

        #endregion


        #region handler ---------------------------------------------------------------------------

        private static void OnTickerElapsed(object source, ElapsedEventArgs e)
        {
            Processor.Instance.ProcessAsync(Instance.Ticks++);
        }

        #endregion
    }
}
