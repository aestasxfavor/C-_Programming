using Poker;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public class PokerManager
    {
        private List<Human> m_listHuman = new List<Human>();

        private int m_turn = 0;
        private int m_index = 0;

        public PokerManager(int _humanCount)
        {
            m_listHuman.Add(new Human("플레이어"));
            for (int i = 1; i < _humanCount; i++)
            {
                string str = "갬블러 " + i.ToString();
                m_listHuman.Add(new Human(str));
            }
        }


        public void DealCard(List<Card> _listCard)
        {
            for (int i = 0; i < m_listHuman.Count; i++)
                m_listHuman[i].AddCard(_listCard[i]);
        }



        public int GetPlayerCount()
        {
            return m_listHuman.Count;
        }

        public void ShowCard()
        {
            for (int i = 0; i < m_listHuman.Count; i++)
            {
                m_listHuman[i].ShowCard();
            }
        }

        public void ShowCard(int _index)
        {
            m_listHuman[_index].ShowCard();
        }

        public HandRank HandRanking(int _index)
        {

            List<Card> listCard = m_listHuman[_index].GetCards();

            // 랭크 숫자 내림차순으로 정리
            List<int> listValue = new List<int>();
            for (int i = 0; i < listValue.Count; i++)
            {
                listValue.Add((int)listCard[i].Rank);
            }
            listValue.Sort();
            listValue.Reverse();

            // 63번줄 부터 69번줄까지 코드가 밑의 코드랑 같음 ㅅㅂ 알아볼 수 있게 써야지 현업에서도 저렇게는 안쓰겠다 아닌가 쓰려나
            //listValue = listCard.Select(n => (int)n.Rank).OrderByDescending(n => n).ToList();

            // 같은 숫자의 카드끼리 묶음이 필요하다
            // 묶는다? 리스트? 딕셔너리네 딕셔너리는 foreach써야함

            // 10 10 10 10 2
            Dictionary<int, int> dicValue = new Dictionary<int, int>();
            for (int i = 0; i < listValue.Count; i++)
            {
                int value = listValue[i];

                if (dicValue.ContainsKey(value))
                {
                    dicValue[value]++;
                }
                else
                {
                    dicValue.Add(value, 1);
                }

                Console.WriteLine($"value: {listValue[i]}");
            }

            listCard.GroupBy(n => (int)n.Rank).ToDictionary
                (
                g => g.Key
                );

            //foreach (int data in listValue)
            //{
            //    Console.WriteLine($"value: {data}");

            //}

            // 스트레이트 플러쉬
            if (CheckStraight(listValue) && CheckFlush(listCard))
            {
                Card rankCard = listCard.OrderBy(n => n.Rank).ThenBy(n => n.Suit).First();
                return new HandRank(Bonus.StraightFlush, rankCard);
            }

            // 포카드
            if (CheckFourCard(dicValue))
            {
                // 10 10 10 10 11 

                int value = 0;
                foreach (var data in dicValue)
                {
                    if (data.Value == 4)
                    {
                        value = data.Key;
                    }
                }
                // 포카드는 모든 문양이 있으므로 가장 강한 스페이드를 넣어주었다.
                Card rankCard = new Card(suit.spade, (rank)value);
                return new HandRank(Bonus.FourCard, rankCard);
            }

            // 풀하우스
            if (CheckTriple(dicValue) && CheckPair(dicValue) == 1)
            {
                int value = 0;
                foreach (var data in dicValue)
                {
                    if (data.Value == 3)
                    {
                        value = data.Key;
                    }
                }
                // 포카드는 모든 문양이 있으므로 가장 강한 스페이드를 넣어주었다.
                Card rankCard = new Card(suit.spade, (rank)value);
                return new HandRank(Bonus.FullHouse, rankCard);
            }

            // 플러쉬
            if (CheckFlush(listCard))
            {
                Card rankCard = listCard.OrderBy(n => n.Rank).First();
                return new HandRank(Bonus.Flush, rankCard);
            }

            // 스트레이트
            if (CheckStraight(listValue))
            {
                Card rankCard = listCard.OrderBy(n => n.Rank).First();
                return new HandRank(Bonus.Straight, rankCard);
            }

            // 트리플
            if (CheckTriple(dicValue))
            {
                Card rankCard = listCard.OrderBy(n => n.Rank).First();
                return new HandRank(Bonus.Triple, rankCard);
            }

            // 투페어
            if (CheckPair(dicValue) == 2)
            {
                int value = -1;
                foreach (var data in dicValue)
                {
                    if (data.Value == 2 && value < data.Key)
                    {
                        value = data.Key;
                    }
                }

                // value에는 숫자만 있음
                // 문양도 필요함
                Card card = null;
                for (int i = 0; i < listCard.Count; i++)
                {
                    if (value == (int)listCard[i].Rank)
                    {
                        card = listCard[i];
                    }
                    else if (card.Suit < listCard[i].Suit)
                    {
                        card = listCard[i];
                    }
                }

                return new HandRank(Bonus.TwoPair, card);
            }

            // 원페어
            if (CheckPair(dicValue) == 1)
            {
                Card rankCard = null;
                int value = 0;
                foreach (var data in dicValue)
                {
                    if (data.Value == 2)
                    {
                        value = data.Key;
                    }
                }

                // value에는 숫자만 있음
                // 문양도 필요함
                for (int i = 0; i < listCard.Count; i++)
                {
                    if (value == (int)listCard[i].Rank)
                    {
                        rankCard = listCard[i];
                    }

                }
                return new HandRank(Bonus.OnePair, rankCard);
            }

            {
                Card rankCard = listCard.OrderBy(n => n.Rank).First();

                return new HandRank(Bonus.HighCard, rankCard);
            }
          

        }

       

        private bool CheckStraight(List<int> _listValue)
        {

            int value = _listValue[0];  // 이거 왜 인덱스 벗어나냐..? 

            for (int i = 1; i < _listValue.Count; i++)
            {
                if (value - 1 != _listValue[i])
                {
                    return false;

                }
                value = _listValue[i];
            }

            return true;
        }

        private bool CheckFlush(List<Card> _listCard)
        {
            suit value = _listCard[0].Suit;

            for (int i = 1; i < _listCard.Count; i++)
            {
                if (value != _listCard[i].Suit)
                {
                    return false;

                }
            }

            return true;

        }

        private bool CheckFourCard(Dictionary<int, int> _dicValue)
        {
            foreach (var value in _dicValue)
            {
                if (value.Value == 4)
                {
                    return true;

                }
            }
            return false;
        }

        private bool CheckTriple(Dictionary<int, int> _dicValue)
        {
            foreach (var value in _dicValue)
            {
                if (value.Value == 3)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 리턴 값에 따라 나눠진다 0 또는 1 
        /// 1이면 원페어 2면 투페어 같은데 
        /// </summary>
        /// <param name="_dicValue"></param>
        /// <returns></returns>
        private int CheckPair(Dictionary<int, int> _dicValue)
        {
            int pairCount = 0;

            foreach (var data in _dicValue)
            {
                if (data.Value == 2)
                {
                    return pairCount++;
                }
            }
            return pairCount;
        }

        public int CheckWinner(List<HandRank> _listHandRank)
        {
            HandRank winHandRank = _listHandRank[0];
            for (int i = 1; i < _listHandRank.Count; i++)
            {
                if((int)winHandRank.HR > (int)_listHandRank[i].HR)
                {
                    winHandRank = _listHandRank[i];
                }
                else if(winHandRank.HR == _listHandRank[i].HR)
                {
                    if(winHandRank.RankingCard.Rank < _listHandRank[i].RankingCard.Rank)
                    {
                        winHandRank = _listHandRank[i];
                    }
                    else if(winHandRank.RankingCard.Rank == _listHandRank[i].RankingCard.Rank)
                    {
                        if((int)winHandRank.RankingCard.Suit < (int)_listHandRank[i].RankingCard.Suit)
                        {
                            winHandRank = _listHandRank[i];
                        }
                    }
                }
            }
            
        
            Console.WriteLine("결과");
            Console.WriteLine($" 승리자 패 {winHandRank.HR} {winHandRank.RankingCard.Suit} {winHandRank.RankingCard.Rank}");
            return 0;
        }

    }
}


/*
 * o 스트레이트 플러쉬: 문양 5개가 같고 5개가 랭크이미지
 * o 포카드: 같은 랭크가 4개
 * o 풀하우스: 트리플 + 페어
 * o 플러쉬: 문양 5개가 같음
 * o 스트레이트: 숫자 5개가 연속으로 이어짐
 * o 트리플: 같은 숫자 3개
 * 투페어: 같은 숫자 2개가 2개
 * 원페어: 같은 숫자 2개
 * 하이카드: 족보 없는 것 중에 가장 큰 카드
 */