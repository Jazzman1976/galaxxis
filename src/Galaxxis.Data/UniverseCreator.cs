using System;
using System.Collections.Generic;
using System.Linq;

namespace Galaxxis.Data
{
    public static class UniverseCreator
    {
        //public static List<int[]> Galaxies = new List<int[]>();

        public static bool Create3D(int minDist, int size, int count, out List<int[]> created)
        {
            Random rnd = new Random();
            int triesToAdd = 0;
            int maxTries = 100000;
            created = new List<int[]>();
            bool success = false;

            while (created.Count < count)
            {
                // raise triesToAdd
                triesToAdd++;

                if (triesToAdd > maxTries)
                {
                    break;
                }

                // try to add another                
                int[] newCoordinate = new int[] 
                {
                    rnd.Next(-size, size),
                    rnd.Next(-size, size),
                    rnd.Next(-size, size)
                };
                if (created.Any(coordinate => GetDistance(coordinate, newCoordinate) < minDist))
                {
                    continue;
                }
                created.Add(newCoordinate);
            }

            if (triesToAdd > maxTries)
            {
                System.Diagnostics.Debug.WriteLine("creation incomplete!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("creation successfull!");
                success = true;
            }

            return success;
        }

        private static double GetDistance(int[] coordinateA, int[] coordinateB)
        {
            int deltaX = coordinateA[0] - coordinateB[0];
            int deltaY = coordinateA[1] - coordinateB[2];
            int deltaZ = coordinateA[2] - coordinateB[2];

            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
            return distance;
        }
    }
}
