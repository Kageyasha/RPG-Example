using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class Item
    {
        public Item(int iD, string name, string namePlural)
        {
            ID = iD;
            Name = name;
            NamePlural = namePlural;
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string NamePlural { get; set; }
    }
}
