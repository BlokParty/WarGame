using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UnityEngine;
using PlayTable.Unity;

/** \brief PlayTable 
 * 
 */
namespace PlayTable
{
    /*interface*/
    interface IScore
    {
        float Score<T>(params T[] input);
    }

    /*enums*/
    public enum Suit { Spade, Diamond, Club, Heart, Joker };
    public enum Face { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Red, Black }
    public enum Mood { happy, sad, nervous, bored, revenge, win }
    public enum PresetName {Alice, Bob, Cathy, David, Emma, Fred, Gordan, Harry, Ian, Judah, Kevin, Lorra, Mumu, Ning, Oliver, Potter, Quinn, Ralph, Sophia, Taylor, Uber, Vivian, Wesley, Xiaohong, Yuri, Zhao }

    /**
    * \brief Holds descriptional info
    * 
    */
    public class General
    {
        public const string website = "blok.party";
        public static string RandName()
        {
            int countPresetName = Enum.GetNames(typeof(PresetName)).Length;
            return ((PresetName)UnityEngine.Random.Range(0, countPresetName)).ToString();
        }
    }

    /*Card*/
    public abstract class Card
    {
        public abstract bool isValid();
    }
    public abstract class Collection_Card
    {
        protected List<Card> cards = new List<Card>();
        public List<Card> Cards
        {
            get
            {
                return cards;
            }
        }
        public delegate void CardDelegate(Card card);
        public CardDelegate OnCardReceived;
        public CardDelegate OnCardDiscard;
        public int Count { get { return cards.Count; } }

        public virtual bool Add(Card newCard)
        {
            if (newCard == null)
                return false;
            try
            {
                cards.Add(newCard);
                if (OnCardReceived != null)
                {
                    OnCardReceived(newCard);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Discard(Card card)
        {
            try
            {
                foreach (Card c in cards)
                {
                    if (c.Equals(card))
                    {
                        cards.Remove(c);
                        if (OnCardDiscard != null)
                        {
                            OnCardDiscard(c);
                        }
                    }
                }
                return true;
            }
            catch { return false; }
        }
        public void DiscardAll()
        {
            try
            {
                foreach (Card c in cards)
                {
                    Discard(c);
                }
            }
            catch
            {
                cards = new List<Card>();
            }
        }
        public abstract void Sort();
        public Card Get(int i)
        {
            if (cards != null && cards.Count > i)
            {
                return cards[i];
            }
            return null;
        }
        public Card Get(Card card)
        {
            foreach (Card myCard in cards)
            {
                if (myCard.Equals(card)){
                    return myCard;
                }
            }
            return null;
        }
    }
    public class Collection_Playingcard : Collection_Card
    {
        public bool Has(Face face)
        {
            foreach (Playingcard card in cards)
            {
                if (card.face == face)
                {
                    return true;
                }
            }
            return false;
        }

        public override void Sort()
        {
            throw new NotImplementedException();
        }
    }
    public abstract class Deck : Collection_Card
    {
        public int remain
        {
            get
            {
                try
                {
                    return cards.Count;
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("invalid deck");
                    return -1;
                }
            }
        }

        public Card Deal()
        {
            if (remain > 0)
            {
                Card ret = cards[remain - 1];
                cards.Remove(ret);
                return ret;
            }
            else
            {
                return null;
            }
        }
        public List<Card> Deal(int num)
        {
            List<Card> ret = new List<Card>();
            while (num > 0)
            {
                ret.Add(Deal());
                num--;
            }
            return ret;
        }
        public void Deal(Hand hand, int num)
        {
            while (num > 0)
            {
                hand.Add(Deal());
                num--;
            }
        }
        public void Shuffle()
        {
            int shuffleTime = 100;
            while (shuffleTime > 0)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    System.Random rand = new System.Random();
                    int randLater = rand.Next(i, cards.Count - 1);
                    Util.Basic.Swap(cards, i, randLater);
                }
                shuffleTime--;
            }

        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Card card in cards)
            {
                sb.Append(card + "\n");
            }
            return sb.ToString();
        }
        public void AddRandomly(Card card)
        {
            if (Add(card))
            {
                System.Random rand = new System.Random();
                int index = rand.Next(0, cards.Count-1);
                Util.Basic.Swap(cards, index, cards.Count - 1);
            }
        }
    }
    public abstract class Hand : Collection_Card
    {
        public void Play(Card card, Pile pile)
        {
            Discard(card);
            pile.Add(card);
        }
    }
    public abstract class Pile : Collection_Card
    {
        public abstract void Arrange();
        public void SendToDeck(Deck deck)
        {
            foreach (Card card in cards)
            {
                deck.Add(card);
                Discard(card);
            }
        }
    }
    public class Playingcard : Card
    {
        public Suit suit;
        public Face face;

