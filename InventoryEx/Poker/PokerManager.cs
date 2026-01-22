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

            // 같은 숫자의 카드끼리 묶음이 필요하다
            // 묶는다? 리스트? 딕셔너리네 딕셔너리는 foreach써야함

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

            //foreach (int data in listValue)
            //{
            //    Console.WriteLine($"value: {data}");

            //}

            if (CheckStraight(listValue) && CheckFlush(listCard))
            {
                return new HandRank(Bonus.StraightFlush);
            }

            if (CheckFourCard(dicValue))
            {
                return new HandRank(Bonus.FourCard);
            }

            return null;


        }

        private bool CheckStraight(List<int> _listValue)
        {

            int value = _listValue[0];

            for (int i = 0; i < _listValue.Count; i++)
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
    }
}
/*
 * o 스트레이트 플러쉬: 문양 5개가 같고 5개가 랭크이미지
 * 포카드: 같은 랭크가 4개
 * 풀하우스: 트리플 + 페어
 * o 플러쉬: 문양 5개가 같음
 * o 스트레이트: 숫자 5개가 연속으로 이어짐
 * 트리플: 같은 숫자 3개
 * 투페어: 같은 숫자 2개가 2개
 * 원페어: 같은 숫자 2개
 * 하이카드: 족보 없는 것 중에 가장 큰 카드
 */