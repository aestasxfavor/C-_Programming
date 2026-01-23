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

        public Card RankingCard { get; set; }

        public HandRank(Bonus _ranking, Card _rankingCard)
        {
            HR = _ranking;
            RankingCard = _rankingCard;
        }
    }
}
