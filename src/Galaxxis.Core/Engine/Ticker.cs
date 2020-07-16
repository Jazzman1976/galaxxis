
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
        public string Status;

        #endregion


        #region Properties ------------------------------------------------------------------------

        public static Ticker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Ticker();

                    // init instance
                    GalaxxisContext context = new GalaxxisContext();
                    long timePerTick = context.Universe.First().UniverseTimePerTick;
                    _instance._timer = new Timer(timePerTick);
                    _instance._timer.Elapsed += OnTimedEvent;
                    _instance._timer.AutoReset = true;
                    _instance._timer.Enabled = true;
                    _instance.Stop();
                }
                return _instance;
            }
        }

        #endregion


        #region constructors ----------------------------------------------------------------------

        private Ticker() { }

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

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            // stop timer until changes beeing progressed
            Instance.Stop();

            GalaxxisContext context = new GalaxxisContext();

            // save usiverse tick +1
            context.Universe.First().UniverseTicks++;

            // save changes
            context.SaveChanges();

            // continue timer
            Instance.Start();
        }

        #endregion
    }
}
