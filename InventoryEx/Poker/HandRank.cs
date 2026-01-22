using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public enum Bonus
    {
        StraightFlush = 0,
        FourCard,
        FullHouse,
        Flush,
        Straight,
        Triple,
        TwoPair,
        OnePair,
        HighCard


    }

    public class HandRank
    {
        public Bonus HR { get; set; }

        public Card HighCard { get; set; }

        public HandRank(Bonus _ranking)
        {
            HR = _ranking;
        }
    }
}
