using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemEx
{
    public class Item
    {
        string name;
        public string Name { get { return name; } }
        int price;
        public int Price { get { return price; } }


        public string GetName()
        {
            return name;
        }


        public Item(string _name, int _price)
        {
            name = _name;
            price = _price;
        }

        public class CashItem : Item
        {
            int cash;
            public CashItem(string _name, int _price) : base(_name, _price)
            {

            }
        }

        public class NomalItem : Item
        {
            public NomalItem(string _name, int _price) : base(_name, _price)
            {

            }
        }

        public class PlayerPet
        {

        }

    }
}
