using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    enum Rank
    {
        HighCard,
        OnePair,
        TwoPairs,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush
    }

    class Player
    {
        private Dictionary<Rank, List<Card>> ranks;
        private List<Card> hand;
        private bool isPlayable;
        private bool isPlaying;
        private double aggressiveness;

        public int Money { get; set; }
        public string Name { get; }

        // Constructor.
        public Player(String name)
        {
            Name = name;
            Money = 10000;
            hand = new List<Card>();
            aggressiveness = 0.5 + 0.2 * (new Random().NextDouble() - 0.5);
            ranks = new Dictionary<Rank, List<Card>>();
            isPlayable = false;
        }

        // Make this player the user.
        public void SetPlayable()
        {
            isPlayable = true;
        }

        // Add a single card to the hand.
        public void DrawCard(Card card)
        {
            hand.Add(card);
        }

        // Player chooses the amount of money they want to bet in current round.
        public int PlaceBid(int currentBid, bool doesCall)
        {
            int bidSize;
            if (isPlayable)
            {
                if (doesCall)
                    bidSize = currentBid;
                else
                {
                    Console.WriteLine("How much money do you want to bet? ");
                    do
                    {
                        bidSize = Console.Read();
                        if (bidSize > Money)
                            Console.WriteLine("You lack funds to bid so high!");
                        if (bidSize < currentBid)
                            Console.WriteLine($"You have to bid at least {currentBid}.");
                    } while (bidSize > Money || bidSize < currentBid);
                }
            }
            else
            {
                // Non-playable character bid size.
                // First bet probability: fixed 30%.
                // First bet size: any number from 0 to 50, times 10. Number (10/3) is normalizing [0, 0.3) interval into [0, 1).
                // Minimal bid: current bid
                // Maximal bid: current bid * (aggressiveness + 1)
                // Minimal bid probability: 1 - aggressiveness
                // Aggressiveness: 0 - always calls, 0.5 - calls and raises equally, 1 - only raises. Linear.

                if (currentBid == 0)
                    currentBid = 10 * (int)(50 * (10/3) * Math.Max(0, new Random().NextDouble() - 0.7));

                bidSize = (int)Math.Max(currentBid, currentBid * (aggressiveness + new Random().NextDouble()));
            }
            Money -= bidSize;
            Console.WriteLine($"{Name} bids {bidSize}");
            return bidSize;
        }

        // The player decides to fold (i.e. surrender) his current hand, excluding him from the rest of the round.
        public void Fold()
        {
            isPlaying = false;
            Console.WriteLine($"{Name} folds.");
        }

        // The player comes back to the table (another round began).
        public void Play()
        {
            isPlaying = true;
        }

        // Return the information whether a player is still playing (didn't fold nor did he go bankrupt).
        public bool IsPlaying() => isPlaying;

        // Return the cards in the player's hand.
        public List<Card> GetHand()
        {
            return hand;
        }

        // Bubble sort cards in hand by value.
        public void SortHand()
        {
            for (int i = 0; i < hand.Count - 1; i++)
                for (int j = i + 1; j < hand.Count; j++)
                    if (hand[i].Value > hand[j].Value)
                    {
                        var temp = hand[j];
                        hand[j] = hand[i];
                        hand[i] = temp;
                    }
        }

        // Search cards in hand for ranks, like pair, three of a kind or flush.
        public void UpdateRanks()
        {
            SortHand();
            var colorDictionary = new Dictionary<Color, int>();
            var valueDictionary = new Dictionary<Value, int>();
            foreach (var card in hand)
            {
                if (!colorDictionary.ContainsKey(card.Color))
                    colorDictionary.Add(card.Color, 1);
                else
                    colorDictionary[card.Color]++;

                if (!valueDictionary.ContainsKey(card.Value))
                    valueDictionary.Add(card.Value, 1);
                else
                    valueDictionary[card.Value]++;
            }

            // Flush is five cards in one color.
            foreach (var colorCount in colorDictionary)
            {
                if (colorCount.Value == 5)
                    ranks.Add(Rank.Flush, hand);
            }
            // Four of a kind, three of a kind, one pair and two pairs are self-explanatory.
            foreach (KeyValuePair<Value, int> valueCount in valueDictionary)
            {
                switch (valueCount.Value)
                {
                    case 4:
                        ranks.Add(Rank.FourOfAKind, hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                        break;
                    case 3:
                        ranks.Add(Rank.ThreeOfAKind, hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                        break;
                    case 2:
                        if (!ranks.ContainsKey(Rank.OnePair))
                            ranks.Add(Rank.OnePair, hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                        else
                        {
                            // We marge two OnePair ranks into TwoPairs and remove the earlier written OnePair.
                            List<Card> twoPairs = ranks[Rank.OnePair];
                            twoPairs.AddRange(hand.Where(card => card.Value == valueCount.Key).ToList<Card>());
                            ranks.Add(Rank.TwoPairs, twoPairs);
                            ranks.Remove(Rank.OnePair);
                        }
                        break;
                }
            }
            // Full house is one pair and three of a kind.
            if (ranks.ContainsKey(Rank.OnePair) && ranks.ContainsKey(Rank.ThreeOfAKind))
            {
                ranks.Add(Rank.FullHouse, hand);
                ranks.Remove(Rank.ThreeOfAKind);
                ranks.Remove(Rank.OnePair);
            }
            // Straight is a chain of 5 succesive cards (e.g. Four, Five, Six, Seven, Eight)
            bool areCardsConsecutive = true;
            for (int i = 0; i < hand.Count - 1; i++)
                if (hand[i].Value + 1 != hand[i + 1].Value)
                    areCardsConsecutive = false;
            if (areCardsConsecutive)
                ranks.Add(Rank.Straight, hand);
            // Straight flush is a straight and a flush.
            if (ranks.ContainsKey(Rank.Straight) && ranks.ContainsKey(Rank.Flush))
            {
                ranks.Add(Rank.StraightFlush, hand);
                ranks.Remove(Rank.Straight);
                ranks.Remove(Rank.Flush);
            }
            // High card is the card of highest value in case of no other ranks.
            if (ranks.Count == 0)
                ranks.Add(Rank.HighCard, hand.GetRange(hand.Count - 1, 1));
        }

        // Returns ranks present in player's hand.
        public Dictionary<Rank, List<Card>> GetRanks()
        {
            return ranks;
        }

        // Displays all cards in the player's hand, as card symbols.
        public void DisplayHand()
        {
            Console.WriteLine($"{Name}'s hand");
            foreach (var card in hand)
                Console.Write(card.CardSymbol() + " ");
            Console.WriteLine();
        }

        // Lists all ranks in the player's hand, with cards that form them.
        public void DisplayRanks()
        {
            foreach (var rank in ranks)
            {
                Console.Write($"{rank.Key}: ");
                foreach (var card in rank.Value)
                    Console.Write(card.CardSymbol() + " ");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

    }
}
