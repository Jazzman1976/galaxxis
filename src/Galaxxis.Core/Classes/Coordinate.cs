using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxxis.Core.Classes
{
    public class Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Coordinate(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
