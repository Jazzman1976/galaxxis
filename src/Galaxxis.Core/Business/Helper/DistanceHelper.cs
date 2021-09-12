using Galaxxis.Core.Classes;
using Galaxxis.Data.Models;
using System;

namespace Galaxxis.Core.Business.Helper
{
    public static class DistanceHelper
    {
        /// <summary>
        /// Get the distance between two planets.
        /// It is assumed, these planets are in the same system.
        /// Range: 0 to ~35
        /// </summary>
        public static double GetDistance(Planet planetFrom, Planet planetTo)
        {
            Coordinate p1 = new Coordinate(
                planetFrom.PlanetCoordinateX,
                planetFrom.PlanetCoordinateY,
                planetFrom.PlanetCoordinateZ);
            Coordinate p2 = new Coordinate(
                planetTo.PlanetCoordinateX,
                planetTo.PlanetCoordinateY,
                planetTo.PlanetCoordinateZ);
            return GetDistance(p1, p2);
        }

        /// <summary>
        /// Get the distance between two coordinates.
        /// </summary>
        public static double GetDistance(Coordinate from, Coordinate to)
        {
            double rangeX = (double)(to.X - from.X);
            double rangeY = (double)(to.Y - from.Y);
            double rangeZ = (double)(to.Z - from.Z);
            return GetDistance(rangeX, rangeY, rangeZ);
        }

        /// <summary>
        /// Get the distance between 2 3d points
        /// </summary>
        /// <param name="rangeX">the distance between x1 and x2 coords</param>
        /// <param name="rangeY">the distance between y1 and y2 coords</param>
        /// <param name="rangeZ">the distance between z1 and z2 coords</param>
        /// <returns></returns>
        public static double GetDistance(double rangeX, double rangeY, double rangeZ)
        {
            double distance = Math.Pow(
                Math.Pow(rangeX, 2) + Math.Pow(rangeY, 2) + Math.Pow(rangeZ, 2), 
                0.5);
            return distance;
        }
    }
}
