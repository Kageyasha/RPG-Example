using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class Creature
    {
        public Creature(int currentHitPoints, int maximumHitPoints)
        {
            CurrentHitPoints = currentHitPoints;
            MaximumHitPoints = maximumHitPoints;
        }

        public int CurrentHitPoints { get; set; }
        public int MaximumHitPoints { get; set; }
    }
}
