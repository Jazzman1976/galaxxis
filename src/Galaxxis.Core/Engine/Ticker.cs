using Galaxxis.Core.Provider;
using Galaxxis.Data.Models;
using System.Threading;
using System.Timers;

namespace Galaxxis.Core.Engine
{
    public sealed class Ticker
    {
        #region Fields ----------------------------------------------------------------------------

        // singleton instance
        private static Ticker _instance = null;
        private System.Timers.Timer _timer;

        // atomic tick storage used by Interlocked
        private long _ticks;

        #endregion


        #region Properties ------------------------------------------------------------------------

        // expose ticks safely (reads use Interlocked.Read)
        public long Ticks => Interlocked.Read(ref _ticks);

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
                    _instance._timer = new System.Timers.Timer(universe.UniverseTimePerTick);
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
            // set backing field directly
            _ticks = tick;
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
            // increment backing field atomically and pass the new target tick to the processor
            long targetTick = Interlocked.Increment(ref Instance._ticks);
            Processor.Instance.ProcessAsync(targetTick);
        }

        #endregion
    }
}
