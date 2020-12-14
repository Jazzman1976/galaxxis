
using Galaxxis.Core.Business;
using Galaxxis.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Galaxxis.Core.Engine
{
    public sealed class Ticker
    {
        #region Fields ----------------------------------------------------------------------------

        // singleton instance
        private static Ticker _instance = null;

        private Timer _timer;
        public string Status { get; set; }

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
                    long timePerTick
                        = Context.Instance.Database.Universe.First().UniverseTimePerTick;
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
            // raise usiverse ticks +1
            Context.Instance.Database.Universe.First().UniverseTicks++;

            // save changes
            if (Context.Instance.Database.Universe.First().UniverseTicks % 30 == 0)
            {
                // this must not be done async!
                Processor.Instance.SaveChanges();
            }

            // sync changes
            else if (Context.Instance.Database.Universe.First().UniverseTicks % 20 == 0)
            {
                // this must not be done async!
                Processor.Instance.SyncChanges();
            }

            // process
            else if (Context.Instance.Database.Universe.First().UniverseTicks % 10 == 0)
            {
                // this is done async
                Processor.Instance.ProcessTask();
            }
            
            //=================================================================

            // run processor (async)
            
        }

        #endregion
    }
}
