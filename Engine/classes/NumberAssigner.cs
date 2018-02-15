using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.classes
{
    public static class NumberAssigner
    {
        static int nextNumber = 0;
        public static int GetNextNumber() {
            nextNumber++;
            return nextNumber;
        }
    }
}
