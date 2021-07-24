using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;

namespace Galaxxis.Core.Engine
{
    public sealed partial class Processor
    {
        #region Fields ----------------------------------------------------------------------------

        private static Processor _instance = null;
        private static readonly string _dateFormat = "HH.mm.ss.fffffff";

        #endregion


        #region Properties ------------------------------------------------------------------------

        public bool IsWorking { get; set; }
        public Queue<Event> EventQueue { get; set; } = new Queue<Event>();
        private string DateStamp => DateTime.Now.ToString(_dateFormat);
        public static Processor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Processor();
                }
                return _instance;
            }
        }

        #endregion
    }
}
