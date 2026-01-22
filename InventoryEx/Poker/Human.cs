using Poker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public class Human
    {
        private string m_name;
        private List<Card> m_listCard = new List<Card>();
        private int m_money;


        public Human(string _name, int _money = 100)
        {
            m_name = _name;
            m_money = _money;
        }


        public void AddCard(Card _card)
        {
            m_listCard.Add(_card);
        }

        public List<Card> GetCards()
        {
            return m_listCard;
        }

        public int GetCardCount()
        {
            return m_listCard.Count;
        }
        public void ShowCard()
        {
            Console.WriteLine();
            Console.WriteLine($"{m_name}의 카드 {m_listCard.Count}장");

            string strSuit = "♠◈♥♣";
            for (int i = 0; i < m_listCard.Count; i++)
            {
                string str = "";
                suit curSuit = m_listCard[i].Suit;
                rank curRank = m_listCard[i].Rank;

                for (int j = 0; j < 4; j++)
                {
                    if (curSuit == (suit)j)
                        str += strSuit[j].ToString();
                }
                str += " " + curRank + ", ";

                Console.Write(str);
                if ((i + 1) % 13 == 0)
                    Console.WriteLine();
            }
        }

        

    }
}