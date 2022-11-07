using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBrother.Utils
{
    internal class Maths
    {
        public static int CalculateEuclideanDistance(int x, int z)
        {
            return (int)Math.Sqrt(x * x + z * z);
        }
    }
}
