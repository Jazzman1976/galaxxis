using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Galaxxis.Core.Ticker
{
    public sealed class Ticker
    {
        #region Fields ----------------------------------------------------------------------------

        private static Ticker _instance = null;

        #endregion


        #region Properties ------------------------------------------------------------------------

        public static Ticker Instance
        {
            get
            {
                if (Ticker._instance == null)
                {
                    Ticker._instance = new Ticker();
                }
                return Ticker._instance;
            }
        }

        #endregion


        #region constructors ----------------------------------------------------------------------

        private Ticker() 
        {
            
        }

        #endregion
    }
}
