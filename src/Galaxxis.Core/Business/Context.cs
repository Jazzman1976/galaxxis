using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Business
{
    public sealed class Context
    {
        private static Context _instance = null;

        public GalaxxisContext Database { get; private set; }

        public static Context Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Context();
                    _instance.Database = new GalaxxisContext();
                }

                return _instance;
            }
        }
    }
}