        public Playingcard(Suit s, Face f)
        {
            suit = s;
            face = f;
        }
        public static Playingcard GetCard(PTMessage ptMsg)
        {
            Playingcard card = JsonUtility.FromJson<Playingcard>(ptMsg.GetData());
            return card.isValid() ? card : null;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Playingcard;
            return other != null &&
                   this.suit == other.suit &&
                   this.face == other.face;
        }
        public override int GetHashCode()
        {
            var hashCode = -1551645312;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(suit.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(face.ToString());
            return hashCode;
        }

        public override bool isValid()
        {
            return suit >= Suit.Diamond && suit <= Suit.Joker
                && face >= Face.Ace && face <= Face.Black;
        }

        public override string ToString()
        {
            try
            {
                return JsonUtility.ToJson(this);

            }
            catch
            {
                return base.ToString();
            }
        }
    }
    public class Deck_Playingcard : Deck
    {
        public void AddStandardCards()
        {
            var suitTypes = Enum.GetValues(typeof(Suit));
            var faceTypes = Enum.GetValues(typeof(Face));
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    Playingcard card = new Playingcard(
                        (Suit)(suitTypes.GetValue(i)),
                        (Face)(faceTypes.GetValue(j)));
                    Add(card);
                }
            }
        }
        public void AddJokers()
        {
            Add(new Playingcard(Suit.Joker, Face.Red));
            Add(new Playingcard(Suit.Joker, Face.Black));
        }

        public override void Sort()
        {
            throw new NotImplementedException();
        }
    }
    public class Hand_Playingcard : Hand
    {
        public override void Sort()
        {
            throw new NotImplementedException();
        }
    }
    public class Hand_BlackJack : Hand_Playingcard, IScore
    {
        public float Score()
        {
            return Score(cards);
        }

        public float Score<T>(params T[] input)
        {
            List<Card> cards = input as List<Card>;
            float ret = 0;
            foreach (Playingcard card in cards)
            {
                ret += (int)card.face <= 10 ? (int)card.face : 10;
            }
            ret = ret <= 21 ? ret : -1;

            return ret;
        }
    }
    public class Pile_Playingcard : Pile
    {
        public override void Arrange()
        {
            throw new NotImplementedException();
        }

        public override void Sort()
        {
            throw new NotImplementedException();
        }
    }

    /*Toy*/
}

namespace PlayTable.Util
{
    /**
    * Helper class for finding probability
    * 
    */
    public static class Math
    {
        /**
        * Return by probability (50% for instance)
        * 
        * @param percentage percentage of it happening? @Jiaqi Yes
        * 
        */
        public static bool Probability(float percentage)
        {
            //return by probablity (50% for instance)
            System.Random gen = new System.Random();
            float prob = gen.Next(0, 100);
            return prob <= percentage;
        }

    }

    public static class Basic
    {
        /**
        * Generic method to swap two variables by ref
        * 
        * @param one one of the parameter to swap 
        * @param two one of the parameter to swap
        * 
        */
        public static void Swap<T>(ref T one, ref T two)
        {
            T temp;
            temp = one;
            one = two;
            two = temp;
        }

        /**
        * Generic method to swap two variables in a List
        * 
        * @param list the list, where to swap two elements 
        * @param indexA one of the parameter index to swap 
        * @param indexB one of the parameter index to swap 
        * 
        */
        public static void Swap<T>(List<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }

    public class PTLinkedNode<T>{
        public T Value;
        public PTLinkedNode<T> Next = null;

        public PTLinkedNode(T value)
        {
            Value = value;
        }
    }
}