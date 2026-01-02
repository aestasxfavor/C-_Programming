using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestRPG
{
    internal abstract class Character
    {
        protected string m_name;
        public string Name { get { return m_name; } }

        protected int m_hp;
        public int Hp { get { return m_hp; } }

        protected int m_atk;
        public int Atk { get { return m_atk; } }

        protected int m_lv;
        public int Lv { get { return m_lv; } }

        protected int m_def;
        public int Def { get { return m_def; } }

        private int[] m_requireExp = new int[]
            { 0, 20, 50, 100, 200};

        protected int m_exp;
        public int Exp { get { return m_exp; } }

        protected double m_expPercentage => ((double)m_exp / m_requireExp[Lv]) * 100;
       

        protected int m_gold;
        public int Gold { get { return m_gold; } }

        public abstract void ShowStatus();
    }

}
