﻿using System;
using System.Collections.Generic;
using System.Text;


namespace Engine
{
    public class HealingPotion : Item
    {
        public HealingPotion(int id, string name, string namePlural, int amountToHeal) : base(id, name,namePlural)
        {
            AmountToHeal = amountToHeal;
        }

        public int AmountToHeal { get; set; }
    }
}
