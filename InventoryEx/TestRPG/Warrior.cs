using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRPG
{
    internal class Warrior : Character
    {
        public Warrior(string _name, int _hp, int _atk, int _lv, int _def, int _exp, int _gold = 500)
        {
            m_name = _name;   
            m_atk = _atk;
            m_lv = _lv;
            m_gold = _gold;
            m_hp = 100;
            m_def = 30;
            m_exp = _exp;
        }

        public override void ShowStatus()
        {
            Console.WriteLine("================");
            Console.WriteLine($"이름 : {m_name} ");
            Console.WriteLine($"체력 : {m_hp} ");
            Console.WriteLine($"레벨 : {m_lv} ");
            Console.WriteLine($"경험치 : {m_exp} ");
            Console.WriteLine($"공격력 : {m_atk} ");
            Console.WriteLine($"방어력 : {m_def} ");
            Console.WriteLine($"재화 : {m_gold} ");
            Console.WriteLine("================");
        }
    }
}
