using Galaxxis.Core.Provider;
using Galaxxis.Data.Models;
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
                    // get universe
                    // currently only one universe per app and database are supported
                    Universe universe = UniverseProvider.GetFirstUniverse();

                    // init instance
                    _instance = new Ticker(universe.UniverseTicks);
                    _instance._timer = new Timer(universe.UniverseTimePerTick);
                    _instance._timer.Elapsed += OnTickerElapsed;
                    _instance._timer.AutoReset = true;
                    _instance._timer.Enabled = true;
                    _instance.Stop();
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
