using Poker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    /// <summary>
    /// 카드 뭉치.
    /// 카드들을 관리하는 클래스.
    /// </summary>
    public class Deck
    {
        private List<Card> m_listCard = new List<Card>();

        public Deck()
        {
            InitCard();
        }

        /// <summary>
        /// 52장의 카드를 만들어야 한다.
        /// </summary>
        private void InitCard()
        {
            //suit[] suits = (suit[])Enum.GetValues(typeof(suit));
            //suit[] suits = new suit[] { suit.spade, suit.diamond, suit.heart, suit.clover };
            //suit test = (suit)1;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 2; j < 15; j++)
                {
                    m_listCard.Add(new Card((suit)i, (rank)j));
                }
            }
        }

        /// <summary>
        /// 카드 뭉치를 섞는다.
        /// </summary>
        public void Suffle()
        {
            Random rand = new Random();
            for (int i = 0; i < m_listCard.Count; i++)
            {
                int result = rand.Next(i, m_listCard.Count);
                Card tempCard = m_listCard[i];
                m_listCard[i] = m_listCard[result];
                m_listCard[result] = tempCard;
            }
        }

        public Card DealCard()
        {
            Card dealCard = m_listCard[0];
            m_listCard.RemoveAt(0);
            return dealCard;
        }

        public List<Card> DealCard(int _count)
        {
            List<Card> listDealCard = new List<Card>();
            for (int i = 0; i < _count; i++)
            {
                listDealCard.Add(m_listCard[0]);
                m_listCard.RemoveAt(0);
            }
            return listDealCard;
        }



        /// <summary>
        /// 덱의 있는 카드들을 순서대로 확인합니다.
        /// 실제 포커에는 필요가 없는 기능.
        /// </summary>
        public void ShowDeck()
        {
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
                str += " " + (int)curRank + ", ";

                Console.Write(str);
                if ((i + 1) % 13 == 0)
                    Console.WriteLine();
            }
        }


    }

}
