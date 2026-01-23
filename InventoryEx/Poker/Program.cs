using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public class Program
    {
        /// <summary>
        /// 클래스를 통해서 객체를 만들고 메서드를 호출하는 것
        /// </summary>
        static void Main()
        {
            PokerManager pokerManager = new PokerManager(2);

            Deck deck = new Deck();
            deck.Suffle();

            //int playerCount = pokerManager.GetPlayerCount();
            //List<Card> listCrad = deck.DealCard(playerCount);
            //pokerManager.DealCard(listCrad);
            for (int i = 0; i < 5; i++)
            {
                pokerManager.DealCard(deck.DealCard(pokerManager.GetPlayerCount()));

            }

            pokerManager.ShowCard();

            List<HandRank> listHandRank = new List<HandRank>();
            for (int i = 0; i < pokerManager.GetPlayerCount(); i++)
            {
                listHandRank.Add(pokerManager.HandRanking(i));
            }

            Console.WriteLine();

            for (int i = 0; i < listHandRank.Count; i++)
            {
                Console.WriteLine(listHandRank[i].RankingCard);
            }

            pokerManager.CheckWinner(listHandRank);

            HandRank rankA = pokerManager.HandRanking(0);
            HandRank rankB = pokerManager.HandRanking(1);

            Console.WriteLine();
            Console.WriteLine(rankA.HR);
            Console.WriteLine(rankB.HR);
  
            Console.ReadLine();


            //deck.init;
            //deck.suffle;

            //human human1;
            //human human2;

            //human1.addCard;
            //human2.addCard;

            //human1.showCard;
            //human2.showCard;


            //human1.handranking;
            //human2.handranking;



        }
    }
}

/*
 
1. 클래스 만들기    
 최소 3개의 클래스 - deck, card, human
 추가로 포커를 관리하는 클래스 - pokerManager

2. 카드에 대한 enum 추가 


 */

/* 

포커 만들기

세븐 포커 - 각각 패 7장

먼저 파이브 포커 - 각각 패 5장
조커 없습니다.


필요한 것

1. 카드 종류
 - 스페이드 
 - 다이아 
 - 하트
 - 클로버

2. 카드 숫자
 - 2 ~ 10 J, Q, K, A

3. 모든 패는 각각 13장씩 (중복 없이)
 Deck - 13 * 4 = 52 장
 섞여있어야한다.


4. 나와 다른사람에게 골고루 나워주어야 한다.


 족보 - 나중에

 
 */