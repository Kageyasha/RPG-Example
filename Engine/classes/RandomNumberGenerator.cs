using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.classes
{
    public static class RandomNumberGenerator
    {
        private static Random generator = new Random();
        public static int NumberBetween(int minVal, int maxVal) {
            return generator.Next(minVal,maxVal+1);
        }

    }
}
