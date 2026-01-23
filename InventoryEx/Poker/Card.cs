using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    // 문양
    public enum suit
    {
        Clover =0,
        spade,
        diamond,
        heart,
    }
    // 숫자 파워
    public enum rank
    {
        None = 0,
        Dummy = 1,
        Two = 2,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }


    public class Card
    {
        private suit m_suit;
        public suit Suit { get { return m_suit; } }
        private rank m_rank;
        public rank Rank { get { return m_rank; } }

        public Card(suit _suit, rank _rank)
        {
            m_suit = _suit;
            m_rank = _rank;
        }

    }
}

/*
 * 족보의 특징
 * 문양이 같거나
 * 랭크가 같거나
 * 랭크가 이어질 때
 * 족보가 만들어지며
 * 특별해질수록 강한 족보임
 * 
 */ 